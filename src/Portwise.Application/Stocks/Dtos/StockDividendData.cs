namespace Portwise.Application.Stocks.Dtos;

/// <summary>Normalized dividend event data returned by the provider.</summary>
/// <param name="SecurityCode">Security code.</param>
/// <param name="ExchangeCode">Exchange code.</param>
/// <param name="DividendPerShare">Dividend amount per share.</param>
/// <param name="DividendTypeCode">Normalized dividend type code.</param>
/// <param name="DividendStatusCode">Normalized dividend status code.</param>
/// <param name="AnnouncementDate">Announcement date, when available.</param>
/// <param name="ExDividendDate">Ex-dividend date, when available.</param>
/// <param name="PaymentDate">Payment date, when available.</param>
/// <param name="IsSpecialDividend">Whether the dividend is special.</param>
/// <param name="PublishedAt">Publication timestamp, when available.</param>
/// <param name="CapturedAt">UTC capture timestamp.</param>
/// <param name="DataSource">Source-system name.</param>
/// <param name="SourceRecordId">Source-system record identifier.</param>
/// <param name="DataQualityCode">Normalized data quality code.</param>
public sealed record StockDividendData(
    string SecurityCode,
    string ExchangeCode,
    decimal DividendPerShare,
    string DividendTypeCode,
    string DividendStatusCode,
    DateOnly? AnnouncementDate,
    DateOnly? ExDividendDate,
    DateOnly? PaymentDate,
    bool IsSpecialDividend,
    DateTimeOffset? PublishedAt,
    DateTimeOffset CapturedAt,
    string DataSource,
    string SourceRecordId,
    string DataQualityCode);
