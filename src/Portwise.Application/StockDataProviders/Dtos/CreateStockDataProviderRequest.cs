using Portwise.Application.Configuration.Dtos;

namespace Portwise.Application.StockDataProviders.Dtos;

/// <summary>
/// Requests creation of one stock data provider instance.
/// </summary>
public sealed record CreateStockDataProviderRequest(
    Guid ProviderDefinitionId,
    string Name,
    SecretUpdateRequest Credentials);
