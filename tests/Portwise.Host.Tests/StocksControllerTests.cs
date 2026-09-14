using Portwise.Application.Dtos;
using Portwise.Application.Contracts;
using Portwise.Application.Recommendations.Contracts;
using Portwise.Application.Recommendations.Dtos;
using Portwise.Application.Stocks.Contracts;
using Portwise.Application.Stocks.Dtos;
using Portwise.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Portwise.Host.Tests;

public sealed class StocksControllerTests
{
    [Fact]
    public async Task AddStock_returns_the_created_watchlist_item()
    {
        var stock = new StockWatchlistItem(
            "000001",
            "SZSE",
            string.Empty,
            "A-share",
            "CNY",
            new StockHoldingSnapshot(100, 100, 100, 0m));
        var watchlist = new Mock<IStockWatchlistAppService>();
        watchlist
            .Setup(x => x.AddAsync(
                It.IsAny<AddStockRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock);
        var controller = CreateController(
            Mock.Of<IStockDataSyncJobQueue>(),
            watchlist.Object);

        var response = await controller.AddStock(
            new AddStockRequest("000001", "SZSE", 100),
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(stock, ok.Value);
        watchlist.Verify(x => x.AddAsync(
            It.Is<AddStockRequest>(request =>
                request.SecurityCode == "000001"
                && request.ExchangeCode == "SZSE"
                && request.HeldShares == 100),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task SyncStocks_QueuesManualJobAndReturnsAcceptedResponse()
    {
        var job = new StockDataSyncJobResponse(
            Guid.CreateVersion7(), "manual", "pending", 0,
            DateTimeOffset.UnixEpoch, null, null, null, null);
        var queue = new Mock<IStockDataSyncJobQueue>();
        queue
            .Setup(x => x.EnqueueAsync(
                "manual",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);
        var controller = CreateController(queue.Object);

        var response = await controller.SyncStocks(CancellationToken.None);

        var accepted = Assert.IsType<AcceptedAtActionResult>(response.Result);
        Assert.Equal(nameof(StocksController.GetSyncJob), accepted.ActionName);
        Assert.Equal("1", accepted.RouteValues?["version"]);
        Assert.Same(job, accepted.Value);
        queue.Verify(x => x.EnqueueAsync(
            "manual",
            null,
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetSyncJob_ReturnsPersistedStatusAndResult()
    {
        var job = new StockDataSyncJobResponse(
            Guid.CreateVersion7(), "manual", "completed_with_failures", 1,
            DateTimeOffset.UnixEpoch, DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch,
            new StockDataSyncRunResult(1, 0, 1, [], DateTimeOffset.UnixEpoch),
            null);
        var queue = new Mock<IStockDataSyncJobQueue>();
        queue.Setup(x => x.GetAsync(job.Id, CancellationToken.None))
            .ReturnsAsync(job);
        var controller = CreateController(queue.Object);

        var response = await controller.GetSyncJob(job.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(job, ok.Value);
    }

    private static StocksController CreateController(
        IStockDataSyncJobQueue queue,
        IStockWatchlistAppService? watchlist = null)
        => new(
            watchlist ?? Mock.Of<IStockWatchlistAppService>(),
            Mock.Of<IStockModelParameterAppService>(),
            Mock.Of<IStockPriceObservationAppService>(),
            Mock.Of<IStockDividendEventAppService>(),
            Mock.Of<IStockRecommendationAppService>(),
            Mock.Of<IStockFinancialSnapshotAppService>(),
            queue)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
}
