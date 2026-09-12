namespace Portwise.Application.Inference.Dtos;

/// <summary>
/// Requests one atomic update to both inference capability routes.
/// </summary>
public sealed record UpdateInferenceRoutesRequest(
    IReadOnlyList<UpdateInferenceRouteRequest> Routes);
