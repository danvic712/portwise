using FluentValidation;
using Portwise.Application.Stocks.Dtos;
using Portwise.Application.Validators;

namespace Portwise.Application.Stocks.Validators;

/// <summary>
/// Validates a stock added after onboarding.
/// </summary>
public sealed class AddStockRequestValidator : AbstractValidator<AddStockRequest>
{
    /// <summary>
    /// Initializes the validation rules for a watchlist stock.
    /// </summary>
    public AddStockRequestValidator()
    {
        AShareValidationRules.Add(this, item => item.SecurityCode, item => item.ExchangeCode);

        RuleFor(item => item.HeldShares)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Held shares cannot be negative.");
    }
}
