using Portwise.Application.Dtos;

namespace Portwise.Application.Stocks.Dtos;

public sealed record StockWatchlistItem(
    string SecurityCode,
    string ExchangeCode,
    string SecurityName,
    string MarketCode,
    string CurrencyCode,
    StockHoldingSnapshot? Holding,
    string? SectorCode = null)
{
    public Guid SecurityId { get; init; }
}
