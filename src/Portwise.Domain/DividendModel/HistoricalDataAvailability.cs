namespace Portwise.Domain.DividendModel;

public static class HistoricalDataAvailability
{
    public static bool WasPublicBy(
        DateTimeOffset? publishedAt,
        DateOnly dataAsOfDate)
    {
        if (dataAsOfDate == DateOnly.MinValue)
        {
            throw new ArgumentException(
                "Historical data-as-of date is required.",
                nameof(dataAsOfDate));
        }

        return publishedAt is null
            || DateOnly.FromDateTime(publishedAt.Value.UtcDateTime) <= dataAsOfDate;
    }
}
