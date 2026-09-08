namespace Portwise.Application.Recommendations.Dtos;

/// <summary>Portfolio-level recommendation and budget allocation result.</summary>
/// <param name="PortfolioId">Identifier of the portfolio.</param>
/// <param name="StartingAvailableBudgetAmount">Available budget before allocation.</param>
/// <param name="RemainingAvailableBudgetAmount">Available budget after allocation.</param>
/// <param name="TotalSuggestedTradeAmount">Total suggested trade amount.</param>
/// <param name="EstimatedTransactionFeeAmount">Estimated fees for suggested trades.</param>
/// <param name="Stocks">Per-stock recommendation results.</param>
/// <param name="ComputedAt">UTC timestamp when the recommendation was computed.</param>
public sealed record PortfolioRecommendationResult(
    Guid PortfolioId,
    decimal StartingAvailableBudgetAmount,
    decimal RemainingAvailableBudgetAmount,
    decimal TotalSuggestedTradeAmount,
    decimal EstimatedTransactionFeeAmount,
    IReadOnlyList<StockRecommendationResult> Stocks,
    DateTimeOffset ComputedAt);
