using Portwise.Domain.Codes;

namespace Portwise.Domain.Models;

/// <summary>
/// Stores the persisted schedule used by the stock synchronization worker.
/// RunTimesJson contains an ordered JSON array of HH:mm values.
/// </summary>
public sealed class StockDataSyncSettings
{
    public Guid Id { get; set; } = KnownConfigurationIds.StockDataSyncSettings;

    public bool Enabled { get; set; } = true;

    public string TimeZoneId { get; set; } = "Asia/Shanghai";

    public string RunTimesJson { get; set; } = "[\"18:00\"]";

    public long Revision { get; set; } = 1;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public static StockDataSyncSettings Create(
        bool enabled,
        string timeZoneId,
        string runTimesJson,
        DateTimeOffset createdAtUtc) => new()
    {
        Enabled = enabled,
        TimeZoneId = timeZoneId,
        RunTimesJson = runTimesJson,
        Revision = 1,
        CreatedAtUtc = createdAtUtc.ToUniversalTime(),
        UpdatedAtUtc = createdAtUtc.ToUniversalTime()
    };

    public void Update(
        bool enabled,
        string timeZoneId,
        string runTimesJson,
        DateTimeOffset updatedAtUtc)
    {
        Enabled = enabled;
        TimeZoneId = timeZoneId;
        RunTimesJson = runTimesJson;
        Revision = checked(Revision + 1);
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }
}
