namespace Portwise.Application.Stocks.Dtos;

/// <summary>Request to synchronize financial snapshots for one stock.</summary>
/// <param name="SecurityCode">Six-digit security code.</param>
/// <param name="ExchangeCode">Exchange code.</param>
public sealed record SyncStockFinancialsRequest(
    string SecurityCode,
    string ExchangeCode);
