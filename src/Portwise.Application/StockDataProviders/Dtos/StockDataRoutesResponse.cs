namespace Portwise.Application.StockDataProviders.Dtos;

/// <summary>
/// Returns all fixed stock data capability routes.
/// </summary>
public sealed record StockDataRoutesResponse(IReadOnlyList<StockDataRouteDto> Routes);
