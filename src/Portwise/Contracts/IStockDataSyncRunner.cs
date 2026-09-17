using Portwise.Application.Stocks.Dtos;
using Portwise.Domain.Securities;

namespace Portwise.Contracts;

public interface IStockDataSyncRunner
{
    Task<StockDataSyncExecutionResult> RunAsync(
        StockDataSyncTrigger trigger,
        AShareReference reference,
        CancellationToken cancellationToken);
}
