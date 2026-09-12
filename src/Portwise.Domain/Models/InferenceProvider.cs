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

    public static InferenceProvider Create(
        string name,
        string baseUrl,
        string? protectedApiKey,
        DateTimeOffset createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrl);
        var timestamp = createdAtUtc.ToUniversalTime();
        return new InferenceProvider
        {
            Id = Guid.CreateVersion7(),
            Name = name.Trim(),
            NormalizedName = NormalizeName(name),
            ProviderType = InferenceProviderType.OpenAiCompatible,
            BaseUrl = baseUrl.Trim(),
            ProtectedApiKey = protectedApiKey,
            VerificationState = ProviderVerificationState.Unverified,
            Revision = 1,
            CreatedAtUtc = timestamp,
            UpdatedAtUtc = timestamp
        };
    }

    public void Update(
        string name,
        string baseUrl,
        bool connectionChanged,
        string? protectedApiKey,
        DateTimeOffset updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrl);
        Name = name.Trim();
        NormalizedName = NormalizeName(name);
        BaseUrl = baseUrl.Trim();
        if (connectionChanged)
        {
            ProtectedApiKey = protectedApiKey;
            VerificationState = ProviderVerificationState.Unverified;
            LastVerifiedAtUtc = null;
            LastVerificationErrorCode = null;
        }

        AdvanceRevision(updatedAtUtc);
    }

    public void MarkVerificationSucceeded(DateTimeOffset verifiedAtUtc)
    {
        VerificationState = ProviderVerificationState.Succeeded;
        LastVerifiedAtUtc = verifiedAtUtc.ToUniversalTime();
        LastVerificationErrorCode = null;
        AdvanceRevision(verifiedAtUtc);
    }

    public void MarkVerificationFailed(
        string errorCode,
        DateTimeOffset verifiedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);
        VerificationState = ProviderVerificationState.Failed;
        LastVerifiedAtUtc = verifiedAtUtc.ToUniversalTime();
        LastVerificationErrorCode = errorCode;
        AdvanceRevision(verifiedAtUtc);
    }

    public void ResetVerification(DateTimeOffset updatedAtUtc)
    {
        if (VerificationState == ProviderVerificationState.Unverified
            && LastVerifiedAtUtc is null
            && LastVerificationErrorCode is null)
        {
            return;
        }

        VerificationState = ProviderVerificationState.Unverified;
        LastVerifiedAtUtc = null;
        LastVerificationErrorCode = null;
        AdvanceRevision(updatedAtUtc);
    }

    public static string NormalizeName(string name) => name.Trim().ToUpperInvariant();

    private void AdvanceRevision(DateTimeOffset updatedAtUtc)
    {
        Revision = checked(Revision + 1);
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }
}
