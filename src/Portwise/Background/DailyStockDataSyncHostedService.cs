using System.Globalization;
using Portwise.Application.Stocks;
using Portwise.Application.Stocks.Contracts;
using Portwise.Configuration;
using Microsoft.Extensions.Options;

namespace Portwise.Background;

public sealed class DailyStockDataSyncHostedService(
    IStockDataSyncJobQueue queue,
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

        var localNow = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), timeZone);
        if (localNow.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)
            && localNow.TimeOfDay >= localTime.ToTimeSpan())
        {
            await QueueSyncAsync(
                DateOnly.FromDateTime(localNow.DateTime),
                stoppingToken);
        }

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

            var localDate = DateOnly.FromDateTime(
                TimeZoneInfo.ConvertTime(nextRun, timeZone).DateTime);
            await QueueSyncAsync(localDate, stoppingToken);
        }
    }

    private async Task QueueSyncAsync(
        DateOnly localDate,
        CancellationToken cancellationToken)
    {
        var deduplicationKey = "scheduled:"
            + localDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await queue.EnqueueAsync(
                    "scheduled",
                    deduplicationKey,
                    cancellationToken);
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    "Daily stock sync job could not be queued. ExceptionType: {ExceptionType}.",
                    exception.GetType().Name);
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), timeProvider, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }
    }
}
