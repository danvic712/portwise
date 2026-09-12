namespace Portwise.Application.Initialization.Dtos;

/// <summary>
/// Describes one stock saved with the portfolio during onboarding.
/// </summary>
/// <param name="SecurityCode">Six-digit A-share security code.</param>
/// <param name="ExchangeCode">Exchange code.</param>
/// <param name="HeldShares">Number of shares currently held; use zero when there is no position.</param>
public sealed record InitialStockRequest(
    string SecurityCode,
    string ExchangeCode,
    int HeldShares);
