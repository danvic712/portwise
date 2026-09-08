using Portwise.Application.Recommendations.Dtos;

namespace Portwise.Application.Recommendations.Contracts;

public interface IPortfolioRecommendationAppService
{
    Task<PortfolioRecommendationResult> GetAsync(
        CancellationToken cancellationToken);
}
