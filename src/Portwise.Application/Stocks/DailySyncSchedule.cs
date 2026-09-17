namespace Portwise.Application.Stocks;

public static class DailySyncSchedule
{
    public static DateTimeOffset GetNextRunUtc(
        DateTimeOffset utcNow,
        TimeOnly localRunTime,
        TimeZoneInfo timeZone)
        => GetNextRunUtc(utcNow, [localRunTime], timeZone);

    public static DateTimeOffset GetNextRunUtc(
        DateTimeOffset utcNow,
        IReadOnlyList<TimeOnly> localRunTimes,
        TimeZoneInfo timeZone)
    {
        ArgumentNullException.ThrowIfNull(timeZone);
        ArgumentNullException.ThrowIfNull(localRunTimes);
        if (localRunTimes.Count == 0)
        {
            throw new ArgumentException("At least one local run time is required.", nameof(localRunTimes));
        }

        var localNow = TimeZoneInfo.ConvertTime(utcNow, timeZone);
        var nextDate = DateOnly.FromDateTime(localNow.DateTime);
        var runTimes = localRunTimes.Order().ToArray();
        while (true)
        {
            if (nextDate.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
            {
                foreach (var localRunTime in runTimes)
                {
                    if (nextDate != DateOnly.FromDateTime(localNow.DateTime)
                        || localNow.TimeOfDay < localRunTime.ToTimeSpan())
                    {
                        var nextLocal = nextDate.ToDateTime(localRunTime, DateTimeKind.Unspecified);
                        if (timeZone.IsInvalidTime(nextLocal))
                        {
                            continue;
                        }

                        var nextUtc = TimeZoneInfo.ConvertTimeToUtc(nextLocal, timeZone);
                        return new DateTimeOffset(nextUtc, TimeSpan.Zero);
                    }
                }
            }

            nextDate = nextDate.AddDays(1);
        }
    }
}
