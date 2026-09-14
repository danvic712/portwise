using Portwise.Application.Exceptions;
using Portwise.Application.Stocks.Contracts;
using Portwise.Application.Stocks.Dtos;
using Portwise.Contracts;

namespace Portwise.Background;

internal sealed class StockDataSyncBackgroundService(
    IStockDataSyncJobQueue queue,
    IStockDataSyncRunner runner,
    TimeProvider timeProvider,
    ILogger<StockDataSyncBackgroundService> logger) : BackgroundService
{
    private readonly Guid ownerId = Guid.CreateVersion7();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var job = await queue.TryClaimAsync(ownerId, stoppingToken);
                if (job is not null)
                {
                    await ExecuteJobAsync(job, stoppingToken);
                    continue;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    "Stock sync queue poll failed. ExceptionType: {ExceptionType}.",
                    exception.GetType().Name);
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2), timeProvider, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private async Task ExecuteJobAsync(
        StockDataSyncJobLease job,
        CancellationToken stoppingToken)
    {
        using var runCancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        var leaseLost = false;
        var heartbeat = RenewLeaseUntilCanceledAsync(job.Id, runCancellation, () => leaseLost = true);
        try
        {
            if (!Enum.TryParse<StockDataSyncTrigger>(
                    job.TriggerCode, ignoreCase: true, out var trigger))
            {
                throw new InvalidOperationException("Unknown stock sync job trigger.");
            }

            var execution = await runner.RunAsync(trigger, runCancellation.Token);
            runCancellation.Cancel();
            await heartbeat;
            if (leaseLost)
            {
                throw new InvalidOperationException("The stock sync job lease was lost.");
            }

            await queue.CompleteAsync(job.Id, ownerId, execution.Result, stoppingToken);
            logger.LogInformation(
                "Stock sync job completed. JobId: {JobId}, RunId: {RunId}.",
                job.Id,
                execution.RunId);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            runCancellation.Cancel();
            await heartbeat;
        }
        catch (Exception exception)
        {
            runCancellation.Cancel();
            await heartbeat;
            var errorCode = exception is ApplicationExceptionBase applicationException
                ? applicationException.ErrorCode
                : "stock_sync_execution_failed";
            logger.LogError(
                "Stock sync job failed. JobId: {JobId}, Attempt: {Attempt}, ErrorCode: {ErrorCode}, ExceptionType: {ExceptionType}.",
                job.Id,
                job.AttemptCount,
                errorCode,
                exception.GetType().Name);
            await queue.FailAsync(job.Id, ownerId, job.AttemptCount, errorCode, stoppingToken);
        }
    }

    private async Task RenewLeaseUntilCanceledAsync(
        Guid jobId,
        CancellationTokenSource runCancellation,
        Action onLeaseLost)
    {
        try
        {
            while (!runCancellation.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), timeProvider, runCancellation.Token);
                if (!await queue.RenewLeaseAsync(jobId, ownerId, runCancellation.Token))
                {
                    onLeaseLost();
                    runCancellation.Cancel();
                    return;
                }
            }
        }
        catch (OperationCanceledException) when (runCancellation.IsCancellationRequested)
        {
            return;
        }
        catch (Exception exception)
        {
            logger.LogError(
                "Stock sync job heartbeat failed. JobId: {JobId}, ExceptionType: {ExceptionType}.",
                jobId,
                exception.GetType().Name);
            onLeaseLost();
            runCancellation.Cancel();
        }
    }
}
