namespace Portwise.Application.Stocks.Dtos;

/// <summary>Persisted price observation returned by the API.</summary>
/// <param name="PriceObservationId">Identifier of the persisted observation.</param>
/// <param name="SecurityCode">Security code.</param>
/// <param name="ExchangeCode">Exchange code.</param>
/// <param name="TradingDate">Trading date.</param>
/// <param name="ClosePrice">Closing price.</param>
/// <param name="PriceObservedAt">UTC observation timestamp.</param>
/// <param name="DataSource">Source-system name.</param>
/// <param name="SourceRecordId">Source-system record identifier.</param>
/// <param name="DataQualityCode">Normalized data quality code.</param>
public sealed record StockPriceObservationResult(
    Guid PriceObservationId,
    string SecurityCode,
    string ExchangeCode,
    DateOnly TradingDate,
    decimal ClosePrice,
    DateTimeOffset PriceObservedAt,
    string DataSource,
    string SourceRecordId,
    string DataQualityCode);
