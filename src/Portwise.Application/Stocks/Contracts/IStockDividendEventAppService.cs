using Portwise.Application.Stocks.Dtos;

namespace Portwise.Application.Stocks.Contracts;

public interface IStockDividendEventAppService
{
    Task<IReadOnlyList<StockDividendEventResult>> SyncAsync(
        SyncStockDividendsRequest request,
        CancellationToken cancellationToken);
}
