using Portwise.Application.Stocks.Dtos;

namespace Portwise.Application.Stocks.Contracts;

public interface IStockPriceObservationAppService
{
    Task<StockPriceObservationResult> SyncAsync(
        SyncStockPriceRequest request,
        CancellationToken cancellationToken);
}
