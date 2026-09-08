namespace Portwise.Application.Stocks.Dtos;

/// <summary>Persisted financial snapshot returned by the API.</summary>
/// <param name="FinancialSnapshotId">Identifier of the persisted financial snapshot.</param>
/// <param name="SecurityCode">Security code.</param>
/// <param name="ExchangeCode">Exchange code.</param>
/// <param name="DataAsOfDate">Date through which the data is valid.</param>
/// <param name="CapturedAt">UTC capture timestamp.</param>
/// <param name="PublishedAt">Publication timestamp, when available.</param>
/// <param name="EarningsPerShare">Earnings per share, when available.</param>
/// <param name="DividendPayoutRatio">Dividend payout ratio, when available.</param>
/// <param name="ThreeYearAverageDividendPayoutRatio">Three-year average payout ratio.</param>
/// <param name="PriceToBookRatio">Price-to-book ratio, when available.</param>
/// <param name="ReturnOnEquity">Return on equity, when available.</param>
/// <param name="DataSource">Source-system name.</param>
/// <param name="SourceRecordId">Source-system record identifier.</param>
/// <param name="DataQualityCode">Normalized data quality code.</param>
public sealed record StockFinancialSnapshotResult(
    Guid FinancialSnapshotId,
    string SecurityCode,
    string ExchangeCode,
    DateOnly DataAsOfDate,
    DateTimeOffset CapturedAt,
    DateTimeOffset? PublishedAt,
    decimal? EarningsPerShare,
    decimal? DividendPayoutRatio,
    decimal? ThreeYearAverageDividendPayoutRatio,
    decimal? PriceToBookRatio,
    decimal? ReturnOnEquity,
    string DataSource,
    string SourceRecordId,
    string DataQualityCode);
