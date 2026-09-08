using Portwise.Application.Contracts;
using Portwise.Application.Dtos;

namespace Portwise.Application.Recommendations;

public sealed class PortfolioRecommendationAppService(
    IStockWatchlistAppService stockWatchlistAppService,
    IStockAnalysisAppService stockAnalysisAppService,
    IBudgetAppService budgetAppService,
    IPortfolioAllocationAppService portfolioAllocationAppService)
    : IPortfolioRecommendationAppService
{
    public async Task<PortfolioRecommendationResult> GetAsync(
        CancellationToken cancellationToken)
    {
        var watchlist = await stockWatchlistAppService.GetAsync(cancellationToken);
        var analyses = await stockAnalysisAppService.GetAsync(watchlist, cancellationToken);

        var budgetSummary = await budgetAppService.GetSummaryAsync(cancellationToken);
        return await portfolioAllocationAppService.RunAsync(
            watchlist,
            analyses,
            budgetSummary,
            cancellationToken);
    }
}
