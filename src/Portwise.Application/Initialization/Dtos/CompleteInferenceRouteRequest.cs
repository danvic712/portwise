namespace Portwise.Application.Initialization.Dtos;

/// <summary>
/// Describes an optional Chat or Embedding binding by provider name and model.
/// </summary>
public sealed record CompleteInferenceRouteRequest(
    string CapabilityCode,
    string? ProviderName,
    string? ModelName);
