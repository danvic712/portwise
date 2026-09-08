using Portwise.Application.Dtos;

namespace Portwise.Application.Stocks.Dtos;

/// <summary>Configured stock identity with its latest holding snapshot.</summary>
/// <param name="SecurityCode">Security code.</param>
/// <param name="ExchangeCode">Exchange code.</param>
/// <param name="SecurityName">Display name of the security.</param>
/// <param name="MarketCode">Normalized market code.</param>
/// <param name="CurrencyCode">Normalized currency code.</param>
/// <param name="Holding">Latest holding snapshot, when available.</param>
/// <param name="SectorCode">Optional normalized sector code.</param>
public sealed record StockWatchlistItem(
    string SecurityCode,
    string ExchangeCode,
    string SecurityName,
    string MarketCode,
    string CurrencyCode,
    StockHoldingSnapshot? Holding,
    string? SectorCode = null)
{
    /// <summary>Persistent identifier of the configured security.</summary>
    public Guid SecurityId { get; init; }
}
