namespace Portwise.Application.StockDataProviders.Dtos;

/// <summary>
/// Requests one optimistic-concurrency stock data route binding update.
/// </summary>
public sealed record UpdateStockDataRouteRequest(
    string CapabilityCode,
    Guid? ProviderId,
    long ExpectedRevision);
