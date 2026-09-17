using Portwise.Application.Contracts;
using Portwise.Application.Diagnostics;
using Portwise.Application.Exceptions;
using Portwise.Application.Stocks.Contracts;
using Portwise.Contracts;
using Portwise.Domain.Securities;

namespace Portwise.Background;

internal sealed class StockDataSyncRunner(
    IServiceScopeFactory serviceScopeFactory,
    IDiagnosticContext diagnosticContext,
    ILogger<StockDataSyncRunner> logger) : IStockDataSyncRunner
{
    private readonly SemaphoreSlim gate = new(1, 1);

    public async Task<StockDataSyncExecutionResult> RunAsync(
        StockDataSyncTrigger trigger,
        AShareReference reference,
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
            var factSyncAppService = scope.ServiceProvider
                .GetRequiredService<IStockFactSyncAppService>();
            var result = await factSyncAppService.SyncAsync(reference, cancellationToken);
            logger.LogInformation(
                "Stock data synchronization finished. Trigger: {Trigger}, RunId: {RunId}, SecurityCode: {SecurityCode}, ExchangeCode: {ExchangeCode}, failed: {Failed}.",
                trigger,
                runId,
                reference.SecurityCode,
                reference.ExchangeCode,
                result.Failures.Count);
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
            StockDataSyncTrigger.Initialization => "stock_data_sync",
            StockDataSyncTrigger.Scheduled => "daily_stock_data_sync",
            StockDataSyncTrigger.Watchlist => "stock_data_sync",
            _ => throw new ArgumentOutOfRangeException(nameof(trigger), trigger, null)
        };
}
