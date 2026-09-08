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
            failures.Add("DailySync:LocalTime 必须使用 HH:mm 格式。");
        }

        if (string.IsNullOrWhiteSpace(options.TimeZoneId)
            || !TimeZoneInfo.TryFindSystemTimeZoneById(options.TimeZoneId, out _))
        {
            failures.Add("DailySync:TimeZoneId 必须是当前系统支持的时区标识。");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
