using Portwise.Application.Configuration.Dtos;

namespace Portwise.Application.StockDataProviders.Dtos;

/// <summary>
/// Requests an optimistic-concurrency update to a stock data provider.
/// </summary>
public sealed record UpdateStockDataProviderRequest(
    string Name,
    SecretUpdateRequest Credentials,
    long ExpectedRevision);
