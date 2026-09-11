namespace Portwise.Domain.Models;

/// <summary>
/// Stores migration-managed, non-sensitive FTShare runtime settings.
/// </summary>
public sealed class FtShareProviderSettings
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid ProviderDefinitionId { get; set; }

    public string McpEndpoint { get; set; } = string.Empty;

    public string StockProfileToolName { get; set; } = string.Empty;

    public string StockMarketDataToolName { get; set; } = string.Empty;

    public string StockDividendEventsToolName { get; set; } = string.Empty;

    public string StockFinancialSnapshotsToolName { get; set; } = string.Empty;

    public string SecurityCodeArgumentName { get; set; } = string.Empty;

    public string ExchangeCodeArgumentName { get; set; } = string.Empty;

    public int RequestTimeoutSeconds { get; set; }

    public int MaxRetryCount { get; set; }

    public int RetryDelayMilliseconds { get; set; }
}
