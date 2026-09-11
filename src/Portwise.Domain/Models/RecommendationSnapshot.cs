using Portwise.Domain.Codes;

namespace Portwise.Domain.Models;

public sealed class RecommendationSnapshot
{
    private RecommendationSnapshot()
    {
    }

    public Guid Id { get; private set; }

    public Guid ModelRunId { get; private set; }

    public Guid PortfolioId { get; private set; }

    public Guid SecurityId { get; private set; }

    public DateOnly? DataAsOfDate { get; private set; }

    public decimal? ClosePrice { get; private set; }

    public decimal? ModelDividendPerShare { get; private set; }

    public string? DividendModeCode { get; private set; }

    public string ModelStatusCode { get; private set; } = string.Empty;

    public string DividendReliabilityCode { get; private set; } = string.Empty;

    public string? ObservedPriceZoneCode { get; private set; }

    public string? PriceZoneCode { get; private set; }

    public bool PriceZoneConfirmed { get; private set; }

    public string RecommendationCode { get; private set; } = string.Empty;

    public decimal? DividendYield { get; private set; }

    public int SuggestedBuyShares { get; private set; }

    public int SuggestedSellShares { get; private set; }

    public decimal SuggestedTradeAmount { get; private set; }

    public decimal EstimatedTransactionFeeAmount { get; private set; }

    public DateTimeOffset ComputedAt { get; private set; }

    public Guid? ModelParameterSetId { get; private set; }

    public static RecommendationSnapshot Create(
        Guid modelRunId,
        Guid portfolioId,
        Guid securityId,
        DateOnly? dataAsOfDate,
        decimal? closePrice,
        decimal? modelDividendPerShare,
        string? dividendModeCode,
        string modelStatusCode,
        string dividendReliabilityCode,
        string? observedPriceZoneCode,
        string? priceZoneCode,
        bool priceZoneConfirmed,
        string recommendationCode,
        decimal? dividendYield,
        int suggestedBuyShares,
        int suggestedSellShares,
        decimal suggestedTradeAmount,
        decimal estimatedTransactionFeeAmount,
        DateTimeOffset computedAt,
        Guid? modelParameterSetId)
    {
        if (modelRunId == Guid.Empty)
        {
            throw new ArgumentException("Model run identifier is required.", nameof(modelRunId));
        }

        if (portfolioId == Guid.Empty)
        {
            throw new ArgumentException("Portfolio identifier is required.", nameof(portfolioId));
        }

        if (securityId == Guid.Empty)
        {
            throw new ArgumentException("Security identifier is required.", nameof(securityId));
        }

        ValidateOptionalPositive(closePrice, nameof(closePrice), "Close price must be greater than zero.");
        ValidateOptionalPositive(
            modelDividendPerShare,
            nameof(modelDividendPerShare),
            "Model dividend per share must be greater than zero.");
        ValidateOptionalRatio(dividendYield, nameof(dividendYield));

        var normalizedModelStatus = modelStatusCode?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!ModelStatusCodes.IsSupported(normalizedModelStatus))
        {
            throw new ArgumentException("Model status code is not supported.", nameof(modelStatusCode));
        }

