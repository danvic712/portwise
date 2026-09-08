using Portwise.Application.Dtos;

namespace Portwise.Application.Contracts;

public interface IStockFinancialSnapshotAppService
{
    Task<IReadOnlyList<StockFinancialSnapshotResult>> SyncAsync(
        SyncStockFinancialsRequest request,
        CancellationToken cancellationToken);
}
