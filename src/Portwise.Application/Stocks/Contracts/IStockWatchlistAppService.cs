using Portwise.Application.Stocks.Dtos;

namespace Portwise.Application.Stocks.Contracts;

public interface IStockWatchlistAppService
{
    Task<IReadOnlyList<StockWatchlistItem>> GetAsync(CancellationToken cancellationToken);
}
