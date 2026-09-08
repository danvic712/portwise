using Portwise.Application.Stocks.Dtos;

namespace Portwise.Application.Stocks.Contracts;

public interface IStockFinancialSnapshotAppService
{
    Task<IReadOnlyList<StockFinancialSnapshotResult>> SyncAsync(
        SyncStockFinancialsRequest request,
        CancellationToken cancellationToken);
}
