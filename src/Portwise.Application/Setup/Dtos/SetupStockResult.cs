namespace Portwise.Application.Setup.Dtos;

public sealed record SetupStockResult(
    string SecurityCode,
    string ExchangeCode,
    string? SecurityName);
