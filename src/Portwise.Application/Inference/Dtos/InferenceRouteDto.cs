namespace Portwise.Application.Inference.Dtos;

/// <summary>
/// Describes one inference capability binding and its effective runtime status.
/// </summary>
public sealed record InferenceRouteDto(
    Guid Id,
    string CapabilityCode,
    Guid? ProviderId,
    string? ModelName,
    string RuntimeStatusCode,
    long Revision,
    DateTimeOffset UpdatedAtUtc);
