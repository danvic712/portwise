using Portwise.Application.Configuration;
using Portwise.Application.Configuration.Contracts;
using Portwise.Application.Configuration.Dtos;
using Portwise.Application.Exceptions;
using Portwise.Application.Inference.Contracts;
using Portwise.Application.Inference.Dtos;
using Portwise.Domain.Codes;
using Portwise.Domain.Contracts;
using Portwise.Domain.Enums;
using Portwise.Domain.Exceptions;
using Portwise.Domain.Models;

namespace Portwise.Application.Inference;

public sealed class InferenceConfigurationAppService(
    IUow uow,
    ISecretProtector secretProtector,
    IInferenceProviderConnectionVerifier connectionVerifier,
    TimeProvider timeProvider) : IInferenceConfigurationAppService
{
    public async Task<InferenceProvidersResponse> GetProvidersAsync(
        CancellationToken cancellationToken)
    {
        var providers = await uow.Get<InferenceProvider>()
            .ListAsync(cancellationToken: cancellationToken);
        return new InferenceProvidersResponse(providers
            .OrderBy(provider => provider.Name, StringComparer.OrdinalIgnoreCase)
            .Select(ToProviderDto)
            .ToArray());
    }

    public async Task<InferenceProviderDto> CreateProviderAsync(
        CreateInferenceProviderRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryNormalizeProviderInput(request.Name, request.BaseUrl, out var name, out var baseUrl)
            || request.ApiKey is null
            || !TryResolveCreateSecret(request.ApiKey, out var protectedApiKey))
        {
            throw InvalidProvider();
        }

        var normalizedName = InferenceProvider.NormalizeName(name);
        var repository = uow.Get<InferenceProvider>();
        if (await repository.AnyAsync(
            provider => provider.NormalizedName == normalizedName,
            cancellationToken))
        {
            throw ApplicationErrors.Simple(ApplicationErrorCodes.InferenceProviderNameConflict);
        }

        var provider = InferenceProvider.Create(
            name,
            baseUrl,
            protectedApiKey,
            timeProvider.GetUtcNow());
        await repository.AddAsync(provider, cancellationToken);
        try
        {
            await uow.CommitAsync(cancellationToken);
        }
        catch (UnitOfWorkCommitException exception) when (exception.IsUniqueConstraintViolation)
        {
            throw ApplicationErrors.Simple(
                ApplicationErrorCodes.InferenceProviderNameConflict,
                exception);
        }

        return ToProviderDto(provider);
    }

    public async Task<InferenceProviderDto> UpdateProviderAsync(
        Guid providerId,
        UpdateInferenceProviderRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (providerId == Guid.Empty
            || request.ExpectedRevision < 1
            || request.ApiKey is null
            || !TryNormalizeProviderInput(
                request.Name,
                request.BaseUrl,
                out var name,
                out var baseUrl))
        {
            throw InvalidProvider();
        }

        var provider = await GetProviderAsync(providerId, cancellationToken, asNoTracking: false);
        EnsureProviderRevision(provider.Revision, request.ExpectedRevision);
        var normalizedName = InferenceProvider.NormalizeName(name);
        if (await uow.Get<InferenceProvider>().AnyAsync(
            item => item.Id != providerId && item.NormalizedName == normalizedName,
            cancellationToken))
        {
            throw ApplicationErrors.Simple(ApplicationErrorCodes.InferenceProviderNameConflict);
        }

        if (!TryResolveUpdateSecret(
            request.ApiKey,
            provider.ProtectedApiKey,
            out var secretChanged,
            out var protectedApiKey))
        {
            throw InvalidProvider();
        }

        var connectionChanged = secretChanged
            || !string.Equals(provider.BaseUrl, baseUrl, StringComparison.Ordinal);
        provider.Update(
            name,
            baseUrl,
            connectionChanged,
            protectedApiKey,
            timeProvider.GetUtcNow());
        await CommitProviderMutationAsync(cancellationToken);
        return ToProviderDto(provider);
    }

    public async Task DeleteProviderAsync(
        Guid providerId,
        long expectedRevision,
        CancellationToken cancellationToken)
    {
        if (providerId == Guid.Empty || expectedRevision < 1)
        {
            throw InvalidProvider();
        }

        var provider = await GetProviderAsync(providerId, cancellationToken, asNoTracking: false);
        EnsureProviderRevision(provider.Revision, expectedRevision);
        if (await uow.Get<InferenceRoute>().AnyAsync(
            route => route.ProviderId == providerId,
            cancellationToken))
        {
            throw ApplicationErrors.Simple(ApplicationErrorCodes.InferenceProviderInUse);
        }

        uow.Get<InferenceProvider>().Remove(provider);
        try
        {
            await uow.CommitAsync(cancellationToken);
        }
        catch (UnitOfWorkCommitException exception)
            when (exception.IsConcurrencyConflict
                || exception.IsForeignKeyConstraintViolation)
        {
            throw ApplicationErrors.Simple(
                exception.IsConcurrencyConflict
                    ? ApplicationErrorCodes.InferenceProviderRevisionConflict
                    : ApplicationErrorCodes.InferenceProviderInUse,
                exception);
        }
    }

    public async Task<VerifyInferenceProviderResponse> VerifyProviderAsync(
        Guid providerId,
        VerifyInferenceProviderRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (providerId == Guid.Empty || request.ExpectedRevision < 1)
        {
            throw InvalidProvider();
        }

        var provider = await GetProviderAsync(providerId, cancellationToken, asNoTracking: false);
        EnsureProviderRevision(provider.Revision, request.ExpectedRevision);
        if (GetSecretStateCode(provider.ProtectedApiKey) != SecretCodes.Configured)
        {
            return new VerifyInferenceProviderResponse(ToProviderDto(provider));
        }

        var succeeded = await connectionVerifier.VerifyAsync(providerId, cancellationToken);
        if (succeeded is null)
        {
            return new VerifyInferenceProviderResponse(ToProviderDto(provider));
        }

        var now = timeProvider.GetUtcNow();
        if (succeeded.Value)
        {
            provider.MarkVerificationSucceeded(now);
        }
        else
        {
            provider.MarkVerificationFailed(InferenceConfigurationCodes.ConnectionFailed, now);
        }

        await CommitProviderMutationAsync(cancellationToken);
        return new VerifyInferenceProviderResponse(ToProviderDto(provider));
    }

    public async Task<InferenceRoutesResponse> GetRoutesAsync(
        CancellationToken cancellationToken)
    {
        var routes = await uow.Get<InferenceRoute>()
            .ListAsync(cancellationToken: cancellationToken);
        var providers = await uow.Get<InferenceProvider>()
            .ListAsync(cancellationToken: cancellationToken);
        return ToRoutesResponse(routes, providers);
    }

    public async Task<InferenceRoutesResponse> UpdateRoutesAsync(
        UpdateInferenceRoutesRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Routes is null
            || request.Routes.Count != Enum.GetValues<InferenceCapability>().Length)
        {
            throw InvalidRoutes();
        }

        var requestedRoutes = new Dictionary<InferenceCapability, UpdateInferenceRouteRequest>();
        foreach (var route in request.Routes)
        {
            if (route is null
                || !InferenceCapabilityCodes.TryParse(route.CapabilityCode, out var capability)
                || route.ExpectedRevision < 1
                || !IsValidBinding(route.ProviderId, route.ModelName)
                || !requestedRoutes.TryAdd(capability, route))
            {
                throw InvalidRoutes();
            }
        }

        var routes = await uow.Get<InferenceRoute>()
            .ListAsync(cancellationToken: cancellationToken, asNoTracking: false);
        if (routes.Count != requestedRoutes.Count)
        {
            throw InvalidRoutes();
        }

        var providers = await uow.Get<InferenceProvider>()
            .ListAsync(cancellationToken: cancellationToken, asNoTracking: false);
        var providerIds = providers.Select(provider => provider.Id).ToHashSet();
        var providersById = providers.ToDictionary(provider => provider.Id);
        var providersWhoseModelsChanged = new HashSet<Guid>();
        var now = timeProvider.GetUtcNow();
        foreach (var route in routes)
        {
            var requested = requestedRoutes[route.Capability];
            if (requested.ProviderId is { } providerId && !providerIds.Contains(providerId))
            {
                throw InvalidRoutes();
            }

            if (route.Revision != requested.ExpectedRevision)
            {
                throw ApplicationErrors.Simple(
                    ApplicationErrorCodes.InferenceRouteRevisionConflict);
            }

            if (requested.ProviderId is { } requestedProviderId
                && (route.ProviderId != requestedProviderId
                    || !string.Equals(
                        route.ModelName,
                        requested.ModelName?.Trim(),
                        StringComparison.Ordinal)))
            {
                providersWhoseModelsChanged.Add(requestedProviderId);
            }

            route.Bind(requested.ProviderId, requested.ModelName?.Trim(), now);
        }

        foreach (var providerId in providersWhoseModelsChanged)
        {
            providersById[providerId].ResetVerification(now);
        }

        try
        {
            await uow.CommitAsync(cancellationToken);
        }
        catch (UnitOfWorkCommitException exception) when (exception.IsConcurrencyConflict)
        {
            throw ApplicationErrors.Simple(
                ApplicationErrorCodes.InferenceRouteRevisionConflict,
                exception);
        }

        return ToRoutesResponse(routes, providers);
    }

    private async Task<InferenceProvider> GetProviderAsync(
        Guid providerId,
        CancellationToken cancellationToken,
        bool asNoTracking)
    {
        var provider = await uow.Get<InferenceProvider>()
            .SingleOrDefaultAsync(
                item => item.Id == providerId,
                cancellationToken,
                asNoTracking);
        return provider
            ?? throw ApplicationErrors.Simple(ApplicationErrorCodes.InferenceProviderNotFound);
    }

    private async Task CommitProviderMutationAsync(CancellationToken cancellationToken)
    {
        try
        {
            await uow.CommitAsync(cancellationToken);
        }
        catch (UnitOfWorkCommitException exception) when (
            exception.IsConcurrencyConflict || exception.IsUniqueConstraintViolation)
        {
            throw ApplicationErrors.Simple(
                exception.IsConcurrencyConflict
                    ? ApplicationErrorCodes.InferenceProviderRevisionConflict
                    : ApplicationErrorCodes.InferenceProviderNameConflict,
                exception);
        }
    }

    private bool TryResolveCreateSecret(
        SecretUpdateRequest request,
        out string? protectedApiKey)
    {
        protectedApiKey = null;
        return request.Action switch
        {
            SecretCodes.Keep => true,
            SecretCodes.Clear => true,
            SecretCodes.Replace => TryProtect(request.Value, out protectedApiKey),
            _ => false
        };
    }

    private bool TryResolveUpdateSecret(
        SecretUpdateRequest request,
        string? currentProtectedApiKey,
        out bool changed,
        out string? protectedApiKey)
    {
        changed = false;
        protectedApiKey = currentProtectedApiKey;
        switch (request.Action)
        {
            case SecretCodes.Keep:
                return true;
            case SecretCodes.Clear:
                changed = currentProtectedApiKey is not null;
                protectedApiKey = null;
                return true;
            case SecretCodes.Replace:
                changed = true;
                return TryProtect(request.Value, out protectedApiKey);
            default:
                return false;
        }
    }

    private bool TryProtect(string? plaintext, out string? protectedValue)
    {
        protectedValue = null;
        if (string.IsNullOrWhiteSpace(plaintext))
        {
            return false;
        }

        protectedValue = secretProtector.Protect(
            plaintext,
            SecretProtectionPurpose.InferenceProviderApiKey);
        return true;
    }

    private InferenceProviderDto ToProviderDto(InferenceProvider provider)
    {
        var secretStateCode = GetSecretStateCode(provider.ProtectedApiKey);
        return new InferenceProviderDto(
            provider.Id,
            provider.Name,
            InferenceProviderTypeCodes.From(provider.ProviderType),
            provider.BaseUrl,
            new SecretStateDto(secretStateCode),
            GetProviderRuntimeStatusCode(secretStateCode, provider.VerificationState),
            ProviderVerificationStateCodes.From(provider.VerificationState),
            provider.LastVerifiedAtUtc,
            provider.LastVerificationErrorCode,
            provider.Revision,
            provider.UpdatedAtUtc);
    }

    private string GetSecretStateCode(string? protectedApiKey)
    {
        if (string.IsNullOrWhiteSpace(protectedApiKey))
        {
            return SecretCodes.Missing;
        }

        return secretProtector.TryUnprotect(
            protectedApiKey,
            SecretProtectionPurpose.InferenceProviderApiKey,
            out _)
            ? SecretCodes.Configured
            : SecretCodes.Unreadable;
    }

    private InferenceRoutesResponse ToRoutesResponse(
        IEnumerable<InferenceRoute> routes,
        IEnumerable<InferenceProvider> providers)
    {
        var providersById = providers.ToDictionary(provider => provider.Id);
        return new InferenceRoutesResponse(routes
            .OrderBy(route => route.Capability)
            .Select(route => new InferenceRouteDto(
                route.Id,
                InferenceCapabilityCodes.From(route.Capability),
                route.ProviderId,
                route.ModelName,
                GetRouteRuntimeStatusCode(route, providersById),
                route.Revision,
                route.UpdatedAtUtc))
            .ToArray());
    }

    private string GetRouteRuntimeStatusCode(
        InferenceRoute route,
        IReadOnlyDictionary<Guid, InferenceProvider> providers)
    {
        if (route.ProviderId is not { } providerId
            || string.IsNullOrWhiteSpace(route.ModelName)
            || !providers.TryGetValue(providerId, out var provider))
        {
            return InferenceConfigurationCodes.RuntimeUnconfigured;
        }

        return GetProviderRuntimeStatusCode(
            GetSecretStateCode(provider.ProtectedApiKey),
            provider.VerificationState);
    }

    private static string GetProviderRuntimeStatusCode(
        string secretStateCode,
        ProviderVerificationState verificationState)
    {
        if (secretStateCode != SecretCodes.Configured)
        {
            return InferenceConfigurationCodes.RuntimeUnconfigured;
        }

        return verificationState switch
        {
            ProviderVerificationState.Unverified =>
                InferenceConfigurationCodes.RuntimeConfiguredUnverified,
            ProviderVerificationState.Succeeded =>
                InferenceConfigurationCodes.RuntimeRecentlyVerified,
            ProviderVerificationState.Failed =>
                InferenceConfigurationCodes.RuntimeCurrentlyUnavailable,
            _ => throw new ArgumentOutOfRangeException(
                nameof(verificationState), verificationState, null)
        };
    }

    private static bool TryNormalizeProviderInput(
        string? requestedName,
        string? requestedBaseUrl,
        out string name,
        out string baseUrl)
    {
        name = requestedName?.Trim() ?? string.Empty;
        baseUrl = requestedBaseUrl?.Trim().TrimEnd('/') ?? string.Empty;
        return name.Length is > 0 and <= 100
            && baseUrl.Length is > 0 and <= 500
            && Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)
            && uri.Scheme is "http" or "https";
    }

    private static bool IsValidBinding(Guid? providerId, string? modelName) =>
        providerId is null
            ? modelName is null
            : !string.IsNullOrWhiteSpace(modelName) && modelName.Trim().Length <= 200;

    private static void EnsureProviderRevision(long actualRevision, long expectedRevision)
    {
        if (actualRevision != expectedRevision)
        {
            throw ApplicationErrors.Simple(
                ApplicationErrorCodes.InferenceProviderRevisionConflict);
        }
    }

    private static ApplicationErrorException InvalidProvider() =>
        ApplicationErrors.Simple(ApplicationErrorCodes.InferenceProviderValidationFailed);

    private static ApplicationErrorException InvalidRoutes() =>
        ApplicationErrors.Simple(ApplicationErrorCodes.InferenceRouteValidationFailed);
}
