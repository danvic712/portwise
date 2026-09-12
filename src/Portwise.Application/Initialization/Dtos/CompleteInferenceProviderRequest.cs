using Portwise.Application.Configuration.Dtos;

namespace Portwise.Application.Initialization.Dtos;

/// <summary>
/// Describes an optional inference provider to save during onboarding.
/// </summary>
public sealed record CompleteInferenceProviderRequest(
    string Name,
    string BaseUrl,
    SecretUpdateRequest ApiKey);
