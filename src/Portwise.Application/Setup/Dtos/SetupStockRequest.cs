namespace Portwise.Application.Setup.Dtos;

public sealed record SetupStockRequest(
    string SecurityCode,
    string ExchangeCode,
    InitialHoldingInput? InitialHolding);
