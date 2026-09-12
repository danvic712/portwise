using Portwise.Application.Configuration;
using Portwise.Application.Configuration.Contracts;
using Portwise.Application.Configuration.Dtos;
using Portwise.Application.Exceptions;
using Portwise.Application.Inference;
using Portwise.Application.Initialization.Contracts;
using Portwise.Application.Initialization.Dtos;
using Portwise.Application.StockDataProviders;
using Portwise.Domain.Codes;
using Portwise.Domain.Contracts;
using Portwise.Domain.Enums;
using Portwise.Domain.Exceptions;
using Portwise.Domain.Models;
using PortfolioEntity = Portwise.Domain.Models.Portfolio;

namespace Portwise.Application.Initialization;

/// <summary>
/// Coordinates the first-run transaction and reports independent capability readiness.
/// </summary>
public sealed class InitializationAppService(
    IUow uow,
    ISecretProtector secretProtector,
    TimeProvider timeProvider) : IInitializationAppService
{
    private static readonly StockDataCapability[] StockCapabilities =
        Enum.GetValues<StockDataCapability>();

    private static readonly InferenceCapability[] InferenceCapabilities =
        Enum.GetValues<InferenceCapability>();

    public async Task<InitializationStatusResponse> GetAsync(
        CancellationToken cancellationToken)
    {
        var state = await uow.Get<InitializationState>()
            .SingleOrDefaultAsync(cancellationToken);
        var preference = await uow.Get<ApplicationPreference>()
            .SingleOrDefaultAsync(cancellationToken);
        var stockRoutes = await uow.Get<StockDataRoute>()
            .ListAsync(cancellationToken: cancellationToken);
        var stockProviders = await uow.Get<StockDataProvider>()
            .ListAsync(cancellationToken: cancellationToken);
        var stockDefinitions = await uow.Get<StockDataProviderDefinition>()
            .ListAsync(cancellationToken: cancellationToken);
        var inferenceRoutes = await uow.Get<InferenceRoute>()
            .ListAsync(cancellationToken: cancellationToken);
        var inferenceProviders = await uow.Get<InferenceProvider>()
            .ListAsync(cancellationToken: cancellationToken);

        var limitations = new List<CapabilityLimitationDto>(
            StockCapabilities.Length + InferenceCapabilities.Length);
        var stockProvidersById = stockProviders.ToDictionary(provider => provider.Id);
        var stockDefinitionsById = stockDefinitions.ToDictionary(definition => definition.Id);
        var stockRoutesByCapability = stockRoutes.ToDictionary(route => route.Capability);
        foreach (var capability in StockCapabilities)
        {
            limitations.Add(new CapabilityLimitationDto(
                StockDataCapabilityCodes.From(capability),
                GetStockRouteStatus(
                    stockRoutesByCapability.GetValueOrDefault(capability),
                    stockProvidersById,
                    stockDefinitionsById)));
        }

        var inferenceProvidersById = inferenceProviders.ToDictionary(provider => provider.Id);
        var inferenceRoutesByCapability = inferenceRoutes.ToDictionary(route => route.Capability);
        foreach (var capability in InferenceCapabilities)
        {
            limitations.Add(new CapabilityLimitationDto(
                InferenceCapabilityCodes.From(capability),
                GetInferenceRouteStatus(
                    inferenceRoutesByCapability.GetValueOrDefault(capability),
                    inferenceProvidersById)));
        }

        return new InitializationStatusResponse(
            state is not null,
            state?.CompletedAtUtc,
            preference is null ? null : ToPreferenceDto(preference),
            limitations);
    }

    public async Task<CompleteInitializationResponse> CompleteAsync(
        CompleteInitializationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!TryParsePreferences(request, out var language, out var theme)
            || !IsValidPortfolioName(request.PortfolioName)
            || !ValidateProviderAndRouteShape(request))
        {
            throw InvalidRequest();
        }

        if (await uow.Get<InitializationState>().AnyAsync(cancellationToken)
            || await uow.Get<PortfolioEntity>().AnyAsync(cancellationToken))
        {
            throw ApplicationErrors.Simple(ApplicationErrorCodes.InitializationAlreadyCompleted);
        }

        var now = timeProvider.GetUtcNow();
        var preference = ApplicationPreference.Create(language, theme, now);
        var portfolio = new PortfolioEntity
        {
            Id = Guid.CreateVersion7(),
            Name = request.PortfolioName.Trim(),
            CurrencyCode = CurrencyCodes.Cny
        };

        await uow.Get<ApplicationPreference>().AddAsync(preference, cancellationToken);
        await uow.Get<PortfolioEntity>().AddAsync(portfolio, cancellationToken);

        var stockProvidersByName = await SaveStockProvidersAsync(
            request.StockDataProviders,
            now,
            cancellationToken);
        await SaveStockRoutesAsync(
            request.StockDataRoutes,
            stockProvidersByName,
            now,
            cancellationToken);

        var inferenceProvidersByName = await SaveInferenceProvidersAsync(
            request.InferenceProviders,
            now,
            cancellationToken);
        await SaveInferenceRoutesAsync(
            request.InferenceRoutes,
            inferenceProvidersByName,
            now,
            cancellationToken);

        await uow.Get<InitializationState>().AddAsync(
            new InitializationState
            {
                Id = KnownConfigurationIds.InitializationState,
                CompletedAtUtc = now,
                Revision = 1
            },
            cancellationToken);

        try
        {
            await uow.CommitAsync(cancellationToken);
        }
        catch (UnitOfWorkCommitException exception) when (
            exception.IsUniqueConstraintViolation)
        {
            throw ApplicationErrors.Simple(
                ApplicationErrorCodes.InitializationAlreadyCompleted,
                exception);
        }

        return new CompleteInitializationResponse(
            await GetAsync(cancellationToken));
    }

    private async Task<IReadOnlyDictionary<string, Guid>> SaveStockProvidersAsync(
        IReadOnlyList<CompleteStockDataProviderRequest>? requests,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var byName = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        if (requests is null)
        {
            return byName;
        }

        var definitions = await uow.Get<StockDataProviderDefinition>()
            .ListAsync(cancellationToken: cancellationToken);
        var definitionsById = definitions.ToDictionary(definition => definition.Id);
        var definitionIds = new HashSet<Guid>();
        var repository = uow.Get<StockDataProvider>();
        foreach (var request in requests)
        {
            if (request is null
                || request.ProviderDefinitionId == Guid.Empty
                || !IsValidName(request.Name)
                || request.Credentials is null
                || !definitionIds.Add(request.ProviderDefinitionId)
                || !definitionsById.TryGetValue(request.ProviderDefinitionId, out var definition)
                || !definition.IsEnabled
                || !TryResolveCreateSecret(
                    request.Credentials,
                    SecretProtectionPurpose.StockDataProviderCredentials,
                    out var protectedCredentials))
            {
                throw InvalidRequest();
            }

            var name = request.Name.Trim();
            if (!byName.TryAdd(name, Guid.Empty))
            {
                throw InvalidRequest();
            }

            var provider = StockDataProvider.Create(
                definition.Id,
                name,
                protectedCredentials,
                now);
            await repository.AddAsync(provider, cancellationToken);
            byName[name] = provider.Id;
        }

        return byName;
    }

    private async Task SaveStockRoutesAsync(
        IReadOnlyList<CompleteStockDataRouteRequest>? requests,
        IReadOnlyDictionary<string, Guid> providersByName,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (requests is null)
        {
            return;
        }

        var requestedRoutes = new Dictionary<StockDataCapability, CompleteStockDataRouteRequest>();
        foreach (var request in requests)
        {
            if (request is null
                || !StockDataCapabilityCodes.TryParse(request.CapabilityCode, out var capability)
                || !requestedRoutes.TryAdd(capability, request))
            {
                throw InvalidRequest();
            }
        }

        if (requestedRoutes.Count != StockCapabilities.Length)
        {
            throw InvalidRequest();
        }

        var routes = await uow.Get<StockDataRoute>()
            .ListAsync(cancellationToken: cancellationToken, asNoTracking: false);
        if (routes.Count != StockCapabilities.Length
            || routes.Any(route => !requestedRoutes.ContainsKey(route.Capability)))
        {
            throw InvalidRequest();
        }

        foreach (var route in routes)
        {
            var requested = requestedRoutes[route.Capability];
            route.Bind(
                ResolveProvider(requested.ProviderName, providersByName),
                now);
        }
    }

    private async Task<IReadOnlyDictionary<string, Guid>> SaveInferenceProvidersAsync(
        IReadOnlyList<CompleteInferenceProviderRequest>? requests,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var byName = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        if (requests is null)
        {
            return byName;
        }

        var repository = uow.Get<InferenceProvider>();
        foreach (var request in requests)
        {
            if (request is null
                || !TryNormalizeProviderInput(request.Name, request.BaseUrl, out var name, out var baseUrl)
                || request.ApiKey is null
                || !TryResolveCreateSecret(
                    request.ApiKey,
                    SecretProtectionPurpose.InferenceProviderApiKey,
                    out var protectedApiKey))
            {
                throw InvalidRequest();
            }

            var normalizedName = InferenceProvider.NormalizeName(name);
            if (!byName.TryAdd(normalizedName, Guid.Empty))
            {
                throw InvalidRequest();
            }

            var provider = InferenceProvider.Create(name, baseUrl, protectedApiKey, now);
            await repository.AddAsync(provider, cancellationToken);
            byName[normalizedName] = provider.Id;
        }

        return byName;
    }

    private async Task SaveInferenceRoutesAsync(
        IReadOnlyList<CompleteInferenceRouteRequest>? requests,
        IReadOnlyDictionary<string, Guid> providersByName,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (requests is null)
        {
            return;
        }

        var requestedRoutes = new Dictionary<InferenceCapability, CompleteInferenceRouteRequest>();
        foreach (var request in requests)
        {
            if (request is null
                || !InferenceCapabilityCodes.TryParse(request.CapabilityCode, out var capability)
                || !IsValidInferenceBinding(request.ProviderName, request.ModelName)
                || !requestedRoutes.TryAdd(capability, request))
            {
                throw InvalidRequest();
            }
        }

        if (requestedRoutes.Count != InferenceCapabilities.Length)
        {
            throw InvalidRequest();
        }

        var routes = await uow.Get<InferenceRoute>()
            .ListAsync(cancellationToken: cancellationToken, asNoTracking: false);
        if (routes.Count != InferenceCapabilities.Length
            || routes.Any(route => !requestedRoutes.ContainsKey(route.Capability)))
        {
            throw InvalidRequest();
        }

        foreach (var route in routes)
        {
            var requested = requestedRoutes[route.Capability];
            route.Bind(
                ResolveProvider(requested.ProviderName, providersByName),
                requested.ModelName?.Trim(),
                now);
        }
    }

    private static bool ValidateProviderAndRouteShape(CompleteInitializationRequest request)
    {
        if (request.StockDataRoutes is not null
            && request.StockDataProviders is null
            && request.StockDataRoutes.Any(route => !string.IsNullOrWhiteSpace(route?.ProviderName)))
        {
            return false;
        }

        if (request.InferenceRoutes is not null
            && request.InferenceProviders is null
            && request.InferenceRoutes.Any(route => !string.IsNullOrWhiteSpace(route?.ProviderName)))
        {
            return false;
        }

        return true;
    }

    private static bool TryParsePreferences(
        CompleteInitializationRequest request,
        out ApplicationLanguage language,
        out ApplicationTheme theme)
    {
        var languageParsed = ApplicationLanguageCodes.TryParse(request.LanguageCode, out language);
        var themeParsed = ApplicationThemeCodes.TryParse(request.ThemeCode, out theme);
        return languageParsed && themeParsed;
    }

    private static bool IsValidPortfolioName(string? name) =>
        !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= 100;

    private static bool IsValidName(string? name) =>
        !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= 100;

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

    private static bool IsValidInferenceBinding(string? providerName, string? modelName) =>
        string.IsNullOrWhiteSpace(providerName)
            ? string.IsNullOrWhiteSpace(modelName)
            : !string.IsNullOrWhiteSpace(modelName) && modelName.Trim().Length <= 200;

    private bool TryResolveCreateSecret(
        SecretUpdateRequest request,
        SecretProtectionPurpose purpose,
        out string? protectedValue)
    {
        protectedValue = null;
        return request.Action switch
        {
            SecretCodes.Keep or SecretCodes.Clear => true,
            SecretCodes.Replace => TryProtect(request.Value, purpose, out protectedValue),
            _ => false
        };
    }

    private bool TryProtect(
        string? plaintext,
        SecretProtectionPurpose purpose,
        out string? protectedValue)
    {
        protectedValue = null;
        if (string.IsNullOrWhiteSpace(plaintext))
        {
            return false;
        }

        protectedValue = secretProtector.Protect(plaintext, purpose);
        return true;
    }

    private static Guid? ResolveProvider(
        string? providerName,
        IReadOnlyDictionary<string, Guid> providersByName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            return null;
        }

        return providersByName.TryGetValue(providerName.Trim(), out var providerId)
            ? providerId
            : throw InvalidRequest();
    }

    private string GetSecretState(string? protectedValue, SecretProtectionPurpose purpose)
    {
        if (string.IsNullOrWhiteSpace(protectedValue))
        {
            return SecretCodes.Missing;
        }

        return secretProtector.TryUnprotect(protectedValue, purpose, out _)
            ? SecretCodes.Configured
            : SecretCodes.Unreadable;
    }

    private string GetStockRouteStatus(
        StockDataRoute? route,
        IReadOnlyDictionary<Guid, StockDataProvider> providersById,
        IReadOnlyDictionary<Guid, StockDataProviderDefinition> definitionsById)
    {
        if (route?.ProviderId is not { } providerId
            || !providersById.TryGetValue(providerId, out var provider)
            || !definitionsById.TryGetValue(provider.ProviderDefinitionId, out var definition)
            || !definition.IsEnabled)
        {
            return StockDataProviderConfigurationCodes.RuntimeUnconfigured;
        }

        return GetRuntimeStatusCode(
            GetSecretState(provider.ProtectedCredentials, SecretProtectionPurpose.StockDataProviderCredentials),
            provider.VerificationState);
    }

    private string GetInferenceRouteStatus(
        InferenceRoute? route,
        IReadOnlyDictionary<Guid, InferenceProvider> providersById)
    {
        if (route?.ProviderId is not { } providerId
            || string.IsNullOrWhiteSpace(route.ModelName)
            || !providersById.TryGetValue(providerId, out var provider))
        {
            return InferenceConfigurationCodes.RuntimeUnconfigured;
        }

        return GetRuntimeStatusCode(
            GetSecretState(provider.ProtectedApiKey, SecretProtectionPurpose.InferenceProviderApiKey),
            provider.VerificationState);
    }

    private static string GetRuntimeStatusCode(
        string secretState,
        ProviderVerificationState verificationState)
    {
        if (secretState != SecretCodes.Configured)
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
            _ => throw new ArgumentOutOfRangeException(nameof(verificationState), verificationState, null)
        };
    }

    private static ApplicationPreferenceDto ToPreferenceDto(ApplicationPreference preference) =>
        new(
            ApplicationLanguageCodes.From(preference.Language),
            ApplicationThemeCodes.From(preference.Theme),
            preference.Revision,
            preference.UpdatedAtUtc);

    private static ApplicationErrorException InvalidRequest() =>
        ApplicationErrors.Simple(ApplicationErrorCodes.InitializationValidationFailed);
}
