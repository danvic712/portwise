namespace Portwise.Application.Initialization.Dtos;

/// <summary>
/// Describes an optional stock data capability binding by provider name.
/// </summary>
public sealed record CompleteStockDataRouteRequest(
    string CapabilityCode,
    string? ProviderName);
