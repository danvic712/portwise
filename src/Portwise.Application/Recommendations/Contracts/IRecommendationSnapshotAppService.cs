using Portwise.Application.Recommendations.Dtos;

namespace Portwise.Application.Recommendations.Contracts;

public interface IRecommendationSnapshotAppService
{
    Task<CreateRecommendationSnapshotResult> CreateAsync(
        CancellationToken cancellationToken);
}
