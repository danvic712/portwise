namespace Portwise.Application.Dtos;

public sealed record SyncStockPriceRequest(
    string SecurityCode,
    string ExchangeCode);
