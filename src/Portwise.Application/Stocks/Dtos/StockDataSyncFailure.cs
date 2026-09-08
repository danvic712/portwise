namespace Portwise.Application.Stocks.Dtos;

/// <summary>Describes one stock data synchronization failure.</summary>
/// <param name="SecurityCode">Security code that failed.</param>
/// <param name="ExchangeCode">Exchange code that failed.</param>
/// <param name="DataKind">Kind of data that failed to synchronize.</param>
/// <param name="ErrorCode">Stable failure code.</param>
/// <param name="Parameters">Structured failure parameters.</param>
public sealed record StockDataSyncFailure(
    string SecurityCode,
    string ExchangeCode,
    string DataKind,
    string ErrorCode,
    IReadOnlyDictionary<string, object?> Parameters);
