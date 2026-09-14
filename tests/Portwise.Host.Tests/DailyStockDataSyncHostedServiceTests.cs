using Portwise.Application.Stocks.Contracts;
using Portwise.Application.Stocks.Dtos;
using Portwise.Background;
using Portwise.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;

namespace Portwise.Host.Tests;

public sealed class DailyStockDataSyncHostedServiceTests
{
    [Fact]
    public async Task ExecuteAsync_QueuesCurrentTradingDayAfterScheduledTime()
    {
        var jobQueued = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var queue = new Mock<IStockDataSyncJobQueue>();
        queue.Setup(x => x.EnqueueAsync(
                "scheduled", "scheduled:2026-09-07", It.IsAny<CancellationToken>()))
            .Callback(() => jobQueued.TrySetResult())
            .ReturnsAsync(new StockDataSyncJobResponse(
                Guid.CreateVersion7(), "scheduled", "pending", 0,
                DateTimeOffset.UnixEpoch, null, null, null, null));
        var timeProvider = new FakeTimeProvider(
            new DateTimeOffset(2026, 9, 7, 10, 1, 0, TimeSpan.Zero));
        var service = new DailyStockDataSyncHostedService(
            queue.Object,
            Options.Create(new DailySyncOptions
            {
                Enabled = true,
                LocalTime = "18:00",
                TimeZoneId = "Asia/Shanghai"
            }),
            timeProvider,
            NullLogger<DailyStockDataSyncHostedService>.Instance);

        await service.StartAsync(CancellationToken.None);
        await jobQueued.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await service.StopAsync(CancellationToken.None);

        queue.Verify(x => x.EnqueueAsync(
            "scheduled", "scheduled:2026-09-07", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UsesInjectedTimeProviderForScheduledDelay()
    {
        var jobQueued = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var queue = new Mock<IStockDataSyncJobQueue>();
        queue
            .Setup(x => x.EnqueueAsync(
                "scheduled",
                "scheduled:2026-09-07",
                It.IsAny<CancellationToken>()))
            .Callback(() => jobQueued.TrySetResult())
            .ReturnsAsync(new StockDataSyncJobResponse(
                Guid.CreateVersion7(), "scheduled", "pending", 0,
                DateTimeOffset.UnixEpoch, null, null, null, null));
        var timeProvider = new FakeTimeProvider(
            new DateTimeOffset(2026, 9, 7, 9, 59, 0, TimeSpan.Zero));
        var service = new DailyStockDataSyncHostedService(
            queue.Object,
            Options.Create(new DailySyncOptions
            {
                Enabled = true,
                LocalTime = "18:00",
                TimeZoneId = "Asia/Shanghai"
            }),
            timeProvider,
            NullLogger<DailyStockDataSyncHostedService>.Instance);

        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100);
        timeProvider.Advance(TimeSpan.FromMinutes(1));

        await jobQueued.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await service.StopAsync(CancellationToken.None);

        queue.Verify(x => x.EnqueueAsync(
            "scheduled",
            "scheduled:2026-09-07",
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
