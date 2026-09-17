namespace Portwise.Application.Stocks.Dtos;

/// <summary>Persisted daily stock synchronization schedule.</summary>
public sealed record StockDataSyncSettingsResponse(
    bool Enabled,
    string TimeZoneId,
    IReadOnlyList<string> RunTimes,
    long Revision,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? NextRunAtUtc);
