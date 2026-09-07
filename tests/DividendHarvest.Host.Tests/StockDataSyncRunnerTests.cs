using DividendHarvest.Application.Contracts;
using DividendHarvest.Application.Diagnostics;
using DividendHarvest.Application.Dtos;
using DividendHarvest.Background;
using DividendHarvest.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace DividendHarvest.Host.Tests;

public sealed class StockDataSyncRunnerTests
{
    [Fact]
    public async Task RunAsync_SerializesEveryTriggerThroughOneGate()
    {
        var firstRunEntered = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirstRun = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var invocationCount = 0;
        var syncAppService = new Mock<IStockDailyDataSyncAppService>();
        syncAppService
            .Setup(x => x.SyncAsync(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken cancellationToken) =>
            {
                Interlocked.Increment(ref invocationCount);
                firstRunEntered.TrySetResult();
                await releaseFirstRun.Task.WaitAsync(cancellationToken);
                return CreateResult();
            });

        await using var serviceProvider = CreateServiceProvider(syncAppService.Object);
        var runner = CreateRunner(serviceProvider);
        var firstRun = runner.RunAsync(StockDataSyncTrigger.Manual, CancellationToken.None);

        await firstRunEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        try
        {
            using var cancellation = new CancellationTokenSource();
            var queuedRun = runner.RunAsync(
                StockDataSyncTrigger.Scheduled,
                cancellation.Token);
            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => queuedRun);
            Assert.Equal(1, Volatile.Read(ref invocationCount));
        }
        finally
        {
            releaseFirstRun.TrySetResult();
            await firstRun;
        }
    }

    [Fact]
    public async Task RunAsync_CreatesRunContextForEachTrigger()
    {
        var syncAppService = new Mock<IStockDailyDataSyncAppService>();
        syncAppService
            .Setup(x => x.SyncAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateResult());
        var scopes = new List<DiagnosticScope>();
        var diagnosticContext = new Mock<IDiagnosticContext>();
        diagnosticContext
            .Setup(x => x.BeginScope(It.IsAny<DiagnosticScope>()))
            .Callback<DiagnosticScope>(scopes.Add)
            .Returns(Mock.Of<IDisposable>());

        await using var serviceProvider = CreateServiceProvider(syncAppService.Object);
        var runner = CreateRunner(serviceProvider, diagnosticContext.Object);

        var manual = await runner.RunAsync(
            StockDataSyncTrigger.Manual,
            CancellationToken.None);
        var scheduled = await runner.RunAsync(
            StockDataSyncTrigger.Scheduled,
            CancellationToken.None);

        Assert.NotEqual(manual.RunId, scheduled.RunId);
        Assert.Collection(
            scopes,
            scope =>
            {
                Assert.Equal("stock_data_sync", scope.Operation);
                Assert.Equal(manual.RunId, scope.RunId);
                Assert.Equal(manual.RunId, scope.CorrelationId);
            },
            scope =>
            {
                Assert.Equal("daily_stock_data_sync", scope.Operation);
                Assert.Equal(scheduled.RunId, scope.RunId);
                Assert.Equal(scheduled.RunId, scope.CorrelationId);
            });
    }

    private static ServiceProvider CreateServiceProvider(
        IStockDailyDataSyncAppService syncAppService)
        => new ServiceCollection()
            .AddScoped(_ => syncAppService)
            .BuildServiceProvider();

    private static StockDataSyncRunner CreateRunner(
        ServiceProvider serviceProvider,
        IDiagnosticContext? diagnosticContext = null)
        => new(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            diagnosticContext ?? CreateDiagnosticContext(),
            NullLogger<StockDataSyncRunner>.Instance);

    private static IDiagnosticContext CreateDiagnosticContext()
    {
        var diagnosticContext = new Mock<IDiagnosticContext>();
        diagnosticContext
            .Setup(x => x.BeginScope(It.IsAny<DiagnosticScope>()))
            .Returns(Mock.Of<IDisposable>());
        return diagnosticContext.Object;
    }

    private static StockDataSyncRunResult CreateResult()
        => new(1, 1, 0, [], DateTimeOffset.UnixEpoch);
}
