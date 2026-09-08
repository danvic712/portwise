namespace Portwise.Application.Dtos;

public sealed record SetupStockResult(
    string SecurityCode,
    string ExchangeCode,
    string? SecurityName);
