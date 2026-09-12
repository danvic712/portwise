namespace Portwise.Application.Initialization.Dtos;

/// <summary>
/// Returns entry readiness, saved preferences, and independent capability limitations.
/// </summary>
public sealed record InitializationStatusResponse(
    bool IsComplete,
    DateTimeOffset? CompletedAtUtc,
    ApplicationPreferenceDto? Preferences,
    IReadOnlyList<CapabilityLimitationDto> Limitations);
