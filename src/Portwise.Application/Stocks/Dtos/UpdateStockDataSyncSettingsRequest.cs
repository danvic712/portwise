namespace Portwise.Application.Stocks.Dtos;

/// <summary>Updates the persisted daily stock synchronization schedule.</summary>
public sealed record UpdateStockDataSyncSettingsRequest(
    bool Enabled,
    string TimeZoneId,
    IReadOnlyList<string> RunTimes,
    long ExpectedRevision);
