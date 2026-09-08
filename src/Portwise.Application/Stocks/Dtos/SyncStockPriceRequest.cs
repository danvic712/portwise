namespace Portwise.Application.Stocks.Dtos;

public sealed record SyncStockPriceRequest(
    string SecurityCode,
    string ExchangeCode);
