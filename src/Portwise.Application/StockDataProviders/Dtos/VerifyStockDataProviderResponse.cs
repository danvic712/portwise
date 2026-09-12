namespace Portwise.Application.StockDataProviders.Dtos;

/// <summary>
/// Returns the provider state recorded after a connection verification attempt.
/// </summary>
public sealed record VerifyStockDataProviderResponse(StockDataProviderDto Provider);
