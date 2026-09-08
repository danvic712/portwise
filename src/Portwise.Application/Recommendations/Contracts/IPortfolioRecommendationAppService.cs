using Portwise.Application.Dtos;

namespace Portwise.Application.Contracts;

public interface IPortfolioRecommendationAppService
{
    Task<PortfolioRecommendationResult> GetAsync(
        CancellationToken cancellationToken);
}
