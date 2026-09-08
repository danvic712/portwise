using Portwise.Contracts;

namespace Portwise.Background;

internal sealed class StockDataSyncBackgroundService(
    StockDataSyncTaskQueue taskQueue,
    IStockDataSyncRunner syncRunner) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var _ in taskQueue.ReadAllAsync(stoppingToken))
            {
                await RunSyncAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return;
        }
    }

    private async Task RunSyncAsync(CancellationToken cancellationToken)
    {
        try
        {
            await syncRunner.RunAsync(StockDataSyncTrigger.Setup, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception)
        {
            // The shared runner records the failure with its run ID. Keep processing queued runs.
        }
    }
}
