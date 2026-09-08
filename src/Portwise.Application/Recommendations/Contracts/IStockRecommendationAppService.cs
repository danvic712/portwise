using Portwise.Application.Dtos;

namespace Portwise.Application.Contracts;

public interface IStockRecommendationAppService
{
    Task<StockRecommendationResult> GetAsync(
        GetStockAnalysisRequest request,
        CancellationToken cancellationToken);
}
