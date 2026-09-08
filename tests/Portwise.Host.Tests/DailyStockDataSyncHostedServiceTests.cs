using Portwise.Application.Stocks.Dtos;
using Portwise.Background;
using Portwise.Configuration;
using Portwise.Contracts;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;

namespace Portwise.Host.Tests;

public sealed class DailyStockDataSyncHostedServiceTests
{
    [Fact]
    public async Task ExecuteAsync_UsesInjectedTimeProviderForScheduledDelay()
    {
        var syncStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var runner = new Mock<IStockDataSyncRunner>();
        runner
            .Setup(x => x.RunAsync(
                StockDataSyncTrigger.Scheduled,
                It.IsAny<CancellationToken>()))
            .Callback(() => syncStarted.TrySetResult())
            .ReturnsAsync(new StockDataSyncExecutionResult(
                "scheduled-run",
                new StockDataSyncRunResult(1, 1, 0, [], DateTimeOffset.UnixEpoch)));
        var timeProvider = new FakeTimeProvider(
            new DateTimeOffset(2026, 9, 7, 9, 59, 0, TimeSpan.Zero));
        var service = new DailyStockDataSyncHostedService(
            runner.Object,
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

        await syncStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await service.StopAsync(CancellationToken.None);

        runner.Verify(x => x.RunAsync(
            StockDataSyncTrigger.Scheduled,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
