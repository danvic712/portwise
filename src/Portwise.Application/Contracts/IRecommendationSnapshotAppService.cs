using Portwise.Application.Dtos;

namespace Portwise.Application.Contracts;

public interface IRecommendationSnapshotAppService
{
    Task<CreateRecommendationSnapshotResult> CreateAsync(
        CancellationToken cancellationToken);
}
