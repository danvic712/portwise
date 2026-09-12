using Portwise.Domain.Enums;

namespace Portwise.Domain.Models;

/// <summary>
/// Stores a user-configured stock data provider instance and its protected credentials.
/// </summary>
public sealed class StockDataProvider
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid ProviderDefinitionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ProtectedCredentials { get; set; }

    public ProviderVerificationState VerificationState { get; set; } =
        ProviderVerificationState.Unverified;

    public DateTimeOffset? LastVerifiedAtUtc { get; set; }

    public string? LastVerificationErrorCode { get; set; }

    public long Revision { get; set; } = 1;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public static StockDataProvider Create(
        Guid providerDefinitionId,
        string name,
        string? protectedCredentials,
        DateTimeOffset createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var timestamp = createdAtUtc.ToUniversalTime();
        return new StockDataProvider
        {
            Id = Guid.CreateVersion7(),
            ProviderDefinitionId = providerDefinitionId,
            Name = name.Trim(),
            ProtectedCredentials = protectedCredentials,
            VerificationState = ProviderVerificationState.Unverified,
            Revision = 1,
            CreatedAtUtc = timestamp,
            UpdatedAtUtc = timestamp
        };
    }

    public void Update(
        string name,
        bool credentialsChanged,
        string? protectedCredentials,
        DateTimeOffset updatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        if (credentialsChanged)
        {
            ProtectedCredentials = protectedCredentials;
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

    private void AdvanceRevision(DateTimeOffset updatedAtUtc)
    {
        Revision = checked(Revision + 1);
        UpdatedAtUtc = updatedAtUtc.ToUniversalTime();
    }
}
