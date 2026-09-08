using Portwise.Application.Dtos;

namespace Portwise.Application.Contracts;

public interface IStockPriceObservationAppService
{
    Task<StockPriceObservationResult> SyncAsync(
        SyncStockPriceRequest request,
        CancellationToken cancellationToken);
}
