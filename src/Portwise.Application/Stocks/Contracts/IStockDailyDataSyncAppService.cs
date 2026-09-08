using Portwise.Application.Dtos;

namespace Portwise.Application.Contracts;

public interface IStockDailyDataSyncAppService
{
    Task<StockDataSyncRunResult> SyncAsync(CancellationToken cancellationToken);
}
