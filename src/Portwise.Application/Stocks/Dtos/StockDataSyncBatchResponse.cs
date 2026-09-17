namespace Portwise.Application.Stocks.Dtos;

/// <summary>Aggregated status for the per-stock tasks created by one request.</summary>
/// <param name="Id">Batch identifier.</param>
/// <param name="TriggerCode">Source that requested the synchronization.</param>
/// <param name="StatusCode">Pending, running, completed, or completed with failures.</param>
/// <param name="TotalStockCount">Number of stock tasks in the batch.</param>
/// <param name="PendingStockCount">Number of tasks waiting to run.</param>
/// <param name="RunningStockCount">Number of tasks currently running.</param>
/// <param name="CompletedStockCount">Stocks whose four fact groups completed without failures.</param>
/// <param name="FailedStockCount">Stocks with a partial or unexpected task failure.</param>
/// <param name="CreatedAtUtc">UTC creation time of the first task.</param>
/// <param name="CompletedAtUtc">UTC completion time when every task is terminal.</param>
/// <param name="Jobs">Per-stock task details.</param>
public sealed record StockDataSyncBatchResponse(
    Guid Id,
    string TriggerCode,
    string StatusCode,
    int TotalStockCount,
    int PendingStockCount,
    int RunningStockCount,
    int CompletedStockCount,
    int FailedStockCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    IReadOnlyList<StockDataSyncJobResponse> Jobs)
{
    public static StockDataSyncBatchResponse Empty(
        Guid id,
        string triggerCode,
        DateTimeOffset createdAtUtc) => new(
            id,
            triggerCode,
            "completed",
            0,
            0,
            0,
            0,
            0,
            createdAtUtc,
            createdAtUtc,
            []);
}
