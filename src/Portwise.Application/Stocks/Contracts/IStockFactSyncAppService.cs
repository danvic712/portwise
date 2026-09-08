using Portwise.Application.Stocks.Dtos;
using Portwise.Domain.Securities;

namespace Portwise.Application.Stocks.Contracts;

public interface IStockFactSyncAppService
{
    Task<StockFactSyncResult> SyncAsync(
        AShareReference reference,
        CancellationToken cancellationToken);

    Task<StockPriceObservationResult> SyncPriceAsync(
        AShareReference reference,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<StockDividendEventResult>> SyncDividendsAsync(
        AShareReference reference,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<StockFinancialSnapshotResult>> SyncFinancialsAsync(
        AShareReference reference,
        CancellationToken cancellationToken);
}
