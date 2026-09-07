using DividendHarvest.Application.Contracts;
using DividendHarvest.Application.Dtos;
using DividendHarvest.Contracts;
using DividendHarvest.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DividendHarvest.Host.Tests;

public sealed class StocksControllerTests
{
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

    private static StocksController CreateController(IStockDataSyncRunner runner)
        => new(
            Mock.Of<IStockWatchlistAppService>(),
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
