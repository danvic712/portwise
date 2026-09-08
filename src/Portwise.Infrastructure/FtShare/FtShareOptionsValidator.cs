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
            failures.Add("FtShare:McpEndpoint must be a valid HTTP(S) URI.");
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
            failures.Add("FtShare:RequestTimeoutSeconds must be between 1 and 300.");
        }

        if (options.MaxRetryCount is < 0 or > 5)
        {
            failures.Add("FtShare:MaxRetryCount must be between 0 and 5.");
        }

        if (options.RetryDelayMilliseconds is < 0 or > 10_000)
        {
            failures.Add("FtShare:RetryDelayMilliseconds must be between 0 and 10000.");
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
            failures.Add($"FtShare:{propertyName} is required.");
        }
    }
}
