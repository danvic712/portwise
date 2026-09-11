using Portwise.Domain.Enums;

namespace Portwise.Domain.Models;

/// <summary>
/// Stores a reusable OpenAI-compatible provider connection.
/// </summary>
public sealed class InferenceProvider
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public InferenceProviderType ProviderType { get; set; } =
        InferenceProviderType.OpenAiCompatible;

    public string BaseUrl { get; set; } = string.Empty;

    public string? ProtectedApiKey { get; set; }

    public ProviderVerificationState VerificationState { get; set; } =
        ProviderVerificationState.Unverified;

    public DateTimeOffset? LastVerifiedAtUtc { get; set; }

    public string? LastVerificationErrorCode { get; set; }

    public long Revision { get; set; } = 1;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