        var normalizedReliability =
            dividendReliabilityCode?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!DividendReliabilityCodes.IsSupported(normalizedReliability))
        {
            throw new ArgumentException(
                "Dividend reliability code is not supported.",
                nameof(dividendReliabilityCode));
        }

        var normalizedDividendMode = NormalizeOptionalCode(dividendModeCode);
        if (normalizedDividendMode is not null
            && !DividendModeCodes.IsSupported(normalizedDividendMode))
        {
            throw new ArgumentException("Dividend mode code is not supported.", nameof(dividendModeCode));
        }

        var normalizedObservedPriceZone = NormalizeOptionalCode(observedPriceZoneCode);
        if (normalizedObservedPriceZone is not null
            && !PriceZoneCodes.IsSupported(normalizedObservedPriceZone))
        {
            throw new ArgumentException(
                "Observed price-zone code is not supported.",
                nameof(observedPriceZoneCode));
        }

        var normalizedPriceZone = NormalizeOptionalCode(priceZoneCode);
        if (normalizedPriceZone is not null && !PriceZoneCodes.IsSupported(normalizedPriceZone))
        {
            throw new ArgumentException("Confirmed price-zone code is not supported.", nameof(priceZoneCode));
        }

        var normalizedRecommendation = recommendationCode?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!RecommendationCodes.IsSupported(normalizedRecommendation))
        {
            throw new ArgumentException("Recommendation code is not supported.", nameof(recommendationCode));
        }

        if (suggestedBuyShares < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(suggestedBuyShares),
                suggestedBuyShares,
                "Suggested buy shares cannot be negative.");
        }

        if (suggestedSellShares < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(suggestedSellShares),
                suggestedSellShares,
                "Suggested sell shares cannot be negative.");
        }

        if (suggestedTradeAmount < 0 || estimatedTransactionFeeAmount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(suggestedTradeAmount),
                "Suggested trade amount and fee cannot be negative.");
        }

        if (computedAt == default)
        {
            throw new ArgumentException("Computation timestamp is required.", nameof(computedAt));
        }

        ValidateSnapshotSemantics(
            normalizedDividendMode,
            modelDividendPerShare,
            normalizedModelStatus,
            normalizedReliability,
            normalizedObservedPriceZone,
            normalizedPriceZone,
            priceZoneConfirmed,
            normalizedRecommendation,
            closePrice,
            suggestedBuyShares,
            suggestedSellShares,
            suggestedTradeAmount,
            estimatedTransactionFeeAmount);

        return new RecommendationSnapshot
        {
            Id = Guid.CreateVersion7(),
            ModelRunId = modelRunId,
            PortfolioId = portfolioId,
            SecurityId = securityId,
            DataAsOfDate = dataAsOfDate,
            ClosePrice = closePrice,
            ModelDividendPerShare = modelDividendPerShare,
            DividendModeCode = normalizedDividendMode,
            ModelStatusCode = normalizedModelStatus,
            DividendReliabilityCode = normalizedReliability,
            ObservedPriceZoneCode = normalizedObservedPriceZone,
            PriceZoneCode = normalizedPriceZone,
            PriceZoneConfirmed = priceZoneConfirmed,
            RecommendationCode = normalizedRecommendation,
            DividendYield = dividendYield,
            SuggestedBuyShares = suggestedBuyShares,
            SuggestedSellShares = suggestedSellShares,
            SuggestedTradeAmount = suggestedTradeAmount,
            EstimatedTransactionFeeAmount = estimatedTransactionFeeAmount,
            ComputedAt = computedAt.ToUniversalTime(),
            ModelParameterSetId = modelParameterSetId
        };
    }

    private static void ValidateOptionalPositive(
        decimal? value,
        string parameterName,
        string message)
    {
        if (value is <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, message);
        }
    }

    private static void ValidateOptionalRatio(decimal? value, string parameterName)
    {
        if (value is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Dividend yield must be between 0 and 1.");
        }
    }

    private static string? NormalizeOptionalCode(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim().ToLowerInvariant();

    private static void ValidateSnapshotSemantics(
        string? dividendModeCode,
        decimal? modelDividendPerShare,
        string modelStatusCode,
        string reliabilityCode,
        string? observedPriceZoneCode,
        string? priceZoneCode,
        bool priceZoneConfirmed,
        string recommendationCode,
        decimal? closePrice,
        int suggestedBuyShares,
        int suggestedSellShares,
        decimal suggestedTradeAmount,
        decimal estimatedTransactionFeeAmount)
    {
        if ((modelDividendPerShare is null) != (dividendModeCode is null))
        {
            throw new ArgumentException("Model dividend and dividend mode must both be present or both be empty.", nameof(dividendModeCode));
        }

        if (priceZoneConfirmed != (priceZoneCode is not null))
        {
            throw new ArgumentException(
                "Price-zone confirmation must be provided together with a confirmed price zone.",
                nameof(priceZoneConfirmed));
        }

        if (priceZoneConfirmed
            && !string.Equals(observedPriceZoneCode, priceZoneCode, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Confirmed price zone must match the latest observed price zone.",
                nameof(priceZoneCode));
        }

        if (modelStatusCode == ModelStatusCodes.Unavailable
            && (observedPriceZoneCode is not null || priceZoneCode is not null))
        {
            throw new ArgumentException(
                "Price zones cannot be saved when the model is unavailable.",
                nameof(priceZoneCode));
        }

        if (modelStatusCode == ModelStatusCodes.Available
            && reliabilityCode != DividendReliabilityCodes.Passed)
        {
            throw new ArgumentException(
                "Dividend reliability must be passed when the model is available.",
                nameof(reliabilityCode));
        }

        var mustHaveNoTrade = modelStatusCode is
            ModelStatusCodes.Cautious
            or ModelStatusCodes.Failed
            or ModelStatusCodes.ReEvaluate
            or ModelStatusCodes.Unavailable;
        if (mustHaveNoTrade && (suggestedBuyShares > 0 || suggestedSellShares > 0))
        {
            throw new ArgumentException(
                "Trade shares cannot be generated when the model is not available.",
                nameof(suggestedBuyShares));
        }

        var expectedRecommendation = modelStatusCode switch
        {
            ModelStatusCodes.Unavailable or ModelStatusCodes.Failed => RecommendationCodes.NoAction,
            ModelStatusCodes.ReEvaluate => RecommendationCodes.ReEvaluate,
            ModelStatusCodes.Cautious => recommendationCode is RecommendationCodes.Hold
                or RecommendationCodes.NoAction
                ? recommendationCode
                : throw new ArgumentException(
                    "Cautious status can only use hold or no_action.",
                    nameof(recommendationCode)),
            _ when priceZoneCode is null => recommendationCode is RecommendationCodes.Hold
                or RecommendationCodes.NoAction
                ? recommendationCode
                : throw new ArgumentException(
                    "An unconfirmed price zone can only use hold or no_action.",
                    nameof(recommendationCode)),
            _ => priceZoneCode
        };
        if (!string.Equals(expectedRecommendation, recommendationCode, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Recommendation code does not match the model status or price zone.",
                nameof(recommendationCode));
        }

        if (suggestedBuyShares > 0 && suggestedSellShares > 0)
        {
            throw new ArgumentException("A recommendation snapshot cannot contain both buy and sell shares.", nameof(suggestedBuyShares));
        }

        if (suggestedBuyShares > 0 || suggestedSellShares > 0)
        {
            if (closePrice is null)
            {
                throw new ArgumentException("Close price is required when suggested shares are present.", nameof(closePrice));
            }

            var expectedTradeAmount =
                (suggestedBuyShares + suggestedSellShares) * closePrice.Value;
            if (suggestedTradeAmount != expectedTradeAmount)
            {
                throw new ArgumentException(
                    "Suggested trade amount must equal suggested shares multiplied by close price.",
                    nameof(suggestedTradeAmount));
            }
        }
        else if (suggestedTradeAmount != 0m || estimatedTransactionFeeAmount != 0m)
        {
            throw new ArgumentException(
                "Suggested trade amount and fee must be zero when no suggested shares exist.",
                nameof(suggestedTradeAmount));
        }

        if (estimatedTransactionFeeAmount > 0m && suggestedTradeAmount == 0m)
        {
            throw new ArgumentException(
                "Suggested trade amount is required when a transaction fee is present.",
                nameof(estimatedTransactionFeeAmount));
        }
    }
}
