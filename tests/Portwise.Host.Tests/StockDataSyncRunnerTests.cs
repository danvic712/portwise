using Portwise.Application.Contracts;
using Portwise.Application.Diagnostics;
using Portwise.Application.Stocks.Contracts;
using Portwise.Application.Stocks.Dtos;
using Portwise.Background;
using Portwise.Contracts;
using Portwise.Domain.Securities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Portwise.Host.Tests;

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
        var factSync = new Mock<IStockFactSyncAppService>();
        factSync
            .Setup(x => x.SyncAsync(
                It.IsAny<AShareReference>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (AShareReference reference, CancellationToken cancellationToken) =>
            {
                Interlocked.Increment(ref invocationCount);
                firstRunEntered.TrySetResult();
                await releaseFirstRun.Task.WaitAsync(cancellationToken);
                return CreateResult(reference);
            });

        await using var serviceProvider = CreateServiceProvider(factSync.Object);
        var runner = CreateRunner(serviceProvider);
        var reference = AShareReference.Create("000001", "SZSE");
        var firstRun = runner.RunAsync(StockDataSyncTrigger.Manual, reference, CancellationToken.None);

        await firstRunEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        try
        {
            using var cancellation = new CancellationTokenSource();
            var queuedRun = runner.RunAsync(
                StockDataSyncTrigger.Scheduled,
                reference,
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
        var factSync = new Mock<IStockFactSyncAppService>();
        factSync
            .Setup(x => x.SyncAsync(
                It.IsAny<AShareReference>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AShareReference reference, CancellationToken _) => CreateResult(reference));
        var scopes = new List<DiagnosticScope>();
        var diagnosticContext = new Mock<IDiagnosticContext>();
        diagnosticContext
            .Setup(x => x.BeginScope(It.IsAny<DiagnosticScope>()))
            .Callback<DiagnosticScope>(scopes.Add)
            .Returns(Mock.Of<IDisposable>());

        await using var serviceProvider = CreateServiceProvider(factSync.Object);
        var runner = CreateRunner(serviceProvider, diagnosticContext.Object);
        var reference = AShareReference.Create("000001", "SZSE");

        var manual = await runner.RunAsync(
            StockDataSyncTrigger.Manual,
            reference,
            CancellationToken.None);
        var scheduled = await runner.RunAsync(
            StockDataSyncTrigger.Scheduled,
            reference,
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
        IStockFactSyncAppService factSync)
        => new ServiceCollection()
            .AddScoped(_ => factSync)
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

    private static StockFactSyncResult CreateResult(AShareReference reference)
        => new(reference.SecurityCode, reference.ExchangeCode, null, [], [], []);
}
