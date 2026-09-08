using Portwise.Domain.Codes;

namespace Portwise.Domain.Models;

public sealed class PriceObservation
{
    private PriceObservation()
    {
    }

    public Guid Id { get; private set; }

    public Guid SecurityId { get; private set; }

    public DateOnly TradingDate { get; private set; }

    public decimal ClosePrice { get; private set; }

    public DateTimeOffset PriceObservedAt { get; private set; }

    public string DataSource { get; private set; } = string.Empty;

    public string SourceRecordId { get; private set; } = string.Empty;

    public string DataQualityCode { get; private set; } = string.Empty;

    public static PriceObservation Create(
        Guid securityId,
        DateOnly tradingDate,
        decimal closePrice,
        DateTimeOffset priceObservedAt,
        string dataSource,
        string sourceRecordId,
        string dataQualityCode)
    {
        if (securityId == Guid.Empty)
        {
            throw new ArgumentException("Security identifier is required.", nameof(securityId));
        }

        if (tradingDate == DateOnly.MinValue)
        {
            throw new ArgumentException("Trading date is required.", nameof(tradingDate));
        }

        if (closePrice <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(closePrice),
                closePrice,
                "Close price must be greater than zero.");
        }

        if (priceObservedAt == default)
        {
            throw new ArgumentException("Price observation timestamp is required.", nameof(priceObservedAt));
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
            throw new ArgumentException("Data quality code is not supported.", nameof(dataQualityCode));
        }

        return new PriceObservation
        {
            Id = Guid.NewGuid(),
            SecurityId = securityId,
            TradingDate = tradingDate,
            ClosePrice = closePrice,
            PriceObservedAt = priceObservedAt.ToUniversalTime(),
            DataSource = dataSource.Trim(),
            SourceRecordId = sourceRecordId.Trim(),
            DataQualityCode = normalizedQualityCode
        };
    }
}
