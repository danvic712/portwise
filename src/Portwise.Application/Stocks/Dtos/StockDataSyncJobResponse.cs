namespace Portwise.Application.Stocks.Dtos;

/// <summary>Persisted state of one stock synchronization task.</summary>
/// <param name="Id">Task identifier.</param>
/// <param name="TriggerCode">Source that requested the synchronization.</param>
/// <param name="StatusCode">Pending, running, completed, completed with failures, or failed.</param>
/// <param name="AttemptCount">Number of execution attempts.</param>
/// <param name="CreatedAtUtc">UTC creation time.</param>
/// <param name="StartedAtUtc">UTC start time, when claimed.</param>
/// <param name="CompletedAtUtc">UTC completion time for a terminal task.</param>
/// <param name="Result">Structured result when synchronization completes.</param>
/// <param name="ErrorCode">Stable error code after an unexpected failure.</param>
/// <param name="BatchId">Batch identifier shared by tasks created for one request.</param>
/// <param name="SecurityId">Persistent security identifier for a per-stock task.</param>
/// <param name="SecurityCode">Security code resolved from the persisted security.</param>
/// <param name="ExchangeCode">Exchange code resolved from the persisted security.</param>
public sealed record StockDataSyncJobResponse(
    Guid Id,
    string TriggerCode,
    string StatusCode,
    int AttemptCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    StockFactSyncResult? Result,
    string? ErrorCode,
    Guid BatchId,
    Guid SecurityId,
    string SecurityCode,
    string ExchangeCode);
