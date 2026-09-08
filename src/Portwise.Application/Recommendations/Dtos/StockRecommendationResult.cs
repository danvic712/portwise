namespace Portwise.Application.Recommendations.Dtos;

public sealed record StockRecommendationResult(
    StockAnalysisResult Analysis,
    int SuggestedBuyShares,
    int SuggestedSellShares,
    decimal SuggestedTradeAmount,
    decimal EstimatedTransactionFeeAmount);
