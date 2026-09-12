using FluentValidation;

using Portwise.Application.Initialization.Dtos;
using Portwise.Application.Validators;

namespace Portwise.Application.Initialization.Validators;

/// <summary>
/// Validates one stock entered during onboarding.
/// </summary>
public sealed class InitialStockRequestValidator
    : AbstractValidator<InitialStockRequest>
{
    /// <summary>
    /// Initializes the initial stock validation rules.
    /// </summary>
    public InitialStockRequestValidator()
    {
        RuleFor(item => item.SecurityCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(AShareValidationRules.IsValidSecurityCode)
            .WithMessage("Security code must contain exactly 6 digits.");

        RuleFor(item => item.ExchangeCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(AShareValidationRules.IsSupportedExchange)
            .WithMessage("Exchange code must be SSE, SZSE or BSE.");

        RuleFor(item => item.HeldShares)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Held shares cannot be negative.");
    }
}
