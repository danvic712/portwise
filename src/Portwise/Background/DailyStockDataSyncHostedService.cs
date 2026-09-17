using System.Globalization;
using Portwise.Application.Stocks;
using Portwise.Application.Stocks.Contracts;

namespace Portwise.Background;

/// <summary>
/// Polls the database-backed schedule and creates one durable task per stock
/// for every configured trading-day occurrence.
/// </summary>
public sealed class DailyStockDataSyncHostedService(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<DailyStockDataSyncHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan SettingsPollInterval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queuedOccurrences = new HashSet<string>(StringComparer.Ordinal);
        while (!stoppingToken.IsCancellationRequested)
        {
            StockDataSyncScheduleSnapshot schedule;
            try
            {
                schedule = await ReadScheduleAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    "Daily stock sync schedule could not be read. ExceptionType: {ExceptionType}.",
                    exception.GetType().Name);
                await DelayAsync(SettingsPollInterval, stoppingToken);
                continue;
            }

            if (!schedule.Enabled)
            {
                await DelayAsync(SettingsPollInterval, stoppingToken);
                continue;
            }

            var utcNow = timeProvider.GetUtcNow();
            var localNow = TimeZoneInfo.ConvertTime(utcNow, schedule.TimeZone);
            if (localNow.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
            {
                foreach (var runTime in schedule.RunTimes)
                {
                    if (localNow.TimeOfDay < runTime.ToTimeSpan())
                    {
                        continue;
                    }

                    var localDate = DateOnly.FromDateTime(localNow.DateTime);
                    var occurrenceKey = BuildOccurrenceKey(localDate, runTime);
                    if (queuedOccurrences.Add(occurrenceKey))
                    {
                        await QueueSyncAsync(localDate, runTime, stoppingToken);
                    }
                }
            }

            var nextRun = DailySyncSchedule.GetNextRunUtc(
                utcNow,
                schedule.RunTimes,
                schedule.TimeZone);
            var delay = nextRun - utcNow;
            await DelayAsync(
                delay > SettingsPollInterval ? SettingsPollInterval : delay,
                stoppingToken);

            if (queuedOccurrences.Count > 128)
            {
                queuedOccurrences.RemoveWhere(key =>
                    !key.StartsWith(
                        DateOnly.FromDateTime(localNow.DateTime)
                            .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        StringComparison.Ordinal));
            }
        }
    }

    private async Task<StockDataSyncScheduleSnapshot> ReadScheduleAsync(
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var settings = await scope.ServiceProvider
            .GetRequiredService<IStockDataSyncSettingsAppService>()
            .GetAsync(cancellationToken);
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZoneId);
        var runTimes = settings.RunTimes
            .Select(value => TimeOnly.ParseExact(
                value,
                "HH:mm",
                CultureInfo.InvariantCulture))
            .Order()
            .ToArray();
        return new StockDataSyncScheduleSnapshot(settings.Enabled, timeZone, runTimes);
    }

    private async Task QueueSyncAsync(
        DateOnly localDate,
        TimeOnly runTime,
        CancellationToken cancellationToken)
    {
        var occurrenceKey = BuildOccurrenceKey(localDate, runTime);
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                await scope.ServiceProvider
                    .GetRequiredService<IStockDataSyncCoordinator>()
                    .EnqueueWatchlistAsync(
                        "scheduled",
                        $"scheduled:{occurrenceKey}",
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
                    "Daily stock sync tasks could not be queued. ExceptionType: {ExceptionType}.",
                    exception.GetType().Name);
                await DelayAsync(TimeSpan.FromSeconds(30), cancellationToken);
            }
        }
    }

    private async Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        if (delay <= TimeSpan.Zero)
        {
            delay = TimeSpan.FromMilliseconds(100);
        }

        try
        {
            await Task.Delay(delay, timeProvider, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Normal shutdown path.
        }
    }

    private static string BuildOccurrenceKey(DateOnly date, TimeOnly time)
        => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            + ":"
            + time.ToString("HH:mm", CultureInfo.InvariantCulture);

    private sealed record StockDataSyncScheduleSnapshot(
        bool Enabled,
        TimeZoneInfo TimeZone,
        IReadOnlyList<TimeOnly> RunTimes);
}
