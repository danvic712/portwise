using System.Globalization;
using Microsoft.Extensions.Options;

namespace Portwise.Configuration;

internal sealed class DailySyncOptionsValidator : IValidateOptions<DailySyncOptions>
{
    public ValidateOptionsResult Validate(string? name, DailySyncOptions options)
    {
        var failures = new List<string>();
        if (!TimeOnly.TryParseExact(
                options.LocalTime,
                "HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _))
        {
            failures.Add("DailySync:LocalTime must use the HH:mm format.");
        }

        if (string.IsNullOrWhiteSpace(options.TimeZoneId)
            || !TimeZoneInfo.TryFindSystemTimeZoneById(options.TimeZoneId, out _))
        {
            failures.Add("DailySync:TimeZoneId must identify a time zone supported by the host.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
