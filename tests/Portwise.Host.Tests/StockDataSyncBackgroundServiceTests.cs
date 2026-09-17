using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Portwise.Application.Stocks.Contracts;
using Portwise.Application.Stocks.Dtos;
using Portwise.Background;
using Portwise.Contracts;
using Portwise.Domain.Securities;
using Xunit;

namespace Portwise.Host.Tests;

public sealed class StockDataSyncBackgroundServiceTests
{
    [Fact]
    public async Task Worker_CompletesClaimedStockJobWithStructuredResult()
    {
        var id = Guid.CreateVersion7();
        var claimed = new StockDataSyncJobLease(
            id,
            "manual",
            1,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "000001",
            "SZSE");
        var result = new StockFactSyncResult(
            "000001",
            "SZSE",
            null,
            [],
            [],
            [new StockDataSyncFailure(
                "000001",
                "SZSE",
                "price",
                "stock_market_data_unavailable",
                new Dictionary<string, object?>())]);
        var queue = new Mock<IStockDataSyncJobQueue>();
        queue.SetupSequence(x => x.TryClaimAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(claimed)
            .ReturnsAsync((StockDataSyncJobLease?)null);
        var completed = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        queue.Setup(x => x.CompleteAsync(
                id,
                It.IsAny<Guid>(),
                result,
                It.IsAny<CancellationToken>()))
            .Callback(() => completed.TrySetResult())
            .Returns(Task.CompletedTask);
        var runner = new Mock<IStockDataSyncRunner>();
        runner.Setup(x => x.RunAsync(
                StockDataSyncTrigger.Manual,
                It.Is<AShareReference>(reference =>
                    reference.SecurityCode == "000001" && reference.ExchangeCode == "SZSE"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockDataSyncExecutionResult("run-1", result));
        var service = new StockDataSyncBackgroundService(
            queue.Object,
            runner.Object,
            TimeProvider.System,
            NullLogger<StockDataSyncBackgroundService>.Instance);

        await service.StartAsync(CancellationToken.None);
        try
        {
            await completed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            await service.StopAsync(CancellationToken.None);
        }

        queue.Verify(x => x.CompleteAsync(
            id,
            It.IsAny<Guid>(),
            result,
            It.IsAny<CancellationToken>()), Times.Once);
        queue.Verify(x => x.FailAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
