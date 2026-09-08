using Portwise.Application.Portfolio.Dtos;
using Portwise.Application.Recommendations.Dtos;
using Portwise.Application.Stocks.Dtos;

namespace Portwise.Application.Recommendations.Contracts;

public interface IPortfolioAllocationAppService
{
    Task<PortfolioRecommendationResult> RunAsync(
        IReadOnlyList<StockWatchlistItem> watchlist,
        IReadOnlyList<StockAnalysisResult> analyses,
        BudgetSummary budgetSummary,
        CancellationToken cancellationToken);
}
