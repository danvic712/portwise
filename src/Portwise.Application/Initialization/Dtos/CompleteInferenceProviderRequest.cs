using Portwise.Application.Configuration.Dtos;

namespace Portwise.Application.Initialization.Dtos;

/// <summary>
/// Describes an existing inference provider connection to save during onboarding.
/// </summary>
/// <param name="ProviderId">The existing inference provider identifier.</param>
/// <param name="ApiKey">The explicit encrypted-key operation.</param>
/// <param name="BaseUrl">The optional service address for providers that allow customization.</param>
public sealed record CompleteInferenceProviderRequest(
    Guid ProviderId,
    SecretUpdateRequest ApiKey,
    string? BaseUrl = null);
