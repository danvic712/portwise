using Portwise.Application.Stocks.Dtos;

namespace Portwise.Application.Stocks.Contracts;

public interface IStockDailyDataSyncAppService
{
    Task<StockDataSyncRunResult> SyncAsync(CancellationToken cancellationToken);
}
