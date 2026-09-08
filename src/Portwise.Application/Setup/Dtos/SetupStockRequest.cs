namespace Portwise.Application.Setup.Dtos;

/// <summary>Stock identity and optional initial holding used during setup.</summary>
/// <param name="SecurityCode">Six-digit security code.</param>
/// <param name="ExchangeCode">Exchange code.</param>
/// <param name="InitialHolding">Optional initial holding values.</param>
public sealed record SetupStockRequest(
    string SecurityCode,
    string ExchangeCode,
    InitialHoldingInput? InitialHolding);
