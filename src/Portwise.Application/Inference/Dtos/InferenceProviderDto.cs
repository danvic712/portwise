using Portwise.Application.Configuration.Dtos;

namespace Portwise.Application.Inference.Dtos;

/// <summary>
/// Describes one inference provider without returning its API key.
/// </summary>
/// <param name="Id">The provider identifier.</param>
/// <param name="Name">The user-facing provider name.</param>
/// <param name="ProviderTypeCode">The stable provider protocol code.</param>
/// <param name="BaseUrl">The current provider service address.</param>
/// <param name="IsBaseUrlEditable">Whether the service address can be changed by the user.</param>
/// <param name="SecretState">The state of the protected API key.</param>
/// <param name="RuntimeStatusCode">The effective runtime readiness state.</param>
/// <param name="VerificationStateCode">The last connection verification state.</param>
/// <param name="LastVerifiedAtUtc">The time of the last verification attempt.</param>
/// <param name="LastVerificationErrorCode">The stable error code from the last failed verification.</param>
/// <param name="Revision">The optimistic concurrency revision.</param>
/// <param name="UpdatedAtUtc">The time the provider was last changed.</param>
public sealed record InferenceProviderDto(
    Guid Id,
    string Name,
    string ProviderTypeCode,
    string BaseUrl,
    bool IsBaseUrlEditable,
    SecretStateDto SecretState,
    string RuntimeStatusCode,
    string VerificationStateCode,
    DateTimeOffset? LastVerifiedAtUtc,
    string? LastVerificationErrorCode,
    long Revision,
    DateTimeOffset UpdatedAtUtc);
