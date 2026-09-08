namespace Portwise.Application.Stocks.Dtos;

public sealed record SyncStockDividendsRequest(
    string SecurityCode,
    string ExchangeCode);
