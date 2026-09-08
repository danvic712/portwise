using Portwise.Application.Stocks.Dtos;

namespace Portwise.Contracts;

public interface IStockDataSyncRunner
{
    Task<StockDataSyncExecutionResult> RunAsync(
        StockDataSyncTrigger trigger,
        CancellationToken cancellationToken);
}
