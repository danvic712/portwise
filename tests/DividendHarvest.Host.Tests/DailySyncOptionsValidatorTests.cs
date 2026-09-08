using DividendHarvest.Configuration;
using Xunit;

namespace DividendHarvest.Host.Tests;

public sealed class DailySyncOptionsValidatorTests
{
    [Fact]
    public void Validate_RejectsInvalidTimeAndTimeZone()
    {
        var result = new DailySyncOptionsValidator().Validate(
            null,
            new DailySyncOptions
            {
                LocalTime = "25:99",
                TimeZoneId = "Not/A_TimeZone"
            });

        Assert.True(result.Failed);
        Assert.Equal(2, result.Failures.Count());
    }

    [Fact]
    public void Validate_AcceptsConfiguredTimeAndTimeZone()
    {
        var result = new DailySyncOptionsValidator().Validate(
            null,
            new DailySyncOptions
            {
                LocalTime = "18:00",
                TimeZoneId = "Asia/Shanghai"
            });

        Assert.True(result.Succeeded);
    }
}
