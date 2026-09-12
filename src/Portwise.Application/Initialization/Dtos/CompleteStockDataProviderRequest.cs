using Portwise.Application.Configuration.Dtos;

namespace Portwise.Application.Initialization.Dtos;

/// <summary>
/// Describes an optional stock data provider to save during onboarding.
/// </summary>
public sealed record CompleteStockDataProviderRequest(
    Guid ProviderDefinitionId,
    string Name,
    SecretUpdateRequest Credentials);
