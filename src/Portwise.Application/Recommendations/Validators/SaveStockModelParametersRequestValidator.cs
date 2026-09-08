using System.Linq.Expressions;
using Portwise.Application.Recommendations.Dtos;
using Portwise.Application.Validators;
using FluentValidation;

namespace Portwise.Application.Recommendations.Validators;

public sealed class SaveStockModelParametersRequestValidator
    : AbstractValidator<SaveStockModelParametersRequest>
{
    public SaveStockModelParametersRequestValidator()
    {
        AShareValidationRules.Add(this, x => x.SecurityCode, x => x.ExchangeCode);

        RuleFor(x => x.ModelVersion)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Model version is required.")
            .MaximumLength(32)
            .WithMessage("Model version cannot exceed 32 characters.");

        RuleFor(x => x.StrongBuyYieldThreshold)
            .GreaterThan(0)
            .WithMessage("Yield threshold must be greater than zero.");
        RuleFor(x => x.AccumulationYieldThreshold)
            .GreaterThan(0)
            .WithMessage("Yield threshold must be greater than zero.");
        RuleFor(x => x.PartialTrimYieldThreshold)
            .GreaterThan(0)
            .WithMessage("Yield threshold must be greater than zero.");
        RuleFor(x => x.AggressiveTrimYieldThreshold)
            .GreaterThan(0)
            .WithMessage("Yield threshold must be greater than zero.");

        RuleFor(x => x.AccumulationYieldThreshold)
            .Must((request, value) => request.StrongBuyYieldThreshold > value)
            .WithMessage("Strong-buy yield threshold must exceed the accumulation threshold.");
        RuleFor(x => x.PartialTrimYieldThreshold)
            .Must((request, value) => request.AccumulationYieldThreshold > value)
            .WithMessage("Accumulation yield threshold must exceed the partial-trim threshold.");
        RuleFor(x => x.AggressiveTrimYieldThreshold)
            .Must((request, value) => request.PartialTrimYieldThreshold > value)
            .WithMessage("Partial-trim yield threshold must exceed the aggressive-trim threshold.");

        AddRatioRule(x => x.StrongBuyBudgetRatio, "Strong-buy budget ratio");
        AddRatioRule(x => x.AccumulateBudgetRatio, "Accumulation budget ratio");
        AddRatioRule(x => x.PartialTrimRatio, "Partial-trim ratio");
        AddRatioRule(x => x.AggressiveTrimRatio, "Aggressive-trim ratio");
        AddRatioRule(x => x.MaxSecurityWeight, "Maximum security weight");
        AddRatioRule(x => x.MaxSectorWeight, "Maximum sector weight");
        AddRatioRule(x => x.CashReserveRatio, "Cash reserve ratio");
        AddRatioRule(x => x.TransactionFeeRatio, "Transaction fee ratio");

        RuleFor(x => x.MaxSingleTradeAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Maximum single-trade amount cannot be negative.");
        RuleFor(x => x.MaxPeriodBudgetAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Maximum period budget cannot be negative.");
        RuleFor(x => x.MinimumTransactionFeeAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Minimum transaction fee cannot be negative.");
        RuleFor(x => x.TradingLotSize)
            .GreaterThan(0)
            .WithMessage("Trading lot size must be greater than zero.");
        RuleFor(x => x.EffectiveFromDate)
            .NotEqual(DateOnly.MinValue)
            .WithMessage("Effective-from date is required.");
    }

    private void AddRatioRule(
        Expression<Func<SaveStockModelParametersRequest, decimal>> selector,
        string label)
    {
        RuleFor(selector)
            .InclusiveBetween(0, 1)
            .WithMessage($"{label} must be between 0 and 1.");
    }
}
