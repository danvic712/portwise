namespace Portwise.Domain.Models;

public sealed class ModelParameterSet
{
    private ModelParameterSet()
    {
    }

    public Guid Id { get; private set; }

    public Guid PortfolioId { get; private set; }

    public Guid SecurityId { get; private set; }

    public string ModelVersion { get; private set; } = string.Empty;

    public decimal StrongBuyYieldThreshold { get; private set; }

    public decimal AccumulationYieldThreshold { get; private set; }

    public decimal PartialTrimYieldThreshold { get; private set; }

    public decimal AggressiveTrimYieldThreshold { get; private set; }

    public decimal StrongBuyBudgetRatio { get; private set; }

    public decimal AccumulateBudgetRatio { get; private set; }

    public decimal PartialTrimRatio { get; private set; }

    public decimal AggressiveTrimRatio { get; private set; }

    public decimal MaxSecurityWeight { get; private set; }

    public decimal MaxSectorWeight { get; private set; }

    public decimal CashReserveRatio { get; private set; }

    public decimal MaxSingleTradeAmount { get; private set; }

    public decimal MaxPeriodBudgetAmount { get; private set; }

    public decimal TransactionFeeRatio { get; private set; }

    public decimal MinimumTransactionFeeAmount { get; private set; }

    public int TradingLotSize { get; private set; }

    public DateOnly EffectiveFromDate { get; private set; }

    public static ModelParameterSet Create(
        Guid portfolioId,
        Guid securityId,
        string modelVersion,
        decimal strongBuyYieldThreshold,
        decimal accumulationYieldThreshold,
        decimal partialTrimYieldThreshold,
        decimal aggressiveTrimYieldThreshold,
        decimal strongBuyBudgetRatio,
        decimal accumulateBudgetRatio,
        decimal partialTrimRatio,
        decimal aggressiveTrimRatio,
        decimal maxSecurityWeight,
        decimal maxSectorWeight,
        decimal cashReserveRatio,
        decimal maxSingleTradeAmount,
        decimal maxPeriodBudgetAmount,
        decimal transactionFeeRatio,
        decimal minimumTransactionFeeAmount,
        int tradingLotSize,
        DateOnly effectiveFromDate)
    {
        if (portfolioId == Guid.Empty)
        {
            throw new ArgumentException("Portfolio identifier is required.", nameof(portfolioId));
        }

        if (securityId == Guid.Empty)
        {
            throw new ArgumentException("Security identifier is required.", nameof(securityId));
        }

        if (string.IsNullOrWhiteSpace(modelVersion))
        {
            throw new ArgumentException("Model version is required.", nameof(modelVersion));
        }

        EnsurePositiveRatio(strongBuyYieldThreshold, nameof(strongBuyYieldThreshold));
        EnsurePositiveRatio(accumulationYieldThreshold, nameof(accumulationYieldThreshold));
        EnsurePositiveRatio(partialTrimYieldThreshold, nameof(partialTrimYieldThreshold));
        EnsurePositiveRatio(aggressiveTrimYieldThreshold, nameof(aggressiveTrimYieldThreshold));

        if (strongBuyYieldThreshold <= accumulationYieldThreshold)
        {
            throw new ArgumentException(
                "Strong-buy yield threshold must exceed the accumulation threshold.",
                nameof(strongBuyYieldThreshold));
        }

        if (accumulationYieldThreshold <= partialTrimYieldThreshold)
        {
            throw new ArgumentException(
                "Accumulation yield threshold must exceed the partial-trim threshold.",
                nameof(partialTrimYieldThreshold));
        }

        if (partialTrimYieldThreshold <= aggressiveTrimYieldThreshold)
        {
            throw new ArgumentException(
                "Partial-trim yield threshold must exceed the aggressive-trim threshold.",
                nameof(aggressiveTrimYieldThreshold));
        }

        EnsureRatio(strongBuyBudgetRatio, nameof(strongBuyBudgetRatio));
        EnsureRatio(accumulateBudgetRatio, nameof(accumulateBudgetRatio));
        EnsureRatio(partialTrimRatio, nameof(partialTrimRatio));
        EnsureRatio(aggressiveTrimRatio, nameof(aggressiveTrimRatio));
        EnsureRatio(maxSecurityWeight, nameof(maxSecurityWeight));
        EnsureRatio(maxSectorWeight, nameof(maxSectorWeight));
        EnsureRatio(cashReserveRatio, nameof(cashReserveRatio));
        EnsureNonNegative(maxSingleTradeAmount, nameof(maxSingleTradeAmount));
        EnsureNonNegative(maxPeriodBudgetAmount, nameof(maxPeriodBudgetAmount));
        EnsureRatio(transactionFeeRatio, nameof(transactionFeeRatio));
        EnsureNonNegative(minimumTransactionFeeAmount, nameof(minimumTransactionFeeAmount));

        if (tradingLotSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tradingLotSize),
                tradingLotSize,
                "Trading lot size must be greater than zero.");
        }

        if (effectiveFromDate == DateOnly.MinValue)
        {
            throw new ArgumentException("Effective-from date is required.", nameof(effectiveFromDate));
        }

        return new ModelParameterSet
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolioId,
            SecurityId = securityId,
            ModelVersion = modelVersion.Trim(),
            StrongBuyYieldThreshold = strongBuyYieldThreshold,
            AccumulationYieldThreshold = accumulationYieldThreshold,
            PartialTrimYieldThreshold = partialTrimYieldThreshold,
            AggressiveTrimYieldThreshold = aggressiveTrimYieldThreshold,
            StrongBuyBudgetRatio = strongBuyBudgetRatio,
            AccumulateBudgetRatio = accumulateBudgetRatio,
            PartialTrimRatio = partialTrimRatio,
            AggressiveTrimRatio = aggressiveTrimRatio,
            MaxSecurityWeight = maxSecurityWeight,
            MaxSectorWeight = maxSectorWeight,
            CashReserveRatio = cashReserveRatio,
            MaxSingleTradeAmount = maxSingleTradeAmount,
            MaxPeriodBudgetAmount = maxPeriodBudgetAmount,
            TransactionFeeRatio = transactionFeeRatio,
            MinimumTransactionFeeAmount = minimumTransactionFeeAmount,
            TradingLotSize = tradingLotSize,
            EffectiveFromDate = effectiveFromDate
        };
    }

    private static void EnsurePositiveRatio(decimal value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Yield threshold must be greater than zero.");
        }
    }

    private static void EnsureRatio(decimal value, string parameterName)
    {
        if (value is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Ratio must be between 0 and 1.");
        }
    }

    private static void EnsureNonNegative(decimal value, string parameterName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Amount cannot be negative.");
        }
    }
}
