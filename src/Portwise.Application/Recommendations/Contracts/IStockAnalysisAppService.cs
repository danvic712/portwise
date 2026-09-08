using Portwise.Application.Dtos;

namespace Portwise.Application.Contracts;

public interface IStockAnalysisAppService
{
    Task<StockAnalysisResult> GetAsync(
        GetStockAnalysisRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<StockAnalysisResult>> GetAsync(
        IReadOnlyList<StockWatchlistItem> stocks,
        CancellationToken cancellationToken);
}
