using Portwise.Domain.Codes;

namespace Portwise.Domain.Models;

public sealed class FinancialSnapshot
{
    private FinancialSnapshot()
    {
    }

    public Guid Id { get; private set; }

    public Guid SecurityId { get; private set; }

    public DateOnly DataAsOfDate { get; private set; }

    public DateTimeOffset CapturedAt { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public decimal? EarningsPerShare { get; private set; }

    public decimal? DividendPayoutRatio { get; private set; }

    public decimal? ThreeYearAverageDividendPayoutRatio { get; private set; }

    public decimal? PriceToBookRatio { get; private set; }

    public decimal? ReturnOnEquity { get; private set; }

    public string DataSource { get; private set; } = string.Empty;

    public string SourceRecordId { get; private set; } = string.Empty;

    public string DataQualityCode { get; private set; } = string.Empty;

    public static FinancialSnapshot Create(
        Guid securityId,
        DateOnly dataAsOfDate,
        DateTimeOffset capturedAt,
        DateTimeOffset? publishedAt,
        decimal? earningsPerShare,
        decimal? dividendPayoutRatio,
        decimal? threeYearAverageDividendPayoutRatio,
        decimal? priceToBookRatio,
        decimal? returnOnEquity,
        string dataSource,
        string sourceRecordId,
        string dataQualityCode)
    {
        if (securityId == Guid.Empty)
        {
            throw new ArgumentException("Security identifier is required.", nameof(securityId));
        }

        if (dataAsOfDate == DateOnly.MinValue)
        {
            throw new ArgumentException("Data-as-of date is required.", nameof(dataAsOfDate));
        }

        if (capturedAt == default)
        {
            throw new ArgumentException("Capture timestamp is required.", nameof(capturedAt));
        }

        if (publishedAt is { } published && published == default)
        {
            throw new ArgumentException("Financial publication timestamp is required.", nameof(publishedAt));
        }

        if (string.IsNullOrWhiteSpace(dataSource))
        {
            throw new ArgumentException("Data source is required.", nameof(dataSource));
        }

        if (string.IsNullOrWhiteSpace(sourceRecordId))
        {
            throw new ArgumentException("Source record identifier is required.", nameof(sourceRecordId));
        }

        var normalizedQualityCode = dataQualityCode?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!DataQualityCodes.IsSupported(normalizedQualityCode))
        {
            throw new ArgumentException(
                "Data quality code is not supported.",
                nameof(dataQualityCode));
        }

        return new FinancialSnapshot
        {
            Id = Guid.NewGuid(),
            SecurityId = securityId,
            DataAsOfDate = dataAsOfDate,
            CapturedAt = capturedAt.ToUniversalTime(),
            PublishedAt = publishedAt?.ToUniversalTime(),
            EarningsPerShare = earningsPerShare,
            DividendPayoutRatio = dividendPayoutRatio,
            ThreeYearAverageDividendPayoutRatio = threeYearAverageDividendPayoutRatio,
            PriceToBookRatio = priceToBookRatio,
            ReturnOnEquity = returnOnEquity,
            DataSource = dataSource.Trim(),
            SourceRecordId = sourceRecordId.Trim(),
            DataQualityCode = normalizedQualityCode
        };
    }
}
