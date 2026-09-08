namespace Portwise.Application.Dtos;

public sealed record SetupStockRequest(
    string SecurityCode,
    string ExchangeCode,
    InitialHoldingInput? InitialHolding);
