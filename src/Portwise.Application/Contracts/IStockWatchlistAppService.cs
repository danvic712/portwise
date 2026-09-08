using Portwise.Application.Dtos;

namespace Portwise.Application.Contracts;

public interface IStockWatchlistAppService
{
    Task<IReadOnlyList<StockWatchlistItem>> GetAsync(CancellationToken cancellationToken);
}
