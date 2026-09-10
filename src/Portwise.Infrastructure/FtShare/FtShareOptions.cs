namespace Portwise.Infrastructure.FtShare;

public sealed class FtShareOptions
{
    private const int MaxHttpRequestsPerExchange = 4;

    public const string SectionName = "FtShare";

    public string McpEndpoint { get; set; } = string.Empty;

    public string StockProfileToolName { get; set; } = "get_stock_profile";

    public string StockMarketDataToolName { get; set; } = "get_stock_market_data";

    public string StockDividendEventsToolName { get; set; } = "get_stock_dividend_events";

    public string StockFinancialSnapshotsToolName { get; set; } =
        "get_stock_financial_snapshots";

    public string SecurityCodeArgumentName { get; set; } = "security_code";

    public string ExchangeCodeArgumentName { get; set; } = "exchange_code";

    public int RequestTimeoutSeconds { get; set; } = 30;

    public int MaxRetryCount { get; set; } = 2;

    public int RetryDelayMilliseconds { get; set; } = 250;

    public TimeSpan RequestTimeout => TimeSpan.FromSeconds(RequestTimeoutSeconds);

    public TimeSpan HttpRequestTimeout => TimeSpan.FromTicks(
        checked(RequestTimeout.Ticks * ((long)MaxRetryCount + 1)));

    // A single MCP exchange may contain several HTTP attempts (session
    // initialization, the initialized notification, tool invocation, and
    // stream completion). Keep one bounded deadline for that exchange while
    // allowing each HTTP request to use its own retry budget. Backoff delays
    // consume this hard cap rather than extending it, so a call cannot outlive
    // its configured budget.
    public TimeSpan OperationTimeout => TimeSpan.FromTicks(
        checked(HttpRequestTimeout.Ticks * MaxHttpRequestsPerExchange));

    public TimeSpan RetryDelay => TimeSpan.FromMilliseconds(RetryDelayMilliseconds);
}
