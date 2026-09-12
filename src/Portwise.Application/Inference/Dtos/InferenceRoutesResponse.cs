namespace Portwise.Application.Inference.Dtos;

/// <summary>
/// Returns the fixed Chat and Embedding inference routes.
/// </summary>
public sealed record InferenceRoutesResponse(IReadOnlyList<InferenceRouteDto> Routes);
