namespace Portwise.Application.Recommendations.Dtos;

/// <summary>Recommendation and suggested trade quantities for one stock.</summary>
/// <param name="Analysis">Underlying stock analysis.</param>
/// <param name="SuggestedBuyShares">Suggested buy quantity in shares.</param>
/// <param name="SuggestedSellShares">Suggested sell quantity in shares.</param>
/// <param name="SuggestedTradeAmount">Suggested trade amount.</param>
/// <param name="EstimatedTransactionFeeAmount">Estimated transaction fee.</param>
public sealed record StockRecommendationResult(
    StockAnalysisResult Analysis,
    int SuggestedBuyShares,
    int SuggestedSellShares,
    decimal SuggestedTradeAmount,
    decimal EstimatedTransactionFeeAmount);
