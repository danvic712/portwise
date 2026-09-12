namespace Portwise.Application.Inference.Dtos;

/// <summary>
/// Requests one inference route binding update.
/// </summary>
public sealed record UpdateInferenceRouteRequest(
    string CapabilityCode,
    Guid? ProviderId,
    string? ModelName,
    long ExpectedRevision);
