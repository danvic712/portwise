using Portwise.Domain.Codes;

namespace Portwise.Domain.Models;

public sealed class DividendEvent
{
    private DividendEvent()
    {
    }

    public Guid Id { get; private set; }

    public Guid SecurityId { get; private set; }

    public decimal DividendPerShare { get; private set; }

    public string DividendTypeCode { get; private set; } = string.Empty;

    public string DividendStatusCode { get; private set; } = string.Empty;

    public DateOnly? AnnouncementDate { get; private set; }

    public DateOnly? ExDividendDate { get; private set; }

    public DateOnly? PaymentDate { get; private set; }

    public bool IsSpecialDividend { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public DateTimeOffset CapturedAt { get; private set; }

    public string DataSource { get; private set; } = string.Empty;

    public string SourceRecordId { get; private set; } = string.Empty;

    public string DataQualityCode { get; private set; } = string.Empty;

    public static DividendEvent Create(
        Guid securityId,
        decimal dividendPerShare,
        string dividendTypeCode,
        string dividendStatusCode,
        DateOnly? announcementDate,
        DateOnly? exDividendDate,
        DateOnly? paymentDate,
        bool isSpecialDividend,
        DateTimeOffset? publishedAt,
        DateTimeOffset capturedAt,
        string dataSource,
        string sourceRecordId,
        string dataQualityCode)
    {
        if (securityId == Guid.Empty)
        {
            throw new ArgumentException("Security identifier is required.", nameof(securityId));
        }

        if (dividendPerShare <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dividendPerShare),
                dividendPerShare,
                "Dividend per share must be greater than zero.");
        }

        var normalizedDividendType = dividendTypeCode?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!DividendTypeCodes.IsSupported(normalizedDividendType))
        {
            throw new ArgumentException(
                "Dividend type must be regular_cash or special_cash.",
                nameof(dividendTypeCode));
        }

        var normalizedDividendStatus = dividendStatusCode?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!DividendStatusCodes.IsSupported(normalizedDividendStatus))
        {
            throw new ArgumentException(
                "Dividend status must be implemented, proposed or cancelled.",
                nameof(dividendStatusCode));
        }

        ValidateDate(announcementDate, nameof(announcementDate));
        ValidateDate(exDividendDate, nameof(exDividendDate));
        ValidateDate(paymentDate, nameof(paymentDate));

        if (publishedAt is { } published && published == default)
        {
            throw new ArgumentException("Dividend publication timestamp is required.", nameof(publishedAt));
        }

        if (capturedAt == default)
        {
            throw new ArgumentException("Capture timestamp is required.", nameof(capturedAt));
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

        return new DividendEvent
        {
            Id = Guid.NewGuid(),
            SecurityId = securityId,
            DividendPerShare = dividendPerShare,
            DividendTypeCode = normalizedDividendType,
            DividendStatusCode = normalizedDividendStatus,
            AnnouncementDate = announcementDate,
            ExDividendDate = exDividendDate,
            PaymentDate = paymentDate,
            IsSpecialDividend = isSpecialDividend,
            PublishedAt = publishedAt?.ToUniversalTime(),
            CapturedAt = capturedAt.ToUniversalTime(),
            DataSource = dataSource.Trim(),
            SourceRecordId = sourceRecordId.Trim(),
            DataQualityCode = normalizedQualityCode
        };
    }

    private static void ValidateDate(DateOnly? date, string parameterName)
    {
        if (date == DateOnly.MinValue)
        {
            throw new ArgumentException("Date is required.", parameterName);
        }
    }
}
