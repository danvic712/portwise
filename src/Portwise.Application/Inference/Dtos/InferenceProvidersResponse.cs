namespace Portwise.Application.Inference.Dtos;

/// <summary>
/// Returns all configured inference providers.
/// </summary>
public sealed record InferenceProvidersResponse(
    IReadOnlyList<InferenceProviderDto> Providers);
