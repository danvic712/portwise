using Portwise.Application.Configuration.Dtos;

namespace Portwise.Application.StockDataProviders.Dtos;

/// <summary>
/// Describes one configured stock data provider without returning its credentials.
/// </summary>
public sealed record StockDataProviderDto(
    Guid Id,
    Guid ProviderDefinitionId,
    string ProviderKindCode,
    string Name,
    SecretStateDto SecretState,
    string RuntimeStatusCode,
    string VerificationStateCode,
    DateTimeOffset? LastVerifiedAtUtc,
    string? LastVerificationErrorCode,
    long Revision,
    DateTimeOffset UpdatedAtUtc);
