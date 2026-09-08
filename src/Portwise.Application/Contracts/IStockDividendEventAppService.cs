using Portwise.Application.Dtos;

namespace Portwise.Application.Contracts;

public interface IStockDividendEventAppService
{
    Task<IReadOnlyList<StockDividendEventResult>> SyncAsync(
        SyncStockDividendsRequest request,
        CancellationToken cancellationToken);
}
