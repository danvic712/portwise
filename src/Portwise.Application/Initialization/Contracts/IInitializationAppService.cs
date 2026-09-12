using Portwise.Application.Initialization.Dtos;

namespace Portwise.Application.Initialization.Contracts;

/// <summary>
/// Reads initialization readiness and atomically completes onboarding.
/// </summary>
public interface IInitializationAppService
{
    Task<InitializationStatusResponse> GetAsync(CancellationToken cancellationToken);

    Task<CompleteInitializationResponse> CompleteAsync(
        CompleteInitializationRequest request,
        CancellationToken cancellationToken);
}
