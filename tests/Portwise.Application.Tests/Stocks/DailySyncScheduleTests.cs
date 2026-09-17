using Portwise.Application.Stocks;
using Xunit;

namespace Portwise.Application.Tests.Stocks;

public sealed class DailySyncScheduleTests
{
    [Fact]
    public void GetNextRunUtc_skips_weekend_and_returns_next_local_run_time()
    {
        var utcNow = new DateTimeOffset(
            2026,
            9,
            4,
            20,
            0,
            0,
            TimeSpan.Zero);

        var nextRun = DailySyncSchedule.GetNextRunUtc(
            utcNow,
            [new TimeOnly(9, 0), new TimeOnly(18, 0)],
            TimeZoneInfo.Utc);

        Assert.Equal(
            new DateTimeOffset(2026, 9, 7, 9, 0, 0, TimeSpan.Zero),
            nextRun);
    }

    [Fact]
    public void GetNextRunUtc_returns_the_next_time_on_the_same_day()
    {
        var utcNow = new DateTimeOffset(
            2026,
            9,
            2,
            9,
            0,
            0,
            TimeSpan.Zero);

        var nextRun = DailySyncSchedule.GetNextRunUtc(
            utcNow,
            [new TimeOnly(9, 0), new TimeOnly(18, 0)],
            TimeZoneInfo.Utc);

        Assert.Equal(
            new DateTimeOffset(2026, 9, 2, 18, 0, 0, TimeSpan.Zero),
            nextRun);
    }

    [Fact]
    public void GetNextRunUtc_orders_unsorted_times_before_selecting()
    {
        var utcNow = new DateTimeOffset(
            2026,
            9,
            2,
            10,
            0,
            0,
            TimeSpan.Zero);

        var nextRun = DailySyncSchedule.GetNextRunUtc(
            utcNow,
            [new TimeOnly(18, 0), new TimeOnly(12, 0)],
            TimeZoneInfo.Utc);

        Assert.Equal(
            new DateTimeOffset(2026, 9, 2, 12, 0, 0, TimeSpan.Zero),
            nextRun);
    }
}
