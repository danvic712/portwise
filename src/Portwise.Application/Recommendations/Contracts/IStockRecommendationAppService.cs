using Portwise.Application.Recommendations.Dtos;

namespace Portwise.Application.Recommendations.Contracts;

public interface IStockRecommendationAppService
{
    Task<StockRecommendationResult> GetAsync(
        GetStockAnalysisRequest request,
        CancellationToken cancellationToken);
}
