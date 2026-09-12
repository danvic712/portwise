namespace Portwise.Application.StockDataProviders.Dtos;

/// <summary>
/// Describes the provider binding and effective status for one stock data capability.
/// </summary>
public sealed record StockDataRouteDto(
    Guid Id,
    string CapabilityCode,
    Guid? ProviderId,
    string RuntimeStatusCode,
    long Revision,
    DateTimeOffset UpdatedAtUtc);
