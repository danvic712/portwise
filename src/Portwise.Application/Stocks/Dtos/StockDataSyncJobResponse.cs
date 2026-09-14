namespace Portwise.Application.Stocks.Dtos;

/// <summary>Persisted state of one full-watchlist synchronization request.</summary>
/// <param name="Id">Task identifier.</param>
/// <param name="TriggerCode">Source that requested the synchronization.</param>
/// <param name="StatusCode">Pending, running, completed, completed with failures, or failed.</param>
/// <param name="AttemptCount">Number of execution attempts.</param>
/// <param name="CreatedAtUtc">UTC creation time.</param>
/// <param name="StartedAtUtc">UTC start time, when claimed.</param>
/// <param name="CompletedAtUtc">UTC completion time for a terminal task.</param>
/// <param name="Result">Structured result when synchronization completes.</param>
/// <param name="ErrorCode">Stable error code after an unexpected failure.</param>
public sealed record StockDataSyncJobResponse(
    Guid Id,
    string TriggerCode,
    string StatusCode,
    int AttemptCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    StockDataSyncRunResult? Result,
    string? ErrorCode);
