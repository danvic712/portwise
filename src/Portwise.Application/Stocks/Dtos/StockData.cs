namespace Portwise.Application.Stocks.Dtos;

/// <summary>Normalized stock profile returned by the data provider.</summary>
/// <param name="SecurityCode">Security code.</param>
/// <param name="ExchangeCode">Exchange code.</param>
/// <param name="SecurityName">Display name of the security.</param>
/// <param name="MarketCode">Normalized market code.</param>
/// <param name="CurrencyCode">Normalized currency code.</param>
/// <param name="SectorCode">Optional normalized sector code.</param>
public sealed record StockData(
    string SecurityCode,
    string ExchangeCode,
    string SecurityName,
    string MarketCode,
    string CurrencyCode,
    string? SectorCode = null);
