namespace Portwise.Application.StockDataProviders.Dtos;

/// <summary>
/// Requests connection verification for the last observed provider revision.
/// </summary>
public sealed record VerifyStockDataProviderRequest(long ExpectedRevision);
