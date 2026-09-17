using Moq;
using Portwise.Application.Stocks;
using Portwise.Application.Stocks.Contracts;
using Portwise.Application.Stocks.Dtos;
using Xunit;

namespace Portwise.Application.Tests.Stocks;

public sealed class StockDataSyncCoordinatorTests
{
    [Fact]
    public async Task EnqueueWatchlistAsync_creates_one_task_per_stock_in_watchlist_order()
    {
        var first = CreateStock("000001", "SZSE");
        var second = CreateStock("600000", "SSE");
        var watchlist = new Mock<IStockWatchlistAppService>();
        watchlist.Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([first, second]);
        var queue = new Mock<IStockDataSyncJobQueue>();
        var calls = new List<(Guid BatchId, Guid SecurityId, string? DeduplicationKey)>();
        queue.Setup(x => x.EnqueueStockAsync(
                "scheduled",
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .Callback((string _, Guid batchId, Guid securityId, string? key, CancellationToken _) =>
                calls.Add((batchId, securityId, key)))
            .ReturnsAsync((string trigger, Guid batchId, Guid securityId, string? _, CancellationToken _) =>
                CreateJob(trigger, batchId, securityId));
        var batch = new StockDataSyncBatchResponse(
            Guid.CreateVersion7(), "scheduled", "pending", 2, 2, 0, 0, 0,
            DateTimeOffset.UnixEpoch, null, []);
        queue.Setup(x => x.GetBatchAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(batch);
        var coordinator = new StockDataSyncCoordinator(
            watchlist.Object,
            queue.Object,
            TimeProvider.System);

        var result = await coordinator.EnqueueWatchlistAsync(
            "scheduled",
            "scheduled:2026-09-16:18:00",
            CancellationToken.None);

        Assert.Same(batch, result);
        Assert.Equal(2, calls.Count);
        Assert.Equal(first.SecurityId, calls[0].SecurityId);
        Assert.Equal(second.SecurityId, calls[1].SecurityId);
        Assert.Equal(calls[0].BatchId, calls[1].BatchId);
        Assert.Equal($"scheduled:2026-09-16:18:00:{first.SecurityCode}:{first.ExchangeCode}", calls[0].DeduplicationKey);
        Assert.Equal($"scheduled:2026-09-16:18:00:{second.SecurityCode}:{second.ExchangeCode}", calls[1].DeduplicationKey);
    }

    [Fact]
    public async Task EnqueueStockAsync_rejects_invalid_reference_before_reading_watchlist()
    {
        var watchlist = new Mock<IStockWatchlistAppService>();
        var queue = new Mock<IStockDataSyncJobQueue>();
        var coordinator = new StockDataSyncCoordinator(
            watchlist.Object,
            queue.Object,
            TimeProvider.System);

        var exception = await Assert.ThrowsAsync<Portwise.Application.Exceptions.ApplicationValidationException>(() =>
            coordinator.EnqueueStockAsync("manual", "bad", "SSE", CancellationToken.None));

        Assert.Equal(Portwise.Application.Exceptions.ApplicationErrorCodes.StockDataSyncValidationFailed, exception.ErrorCode);
        watchlist.Verify(x => x.GetAsync(It.IsAny<CancellationToken>()), Times.Never);
        queue.Verify(x => x.EnqueueStockAsync(
            It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static StockDataSyncJobResponse CreateJob(
        string trigger,
        Guid batchId,
        Guid securityId)
        => new(
            Guid.CreateVersion7(), trigger, "pending", 0,
            DateTimeOffset.UnixEpoch, null, null, null, null,
            batchId, securityId, "000001", "SZSE");

    private static StockWatchlistItem CreateStock(string code, string exchange)
        => new(code, exchange, string.Empty, "A-share", "CNY", null)
        {
            SecurityId = Guid.CreateVersion7()
        };
}
