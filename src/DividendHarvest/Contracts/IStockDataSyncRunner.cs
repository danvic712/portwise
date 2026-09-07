using DividendHarvest.Application.Dtos;

namespace DividendHarvest.Contracts;

public interface IStockDataSyncRunner
{
    Task<StockDataSyncExecutionResult> RunAsync(
        StockDataSyncTrigger trigger,
        CancellationToken cancellationToken);
}
