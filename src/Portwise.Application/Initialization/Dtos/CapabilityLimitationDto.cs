namespace Portwise.Application.Initialization.Dtos;

/// <summary>
/// Describes one capability that is unavailable or not fully configured.
/// </summary>
public sealed record CapabilityLimitationDto(
    string CapabilityCode,
    string StatusCode);
