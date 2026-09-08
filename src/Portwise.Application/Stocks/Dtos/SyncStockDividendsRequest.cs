namespace Portwise.Application.Stocks.Dtos;

/// <summary>Request to synchronize dividend events for one stock.</summary>
/// <param name="SecurityCode">Six-digit security code.</param>
/// <param name="ExchangeCode">Exchange code.</param>
public sealed record SyncStockDividendsRequest(
    string SecurityCode,
    string ExchangeCode);
