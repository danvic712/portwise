namespace Portwise.Application.Stocks.Dtos;

/// <summary>
/// Adds one A-share security to the current portfolio watchlist.
/// </summary>
/// <param name="SecurityCode">The six-digit security code.</param>
/// <param name="ExchangeCode">The exchange code.</param>
/// <param name="HeldShares">The current number of held shares.</param>
public sealed record AddStockRequest(
    string SecurityCode,
    string ExchangeCode,
    int HeldShares);
