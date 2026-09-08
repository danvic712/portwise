using Portwise.Application.Contracts;
using Portwise.Application.Diagnostics;
using Portwise.Application.Exceptions;
using Portwise.Application.Stocks.Contracts;
using Portwise.Contracts;

namespace Portwise.Background;

internal sealed class StockDataSyncRunner(
    IServiceScopeFactory serviceScopeFactory,
    IDiagnosticContext diagnosticContext,
    ILogger<StockDataSyncRunner> logger) : IStockDataSyncRunner
{
    private readonly SemaphoreSlim gate = new(1, 1);

    public async Task<StockDataSyncExecutionResult> RunAsync(
        StockDataSyncTrigger trigger,
        CancellationToken cancellationToken)
    {
        var operation = GetDiagnosticOperation(trigger);
        await gate.WaitAsync(cancellationToken);

        var runId = Guid.NewGuid().ToString("N");
        using var diagnosticScope = diagnosticContext.BeginScope(new DiagnosticScope(
            operation,
            CorrelationId: runId,
            RunId: runId));

        try
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var syncAppService = scope.ServiceProvider
                .GetRequiredService<IStockDailyDataSyncAppService>();
            var result = await syncAppService.SyncAsync(cancellationToken);

            logger.LogInformation(
                "Stock data synchronization finished. Trigger: {Trigger}, RunId: {RunId}, attempted: {Attempted}, completed: {Completed}, failed: {Failed}.",
                trigger,
                runId,
                result.AttemptedStockCount,
                result.FullyCompletedStockCount,
                result.PartiallyFailedStockCount);

            return new StockDataSyncExecutionResult(runId, result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation(
                "Stock data synchronization was canceled. Trigger: {Trigger}, RunId: {RunId}.",
                trigger,
                runId);
            throw;
        }
        catch (ApplicationExceptionBase exception)
        {
            var causeType = exception.InnerException?.GetType().Name ?? exception.GetType().Name;
            logger.LogError(
                "Stock data synchronization failed. Trigger: {Trigger}, RunId: {RunId}, error code: {ErrorCode}, cause type: {CauseType}.",
                trigger,
                runId,
                exception.ErrorCode,
                causeType);
            throw;
        }
        catch (Exception exception)
        {
            var causeType = exception.InnerException?.GetType().Name ?? exception.GetType().Name;
            logger.LogError(
                "Stock data synchronization failed. Trigger: {Trigger}, RunId: {RunId}, exception type: {ExceptionType}, cause type: {CauseType}.",
                trigger,
                runId,
                exception.GetType().Name,
                causeType);
            throw;
        }
        finally
        {
            gate.Release();
        }
    }

    private static string GetDiagnosticOperation(StockDataSyncTrigger trigger)
        => trigger switch
        {
            StockDataSyncTrigger.Manual => "stock_data_sync",
            StockDataSyncTrigger.Setup => "stock_data_sync",
            StockDataSyncTrigger.Scheduled => "daily_stock_data_sync",
            _ => throw new ArgumentOutOfRangeException(nameof(trigger), trigger, null)
        };
}
