using Portwise.Application.Contracts;
using Portwise.Application.Dtos;

namespace Portwise.Application.Recommendations;

public sealed class StockRecommendationAppService(
    IStockAnalysisAppService stockAnalysisAppService,
    IStockWatchlistAppService stockWatchlistAppService,
    IBudgetAppService budgetAppService,
    IPortfolioAllocationAppService portfolioAllocationAppService)
    : IStockRecommendationAppService
{
    public async Task<StockRecommendationResult> GetAsync(
        GetStockAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var watchlist = await stockWatchlistAppService.GetAsync(cancellationToken);
        var requestedStock = watchlist.FirstOrDefault(item =>
            item.SecurityCode == request.SecurityCode.Trim()
            && item.ExchangeCode == request.ExchangeCode.Trim());
        if (requestedStock is null)
        {
            var fallbackAnalysis = await stockAnalysisAppService.GetAsync(request, cancellationToken);
            return new StockRecommendationResult(fallbackAnalysis, 0, 0, 0m, 0m);
        }

        var analyses = await stockAnalysisAppService.GetAsync(watchlist, cancellationToken);
        var analysis = analyses.Single(item => item.SecurityId == requestedStock.SecurityId);

        var budgetSummary = await budgetAppService.GetSummaryAsync(cancellationToken);
        var recommendation = await portfolioAllocationAppService.RunAsync(
            watchlist,
            analyses,
            budgetSummary,
            cancellationToken);

        return recommendation.Stocks
            .Single(stock => stock.Analysis.SecurityId == analysis.SecurityId);
    }
}
