namespace Portwise.Application.Stocks.Dtos;

/// <summary>Summary of a stock data synchronization run.</summary>
/// <param name="AttemptedStockCount">Number of stocks attempted.</param>
/// <param name="FullyCompletedStockCount">Number of fully synchronized stocks.</param>
/// <param name="PartiallyFailedStockCount">Number of stocks with partial failures.</param>
/// <param name="Failures">Failures collected during synchronization.</param>
/// <param name="CompletedAt">UTC completion timestamp.</param>
public sealed record StockDataSyncRunResult(
    int AttemptedStockCount,
    int FullyCompletedStockCount,
    int PartiallyFailedStockCount,
    IReadOnlyList<StockDataSyncFailure> Failures,
    DateTimeOffset CompletedAt);
