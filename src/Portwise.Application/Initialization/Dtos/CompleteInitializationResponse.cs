namespace Portwise.Application.Initialization.Dtos;

/// <summary>
/// Returns the persisted readiness and limitation state after onboarding.
/// </summary>
public sealed record CompleteInitializationResponse(
    InitializationStatusResponse Status);
