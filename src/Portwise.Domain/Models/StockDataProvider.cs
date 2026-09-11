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
}
