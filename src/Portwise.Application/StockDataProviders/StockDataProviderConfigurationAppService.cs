using Portwise.Application.Configuration;
using Portwise.Application.Configuration.Contracts;
using Portwise.Application.Configuration.Dtos;
using Portwise.Application.Exceptions;
using Portwise.Application.StockDataProviders.Contracts;
using Portwise.Application.StockDataProviders.Dtos;
using Portwise.Domain.Codes;
using Portwise.Domain.Contracts;
using Portwise.Domain.Enums;
using Portwise.Domain.Exceptions;
using Portwise.Domain.Models;

namespace Portwise.Application.StockDataProviders;

public sealed class StockDataProviderConfigurationAppService(
    IUow uow,
    ISecretProtector secretProtector,
    IStockDataProviderConnectionVerifier connectionVerifier,
    TimeProvider timeProvider) : IStockDataProviderConfigurationAppService
{
    public async Task<StockDataProvidersResponse> GetProvidersAsync(
        CancellationToken cancellationToken)
    {
        var definitions = await uow.Get<StockDataProviderDefinition>()
            .ListAsync(cancellationToken: cancellationToken);
        var providers = await uow.Get<StockDataProvider>()
            .ListAsync(cancellationToken: cancellationToken);
        var definitionsById = definitions.ToDictionary(definition => definition.Id);

        return new StockDataProvidersResponse(
            definitions
                .OrderBy(definition => definition.DisplayName, StringComparer.Ordinal)
                .Select(ToDefinitionDto)
                .ToArray(),
            providers
                .OrderBy(provider => provider.Name, StringComparer.OrdinalIgnoreCase)
                .Select(provider => ToProviderDto(provider, definitionsById[provider.ProviderDefinitionId]))
                .ToArray());
    }

    public async Task<StockDataProviderDto> CreateProviderAsync(
        CreateStockDataProviderRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!IsValidName(request.Name)
            || request.ProviderDefinitionId == Guid.Empty
            || request.Credentials is null
            || !TryResolveCreateSecret(request.Credentials, out var protectedCredentials))
        {
            throw InvalidProvider();
        }

        var definition = await uow.Get<StockDataProviderDefinition>()
            .SingleOrDefaultAsync(
                item => item.Id == request.ProviderDefinitionId,
                cancellationToken);
        if (definition is null || !definition.IsEnabled)
        {
            throw ApplicationErrors.Simple(
                ApplicationErrorCodes.StockDataProviderDefinitionUnavailable);
        }

        var repository = uow.Get<StockDataProvider>();
        if (await repository.AnyAsync(
            item => item.ProviderDefinitionId == definition.Id,
            cancellationToken))
        {
            throw ApplicationErrors.Simple(ApplicationErrorCodes.StockDataProviderAlreadyExists);
        }

        var provider = StockDataProvider.Create(
            definition.Id,
            request.Name,
            protectedCredentials,
            timeProvider.GetUtcNow());
        await repository.AddAsync(provider, cancellationToken);
        try
        {
            await uow.CommitAsync(cancellationToken);
        }
        catch (UnitOfWorkCommitException exception) when (exception.IsUniqueConstraintViolation)
        {
            throw ApplicationErrors.Simple(
                ApplicationErrorCodes.StockDataProviderAlreadyExists,
                exception);
        }

        return ToProviderDto(provider, definition);
    }

    public async Task<StockDataProviderDto> UpdateProviderAsync(
        Guid providerId,
        UpdateStockDataProviderRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (providerId == Guid.Empty
            || !IsValidName(request.Name)
            || request.ExpectedRevision < 1
            || request.Credentials is null)
        {
            throw InvalidProvider();
        }

        var provider = await GetProviderAsync(providerId, cancellationToken, asNoTracking: false);
        EnsureRevision(provider.Revision, request.ExpectedRevision);
        if (!TryResolveUpdateSecret(
            request.Credentials,
            provider.ProtectedCredentials,
            out var credentialsChanged,
            out var protectedCredentials))
        {
            throw InvalidProvider();
        }

        provider.Update(
            request.Name,
            credentialsChanged,
            protectedCredentials,
            timeProvider.GetUtcNow());
        await CommitProviderMutationAsync(cancellationToken);
        var definition = await GetDefinitionAsync(provider.ProviderDefinitionId, cancellationToken);
        return ToProviderDto(provider, definition);
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
        EnsureRevision(provider.Revision, expectedRevision);
        if (await uow.Get<StockDataRoute>().AnyAsync(
            route => route.ProviderId == providerId,
            cancellationToken))
        {
            throw ApplicationErrors.Simple(ApplicationErrorCodes.StockDataProviderInUse);
        }

        uow.Get<StockDataProvider>().Remove(provider);
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
                    ? ApplicationErrorCodes.StockDataProviderRevisionConflict
                    : ApplicationErrorCodes.StockDataProviderInUse,
                exception);
        }
    }

    public async Task<VerifyStockDataProviderResponse> VerifyProviderAsync(
        Guid providerId,
        VerifyStockDataProviderRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (providerId == Guid.Empty || request.ExpectedRevision < 1)
        {
            throw InvalidProvider();
        }

        var provider = await GetProviderAsync(providerId, cancellationToken, asNoTracking: false);
        EnsureRevision(provider.Revision, request.ExpectedRevision);
        var definition = await GetDefinitionAsync(provider.ProviderDefinitionId, cancellationToken);
        if (GetSecretStateCode(provider.ProtectedCredentials)
            != SecretCodes.Configured)
        {
            return new VerifyStockDataProviderResponse(ToProviderDto(provider, definition));
        }

        var succeeded = await connectionVerifier.VerifyAsync(providerId, cancellationToken);
        var now = timeProvider.GetUtcNow();
        if (succeeded)
        {
            provider.MarkVerificationSucceeded(now);
        }
        else
        {
            provider.MarkVerificationFailed(
                StockDataProviderConfigurationCodes.ConnectionFailed,
                now);
        }

        await CommitProviderMutationAsync(cancellationToken);
        return new VerifyStockDataProviderResponse(ToProviderDto(provider, definition));
    }

    public async Task<StockDataRoutesResponse> GetRoutesAsync(
        CancellationToken cancellationToken)
    {
        var routes = await uow.Get<StockDataRoute>()
            .ListAsync(cancellationToken: cancellationToken);
        var providers = await uow.Get<StockDataProvider>()
            .ListAsync(cancellationToken: cancellationToken);
        return ToRoutesResponse(routes, providers);
    }

    public async Task<StockDataRoutesResponse> UpdateRoutesAsync(
        UpdateStockDataRoutesRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Routes is null
            || request.Routes.Count != Enum.GetValues<StockDataCapability>().Length)
        {
            throw InvalidRoutes();
        }

        var requestedRoutes = new Dictionary<StockDataCapability, UpdateStockDataRouteRequest>();
        foreach (var route in request.Routes)
        {
            if (route is null
                || !StockDataCapabilityCodes.TryParse(route.CapabilityCode, out var capability)
                || route.ExpectedRevision < 1
                || !requestedRoutes.TryAdd(capability, route))
            {
                throw InvalidRoutes();
            }
        }

        var repository = uow.Get<StockDataRoute>();
        var routes = await repository.ListAsync(
            cancellationToken: cancellationToken,
            asNoTracking: false);
        if (routes.Count != requestedRoutes.Count)
        {
            throw InvalidRoutes();
        }

        var providers = await uow.Get<StockDataProvider>()
            .ListAsync(cancellationToken: cancellationToken);
        var providerIds = providers.Select(provider => provider.Id).ToHashSet();
        var now = timeProvider.GetUtcNow();
        foreach (var route in routes)
        {
            var requested = requestedRoutes[route.Capability];
            if (requested.ProviderId is { } providerId && !providerIds.Contains(providerId))
            {
                throw InvalidRoutes();
            }

            EnsureRevision(
                route.Revision,
                requested.ExpectedRevision,
                ApplicationErrorCodes.StockDataRouteRevisionConflict);
            route.Bind(requested.ProviderId, now);
        }

        try
        {
            await uow.CommitAsync(cancellationToken);
        }
        catch (UnitOfWorkCommitException exception) when (exception.IsConcurrencyConflict)
        {
            throw ApplicationErrors.Simple(
                ApplicationErrorCodes.StockDataRouteRevisionConflict,
                exception);
        }

        return ToRoutesResponse(routes, providers);
    }

    private async Task<StockDataProvider> GetProviderAsync(
        Guid providerId,
        CancellationToken cancellationToken,
        bool asNoTracking)
    {
        var provider = await uow.Get<StockDataProvider>()
            .SingleOrDefaultAsync(
                item => item.Id == providerId,
                cancellationToken,
                asNoTracking);
        return provider
            ?? throw ApplicationErrors.Simple(ApplicationErrorCodes.StockDataProviderNotFound);
    }

    private async Task<StockDataProviderDefinition> GetDefinitionAsync(
        Guid definitionId,
        CancellationToken cancellationToken)
    {
        var definition = await uow.Get<StockDataProviderDefinition>()
            .SingleOrDefaultAsync(item => item.Id == definitionId, cancellationToken);
        return definition
            ?? throw ApplicationErrors.Simple(
                ApplicationErrorCodes.StockDataProviderDefinitionUnavailable);
    }

    private async Task CommitProviderMutationAsync(CancellationToken cancellationToken)
    {
        try
        {
            await uow.CommitAsync(cancellationToken);
        }
        catch (UnitOfWorkCommitException exception) when (exception.IsConcurrencyConflict)
        {
            throw ApplicationErrors.Simple(
                ApplicationErrorCodes.StockDataProviderRevisionConflict,
                exception);
        }
    }

    private bool TryResolveCreateSecret(
        SecretUpdateRequest request,
        out string? protectedCredentials)
    {
        protectedCredentials = null;
        return request.Action switch
        {
            SecretCodes.Keep => true,
            SecretCodes.Clear => true,
            SecretCodes.Replace =>
                TryProtect(request.Value, out protectedCredentials),
            _ => false
        };
    }

    private bool TryResolveUpdateSecret(
        SecretUpdateRequest request,
        string? currentProtectedCredentials,
        out bool changed,
        out string? protectedCredentials)
    {
        changed = false;
        protectedCredentials = currentProtectedCredentials;
        switch (request.Action)
        {
            case SecretCodes.Keep:
                return true;
            case SecretCodes.Clear:
                changed = currentProtectedCredentials is not null;
                protectedCredentials = null;
                return true;
            case SecretCodes.Replace:
                changed = true;
                return TryProtect(request.Value, out protectedCredentials);
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
            SecretProtectionPurpose.StockDataProviderCredentials);
        return true;
    }

    private StockDataProviderDto ToProviderDto(
        StockDataProvider provider,
        StockDataProviderDefinition definition)
    {
        var secretStateCode = GetSecretStateCode(provider.ProtectedCredentials);
        return new StockDataProviderDto(
            provider.Id,
            definition.Id,
            StockDataProviderKindCodes.From(definition.ProviderKind),
            provider.Name,
            new SecretStateDto(secretStateCode),
            GetRuntimeStatusCode(secretStateCode, provider.VerificationState),
            ProviderVerificationStateCodes.From(provider.VerificationState),
            provider.LastVerifiedAtUtc,
            provider.LastVerificationErrorCode,
            provider.Revision,
            provider.UpdatedAtUtc);
    }

    private string GetSecretStateCode(string? protectedCredentials)
    {
        if (string.IsNullOrWhiteSpace(protectedCredentials))
        {
            return SecretCodes.Missing;
        }

        return secretProtector.TryUnprotect(
            protectedCredentials,
            SecretProtectionPurpose.StockDataProviderCredentials,
            out _)
            ? SecretCodes.Configured
            : SecretCodes.Unreadable;
    }

    private string GetRouteRuntimeStatusCode(
        StockDataRoute route,
        IReadOnlyDictionary<Guid, StockDataProvider> providers)
    {
        if (route.ProviderId is not { } providerId
            || !providers.TryGetValue(providerId, out var provider))
        {
            return StockDataProviderConfigurationCodes.RuntimeUnconfigured;
        }

        return GetRuntimeStatusCode(
            GetSecretStateCode(provider.ProtectedCredentials),
            provider.VerificationState);
    }

    private static string GetRuntimeStatusCode(
        string secretStateCode,
        ProviderVerificationState verificationState)
    {
        if (secretStateCode != SecretCodes.Configured)
        {
            return StockDataProviderConfigurationCodes.RuntimeUnconfigured;
        }

        return verificationState switch
        {
            ProviderVerificationState.Unverified =>
                StockDataProviderConfigurationCodes.RuntimeConfiguredUnverified,
            ProviderVerificationState.Succeeded =>
                StockDataProviderConfigurationCodes.RuntimeRecentlyVerified,
            ProviderVerificationState.Failed =>
                StockDataProviderConfigurationCodes.RuntimeCurrentlyUnavailable,
            _ => throw new ArgumentOutOfRangeException(
                nameof(verificationState), verificationState, null)
        };
    }

    private StockDataRoutesResponse ToRoutesResponse(
        IEnumerable<StockDataRoute> routes,
        IEnumerable<StockDataProvider> providers)
    {
        var providersById = providers.ToDictionary(provider => provider.Id);
        return new StockDataRoutesResponse(routes
            .OrderBy(route => route.Capability)
            .Select(route => new StockDataRouteDto(
                route.Id,
                StockDataCapabilityCodes.From(route.Capability),
                route.ProviderId,
                GetRouteRuntimeStatusCode(route, providersById),
                route.Revision,
                route.UpdatedAtUtc))
            .ToArray());
    }

    private static StockDataProviderDefinitionDto ToDefinitionDto(
        StockDataProviderDefinition definition) => new(
            definition.Id,
            StockDataProviderKindCodes.From(definition.ProviderKind),
            definition.DisplayName,
            definition.IsEnabled);

    private static bool IsValidName(string? name) =>
        !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= 100;

    private static void EnsureRevision(
        long actualRevision,
        long expectedRevision,
        string errorCode = ApplicationErrorCodes.StockDataProviderRevisionConflict)
    {
        if (actualRevision != expectedRevision)
        {
            throw ApplicationErrors.Simple(
                errorCode);
        }
    }

    private static ApplicationErrorException InvalidProvider() =>
        ApplicationErrors.Simple(ApplicationErrorCodes.StockDataProviderValidationFailed);

    private static ApplicationErrorException InvalidRoutes() =>
        ApplicationErrors.Simple(ApplicationErrorCodes.StockDataRouteValidationFailed);
}
