namespace Portwise.Application.Stocks.Dtos;

public sealed record SyncStockFinancialsRequest(
    string SecurityCode,
    string ExchangeCode);
