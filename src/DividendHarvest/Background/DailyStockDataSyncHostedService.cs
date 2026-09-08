using System.Globalization;
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

        var localTime = TimeOnly.ParseExact(
            options.Value.LocalTime,
            "HH:mm",
            CultureInfo.InvariantCulture);
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZoneId);

        while (!stoppingToken.IsCancellationRequested)
        {
            var utcNow = timeProvider.GetUtcNow();
            var nextRun = DailySyncSchedule.GetNextRunUtc(
                utcNow,
                localTime,
                timeZone);
            var delay = nextRun - utcNow;

            if (delay > TimeSpan.Zero)
            {
                try
                {
                    await Task.Delay(delay, timeProvider, stoppingToken);
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
}
