using DividendHarvest.Application.Stocks;
using DividendHarvest.Configuration;
using DividendHarvest.Contracts;
using Microsoft.Extensions.Options;

namespace DividendHarvest.Background;

public sealed class DailyStockDataSyncHostedService(
    IStockDataSyncRunner syncRunner,
    IOptions<DailySyncOptions> options,
    TimeProvider timeProvider,
    ILogger<DailyStockDataSyncHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("Daily stock data synchronization is disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var timeZone = ResolveTimeZone(options.Value.TimeZoneId);
            var localTime = TimeOnly.TryParse(options.Value.LocalTime, out var parsedTime)
                ? parsedTime
                : new TimeOnly(18, 0);
            var nextRun = DailySyncSchedule.GetNextRunUtc(
                timeProvider.GetUtcNow(),
                localTime,
                timeZone);
            var delay = nextRun - timeProvider.GetUtcNow();

            if (delay > TimeSpan.Zero)
            {
                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
            }

            await RunSyncAsync(stoppingToken);
        }
    }

    private async Task RunSyncAsync(CancellationToken cancellationToken)
    {
        try
        {
            await syncRunner.RunAsync(StockDataSyncTrigger.Scheduled, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception)
        {
            // The shared runner records the failure with its run ID. Keep scheduling future runs.
        }
    }

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }
}
