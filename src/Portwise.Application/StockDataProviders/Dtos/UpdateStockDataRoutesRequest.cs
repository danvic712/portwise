namespace Portwise.Application.StockDataProviders.Dtos;

/// <summary>
/// Requests one atomic update to all fixed stock data capability routes.
/// </summary>
public sealed record UpdateStockDataRoutesRequest(
    IReadOnlyList<UpdateStockDataRouteRequest> Routes);
