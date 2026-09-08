using Portwise.Application.Recommendations.Dtos;
using Portwise.Application.Stocks.Dtos;

namespace Portwise.Application.Recommendations.Contracts;

public interface IStockAnalysisAppService
{
    Task<StockAnalysisResult> GetAsync(
        GetStockAnalysisRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<StockAnalysisResult>> GetAsync(
        IReadOnlyList<StockWatchlistItem> stocks,
        CancellationToken cancellationToken);
}
