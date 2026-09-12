using Portwise.Application.Inference.Dtos;

namespace Portwise.Application.Inference.Contracts;

/// <summary>
/// Manages inference providers, protected keys, capability routes, and verification.
/// </summary>
public interface IInferenceConfigurationAppService
{
    Task<InferenceProvidersResponse> GetProvidersAsync(CancellationToken cancellationToken);

    Task<InferenceProviderDto> CreateProviderAsync(
        CreateInferenceProviderRequest request,
        CancellationToken cancellationToken);

    Task<InferenceProviderDto> UpdateProviderAsync(
        Guid providerId,
        UpdateInferenceProviderRequest request,
        CancellationToken cancellationToken);

    Task DeleteProviderAsync(
        Guid providerId,
        long expectedRevision,
        CancellationToken cancellationToken);

    Task<VerifyInferenceProviderResponse> VerifyProviderAsync(
        Guid providerId,
        VerifyInferenceProviderRequest request,
        CancellationToken cancellationToken);

    Task<InferenceRoutesResponse> GetRoutesAsync(CancellationToken cancellationToken);

    Task<InferenceRoutesResponse> UpdateRoutesAsync(
        UpdateInferenceRoutesRequest request,
        CancellationToken cancellationToken);
}
