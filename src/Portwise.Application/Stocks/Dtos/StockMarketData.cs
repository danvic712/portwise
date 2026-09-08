namespace Portwise.Application.Stocks.Dtos;

/// <summary>Normalized market price data returned by the provider.</summary>
/// <param name="SecurityCode">Security code.</param>
/// <param name="ExchangeCode">Exchange code.</param>
/// <param name="ClosePrice">Closing price.</param>
/// <param name="TradingDate">Trading date.</param>
/// <param name="PriceObservedAt">UTC observation timestamp.</param>
/// <param name="DataSource">Source-system name.</param>
/// <param name="SourceRecordId">Source-system record identifier.</param>
/// <param name="DataQualityCode">Normalized data quality code.</param>
public sealed record StockMarketData(
    string SecurityCode,
    string ExchangeCode,
    decimal ClosePrice,
    DateOnly TradingDate,
    DateTimeOffset PriceObservedAt,
    string DataSource,
    string SourceRecordId,
    string DataQualityCode);
