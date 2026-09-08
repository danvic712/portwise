using Portwise.Application.Contracts;
using Portwise.Application.Portfolio.Dtos;
using Portwise.Application.Recommendations.Contracts;
using Portwise.Application.Recommendations.Dtos;
using Portwise.Application.Stocks.Dtos;
using Portwise.Domain.Contracts;
using Portwise.Domain.Models;
using Portwise.Domain.Recommendations;

namespace Portwise.Application.Recommendations;

public sealed class PortfolioAllocationAppService(
    IUow uow,
    TimeProvider timeProvider) : IPortfolioAllocationAppService
{
    public async Task<PortfolioRecommendationResult> RunAsync(
        IReadOnlyList<StockWatchlistItem> watchlist,
        IReadOnlyList<StockAnalysisResult> analyses,
        BudgetSummary budgetSummary,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(watchlist);
        ArgumentNullException.ThrowIfNull(analyses);
        ArgumentNullException.ThrowIfNull(budgetSummary);

        if (watchlist.Count != analyses.Count)
        {
            throw new ArgumentException(
                "关注股票和单股分析结果的数量必须一致。",
                nameof(analyses));
        }

        var analysesBySecurityId = analyses.ToDictionary(analysis => analysis.SecurityId);
        var alignedAnalyses = watchlist
            .Select(stock =>
            {
                if (stock.SecurityId == Guid.Empty
                    || !analysesBySecurityId.TryGetValue(stock.SecurityId, out var analysis)
                    || analysis.SecurityCode != stock.SecurityCode
                    || analysis.ExchangeCode != stock.ExchangeCode)
                {
                    throw new ArgumentException(
                        $"股票 {stock.SecurityCode}/{stock.ExchangeCode} 的单股分析身份不匹配。",
                        nameof(analyses));
                }

                return analysis;
            })
            .ToArray();

        var computedAt = analyses.Count > 0
            ? analyses[0].ComputedAt
            : timeProvider.GetUtcNow();
        if (analyses.Any(analysis => analysis.ComputedAt != computedAt))
        {
            throw new ArgumentException(
                "组合建议中的单股分析必须来自同一计算时间点。",
                nameof(analyses));
        }
        var currentDate = DateOnly.FromDateTime(computedAt.UtcDateTime);
        var parameters = await uow.Get<ModelParameterSet>().ListAsync(
            parameter =>
                parameter.PortfolioId == budgetSummary.PortfolioId
                && parameter.EffectiveFromDate <= currentDate,
            cancellationToken: cancellationToken);
        var calculation = RecommendationModule.AllocatePortfolio(
            new PortfolioRecommendationInput(
                budgetSummary.PortfolioId,
                budgetSummary.CashBalanceAmount,
                watchlist
                    .Select((stock, index) =>
                    {
                        var analysis = alignedAnalyses[index];
                        return new PortfolioRecommendationStockInput(
                            stock.SecurityId,
                            stock.SectorCode,
                            analysis.ModelStatusCode,
                            analysis.DividendReliabilityCode,
                            analysis.ClosePrice,
                            analysis.PriceZoneCode,
                            analysis.HeldShares,
                            analysis.CoreShares,
                            stock.Holding?.TargetShares ?? 0,
                            analysis.ModelParameterSetId,
                            analysis.Explanation);
                    })
                    .ToArray(),
                parameters,
                currentDate,
                computedAt));
        var analysisById = alignedAnalyses.ToDictionary(analysis => analysis.SecurityId);
        var recommendations = calculation.Stocks
            .Select(stock =>
            {
                var analysis = analysisById[stock.SecurityId] with
                {
                    Explanation = stock.Explanation
                };
                return new StockRecommendationResult(
                    analysis,
                    stock.SuggestedBuyShares,
                    stock.SuggestedSellShares,
                    stock.SuggestedTradeAmount,
                    stock.EstimatedTransactionFeeAmount);
            })
            .ToArray();

        return new PortfolioRecommendationResult(
            calculation.PortfolioId,
            calculation.StartingAvailableBudgetAmount,
            calculation.RemainingAvailableBudgetAmount,
            calculation.TotalSuggestedTradeAmount,
            calculation.EstimatedTransactionFeeAmount,
            recommendations,
            calculation.ComputedAt);
    }
}
