using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Portwise.Application.Stocks.Contracts;
using Portwise.Application.Stocks.Dtos;
using Portwise.Background;
using Xunit;

namespace Portwise.Host.Tests;

public sealed class DailyStockDataSyncHostedServiceTests
{
    [Fact]
    public async Task ExecuteAsync_QueuesEveryConfiguredRunTimeAfterItsScheduledTime()
    {
        var queuedCount = 0;
        var queued = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var settings = new Mock<IStockDataSyncSettingsAppService>();
        settings
            .Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSettings(["09:00", "18:00"]));
        var coordinator = new Mock<IStockDataSyncCoordinator>();
        coordinator
            .Setup(x => x.EnqueueWatchlistAsync(
                "scheduled",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                if (Interlocked.Increment(ref queuedCount) == 2)
                {
                    queued.TrySetResult();
                }
            })
            .ReturnsAsync((string trigger, string? _, CancellationToken _) =>
                StockDataSyncBatchResponse.Empty(Guid.CreateVersion7(), trigger, DateTimeOffset.UnixEpoch));
        await using var provider = CreateProvider(settings.Object, coordinator.Object);
        var service = new DailyStockDataSyncHostedService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            new FakeTimeProvider(new DateTimeOffset(2026, 9, 7, 10, 1, 0, TimeSpan.Zero)),
            NullLogger<DailyStockDataSyncHostedService>.Instance);

        await service.StartAsync(CancellationToken.None);
        await queued.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await service.StopAsync(CancellationToken.None);

        coordinator.Verify(x => x.EnqueueWatchlistAsync(
            "scheduled",
            It.Is<string>(key => key == "scheduled:2026-09-07:09:00" || key == "scheduled:2026-09-07:18:00"),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_UsesInjectedTimeProviderForScheduledDelay()
    {
        var settingsRead = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var queued = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var settings = new Mock<IStockDataSyncSettingsAppService>();
        settings
            .Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
            .Callback(() => settingsRead.TrySetResult())
            .ReturnsAsync(CreateSettings(["18:00"]));
        var coordinator = new Mock<IStockDataSyncCoordinator>();
        coordinator
            .Setup(x => x.EnqueueWatchlistAsync(
                "scheduled",
                "scheduled:2026-09-07:18:00",
                It.IsAny<CancellationToken>()))
            .Callback(() => queued.TrySetResult())
            .ReturnsAsync(StockDataSyncBatchResponse.Empty(
                Guid.CreateVersion7(), "scheduled", DateTimeOffset.UnixEpoch));
        await using var provider = CreateProvider(settings.Object, coordinator.Object);
        var timeProvider = new FakeTimeProvider(
            new DateTimeOffset(2026, 9, 7, 9, 59, 0, TimeSpan.Zero));
        var service = new DailyStockDataSyncHostedService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            timeProvider,
            NullLogger<DailyStockDataSyncHostedService>.Instance);

        await service.StartAsync(CancellationToken.None);
        await settingsRead.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Task.Delay(100);
        timeProvider.Advance(TimeSpan.FromMinutes(1));

        await queued.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await service.StopAsync(CancellationToken.None);

        coordinator.Verify(x => x.EnqueueWatchlistAsync(
            "scheduled",
            "scheduled:2026-09-07:18:00",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static ServiceProvider CreateProvider(
        IStockDataSyncSettingsAppService settings,
        IStockDataSyncCoordinator coordinator)
        => new ServiceCollection()
            .AddScoped(_ => settings)
            .AddScoped(_ => coordinator)
            .BuildServiceProvider();

    private static StockDataSyncSettingsResponse CreateSettings(IReadOnlyList<string> runTimes)
        => new(
            true,
            "Asia/Shanghai",
            runTimes,
            1,
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch);
}
