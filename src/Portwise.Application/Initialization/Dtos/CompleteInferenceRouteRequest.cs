namespace Portwise.Application.Initialization.Dtos;

/// <summary>
/// Describes an optional Chat or Embedding binding by provider identifier and model.
/// </summary>
/// <param name="CapabilityCode">The stable Chat or Embedding capability code.</param>
/// <param name="ProviderId">The existing provider identifier, or null to leave this capability unconfigured.</param>
/// <param name="ModelName">The provider model name, or null to leave this capability unconfigured.</param>
public sealed record CompleteInferenceRouteRequest(
    string CapabilityCode,
    Guid? ProviderId,
    string? ModelName);
