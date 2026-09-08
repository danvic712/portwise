namespace Portwise.Application.Stocks.Dtos;

/// <summary>Result of synchronizing all supported stock facts for one stock.</summary>
/// <param name="SecurityCode">Security code.</param>
/// <param name="ExchangeCode">Exchange code.</param>
/// <param name="PriceObservation">Latest price observation, when available.</param>
/// <param name="DividendEvents">Persisted dividend events.</param>
/// <param name="FinancialSnapshots">Persisted financial snapshots.</param>
/// <param name="Failures">Failures collected during synchronization.</param>
public sealed record StockFactSyncResult(
    string SecurityCode,
    string ExchangeCode,
    StockPriceObservationResult? PriceObservation,
    IReadOnlyList<StockDividendEventResult> DividendEvents,
    IReadOnlyList<StockFinancialSnapshotResult> FinancialSnapshots,
    IReadOnlyList<StockDataSyncFailure> Failures);
