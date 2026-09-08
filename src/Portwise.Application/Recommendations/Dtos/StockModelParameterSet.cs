namespace Portwise.Application.Recommendations.Dtos;

/// <summary>Effective model parameters for one configured stock.</summary>
/// <param name="ModelParameterSetId">Identifier of the parameter set.</param>
/// <param name="SecurityCode">Security code.</param>
/// <param name="ExchangeCode">Exchange code.</param>
/// <param name="ModelVersion">Model version label.</param>
/// <param name="StrongBuyYieldThreshold">Yield threshold for strong-buy recommendations.</param>
/// <param name="AccumulationYieldThreshold">Yield threshold for accumulation recommendations.</param>
/// <param name="PartialTrimYieldThreshold">Yield threshold for partial-trim recommendations.</param>
/// <param name="AggressiveTrimYieldThreshold">Yield threshold for aggressive-trim recommendations.</param>
/// <param name="StrongBuyBudgetRatio">Budget ratio for strong-buy recommendations.</param>
/// <param name="AccumulateBudgetRatio">Budget ratio for accumulation recommendations.</param>
/// <param name="PartialTrimRatio">Position ratio for partial trims.</param>
/// <param name="AggressiveTrimRatio">Position ratio for aggressive trims.</param>
/// <param name="MaxSecurityWeight">Maximum security portfolio weight.</param>
/// <param name="MaxSectorWeight">Maximum sector portfolio weight.</param>
/// <param name="CashReserveRatio">Portfolio cash reserve ratio.</param>
/// <param name="MaxSingleTradeAmount">Maximum amount for one trade.</param>
/// <param name="MaxPeriodBudgetAmount">Maximum budget for one period.</param>
/// <param name="TransactionFeeRatio">Transaction fee ratio.</param>
/// <param name="MinimumTransactionFeeAmount">Minimum transaction fee amount.</param>
/// <param name="TradingLotSize">Trading lot size in shares.</param>
/// <param name="EffectiveFromDate">Date from which the parameters apply.</param>
public sealed record StockModelParameterSet(
    Guid ModelParameterSetId,
    string SecurityCode,
    string ExchangeCode,
    string ModelVersion,
    decimal StrongBuyYieldThreshold,
    decimal AccumulationYieldThreshold,
    decimal PartialTrimYieldThreshold,
    decimal AggressiveTrimYieldThreshold,
    decimal StrongBuyBudgetRatio,
    decimal AccumulateBudgetRatio,
    decimal PartialTrimRatio,
    decimal AggressiveTrimRatio,
    decimal MaxSecurityWeight,
    decimal MaxSectorWeight,
    decimal CashReserveRatio,
    decimal MaxSingleTradeAmount,
    decimal MaxPeriodBudgetAmount,
    decimal TransactionFeeRatio,
    decimal MinimumTransactionFeeAmount,
    int TradingLotSize,
    DateOnly EffectiveFromDate);
