using Portwise.Application.Configuration.Dtos;

namespace Portwise.Application.Inference.Dtos;

/// <summary>
/// Describes one inference provider without returning its API key.
/// </summary>
public sealed record InferenceProviderDto(
    Guid Id,
    string Name,
    string ProviderTypeCode,
    string BaseUrl,
    SecretStateDto SecretState,
    string RuntimeStatusCode,
    string VerificationStateCode,
    DateTimeOffset? LastVerifiedAtUtc,
    string? LastVerificationErrorCode,
    long Revision,
    DateTimeOffset UpdatedAtUtc);
