namespace Portwise.Application.Setup.Dtos;

/// <summary>Result describing a stock created during setup.</summary>
/// <param name="SecurityCode">Created security code.</param>
/// <param name="ExchangeCode">Created exchange code.</param>
/// <param name="SecurityName">Resolved display name, when available.</param>
public sealed record SetupStockResult(
    string SecurityCode,
    string ExchangeCode,
    string? SecurityName);
