using Portwise.Application.Dtos;
using Portwise.Application.Contracts;
using Portwise.Application.Recommendations.Contracts;
using Portwise.Application.Recommendations.Dtos;
using Portwise.Application.Stocks.Contracts;
using Portwise.Application.Stocks.Dtos;
using Portwise.Contracts;
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
            Mock.Of<IStockDataSyncRunner>(),
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
    public async Task SyncStocks_RunsManualTriggerThroughSharedRunner()
    {
        var syncResult = new StockDataSyncRunResult(
            1,
            1,
            0,
            [],
            DateTimeOffset.UnixEpoch);
        var runner = new Mock<IStockDataSyncRunner>();
        runner
            .Setup(x => x.RunAsync(
                StockDataSyncTrigger.Manual,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockDataSyncExecutionResult("run-1", syncResult));
        var controller = CreateController(runner.Object);

        var response = await controller.SyncStocks(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(syncResult, ok.Value);
        Assert.Equal("run-1", controller.Response.Headers["X-Sync-Run-Id"]);
        runner.Verify(x => x.RunAsync(
            StockDataSyncTrigger.Manual,
            CancellationToken.None), Times.Once);
    }

    private static StocksController CreateController(
        IStockDataSyncRunner runner,
        IStockWatchlistAppService? watchlist = null)
        => new(
            watchlist ?? Mock.Of<IStockWatchlistAppService>(),
            Mock.Of<IStockModelParameterAppService>(),
            Mock.Of<IStockPriceObservationAppService>(),
            Mock.Of<IStockDividendEventAppService>(),
            Mock.Of<IStockRecommendationAppService>(),
            Mock.Of<IStockFinancialSnapshotAppService>(),
            runner)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
}
