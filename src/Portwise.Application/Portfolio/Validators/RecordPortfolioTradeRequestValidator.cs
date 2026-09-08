using Portwise.Application.Portfolio.Dtos;
using Portwise.Application.Validators;
using Portwise.Domain.Portfolio;
using FluentValidation;

namespace Portwise.Application.Portfolio.Validators;

public sealed class RecordPortfolioTradeRequestValidator
    : AbstractValidator<RecordPortfolioTradeRequest>
{
    public RecordPortfolioTradeRequestValidator()
    {
        AShareValidationRules.Add(this, x => x.SecurityCode, x => x.ExchangeCode);

        RuleFor(x => x.TradeDate)
            .NotEqual(DateOnly.MinValue)
            .WithMessage("Trade date is required.");
        RuleFor(x => x.TradeDirectionCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(TradeDirectionCodes.IsSupported)
            .WithMessage("Trade direction must be buy or sell.");
        RuleFor(x => x.ShareQuantity)
            .GreaterThan(0)
            .WithMessage("Trade share quantity must be greater than zero.");
        RuleFor(x => x.PricePerShare)
            .GreaterThan(0)
            .WithMessage("Trade price must be greater than zero.");
        RuleFor(x => x.TransactionFeeAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Transaction fee cannot be negative.");
        RuleFor(x => x.SourceRecordId)
            .MaximumLength(200)
            .WithMessage("Source record identifier cannot exceed 200 characters.");
    }
}
