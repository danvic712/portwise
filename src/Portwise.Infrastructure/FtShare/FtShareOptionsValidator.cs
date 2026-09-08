using Microsoft.Extensions.Options;

namespace Portwise.Infrastructure.FtShare;

internal sealed class FtShareOptionsValidator : IValidateOptions<FtShareOptions>
{
    public ValidateOptionsResult Validate(string? name, FtShareOptions options)
    {
        var failures = new List<string>();
        if (!Uri.TryCreate(options.McpEndpoint, UriKind.Absolute, out var endpoint)
            || endpoint.Scheme is not ("http" or "https"))
        {
            failures.Add("FtShare:McpEndpoint 必须是有效的 HTTP(S) 地址。");
        }

        AddRequiredFailure(failures, options.StockProfileToolName, "StockProfileToolName");
        AddRequiredFailure(failures, options.StockMarketDataToolName, "StockMarketDataToolName");
        AddRequiredFailure(
            failures,
            options.StockDividendEventsToolName,
            "StockDividendEventsToolName");
        AddRequiredFailure(
            failures,
            options.StockFinancialSnapshotsToolName,
            "StockFinancialSnapshotsToolName");
        AddRequiredFailure(
            failures,
            options.SecurityCodeArgumentName,
            "SecurityCodeArgumentName");
        AddRequiredFailure(
            failures,
            options.ExchangeCodeArgumentName,
            "ExchangeCodeArgumentName");

        if (options.RequestTimeoutSeconds is < 1 or > 300)
        {
            failures.Add("FtShare:RequestTimeoutSeconds 必须在 1 到 300 之间。");
        }

        if (options.MaxRetryCount is < 0 or > 5)
        {
            failures.Add("FtShare:MaxRetryCount 必须在 0 到 5 之间。");
        }

        if (options.RetryDelayMilliseconds is < 0 or > 10_000)
        {
            failures.Add("FtShare:RetryDelayMilliseconds 必须在 0 到 10000 之间。");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void AddRequiredFailure(
        ICollection<string> failures,
        string value,
        string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            failures.Add($"FtShare:{propertyName} 不能为空。");
        }
    }
}
