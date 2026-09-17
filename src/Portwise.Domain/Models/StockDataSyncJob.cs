namespace Portwise.Domain.Models;

public sealed class StockDataSyncJob
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public string TriggerCode { get; set; } = string.Empty;

    public string StatusCode { get; set; } = "pending";

    public string? DeduplicationKey { get; set; }

    public Guid BatchId { get; set; }

    public Guid SecurityId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset AvailableAtUtc { get; set; }

    public DateTimeOffset? StartedAtUtc { get; set; }

    public DateTimeOffset? LeaseExpiresAtUtc { get; set; }

    public Guid? LeaseOwnerId { get; set; }

    public DateTimeOffset? CompletedAtUtc { get; set; }

    public int AttemptCount { get; set; }

    public string? ResultJson { get; set; }

    public string? ErrorCode { get; set; }

    public static StockDataSyncJob Create(
        string triggerCode,
        Guid batchId,
        Guid securityId,
        DateTimeOffset now,
        string? deduplicationKey = null) => new()
    {
        TriggerCode = triggerCode,
        BatchId = batchId,
        SecurityId = securityId,
        DeduplicationKey = deduplicationKey,
        CreatedAtUtc = now,
        AvailableAtUtc = now
    };
}
