using System.Linq.Expressions;
using FluentValidation;

namespace Portwise.Application.Validators;

internal static class AShareValidationRules
{
    private static readonly string[] SupportedExchanges = ["SSE", "SZSE", "BSE"];

    public static void Add<T>(
        AbstractValidator<T> validator,
        Expression<Func<T, string?>> securityCode,
        Expression<Func<T, string?>> exchangeCode)
    {
        validator.RuleFor(securityCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(IsValidSecurityCode)
            .WithMessage("Security code must contain exactly 6 digits.");

        validator.RuleFor(exchangeCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(IsSupportedExchange)
            .WithMessage("Exchange code must be SSE, SZSE or BSE.");
    }

    internal static bool IsValidSecurityCode(string? value)
        => value?.Trim().Length == 6
            && value.Trim().All(character => character is >= '0' and <= '9');

    internal static bool IsSupportedExchange(string? value)
        => value is not null
            && SupportedExchanges.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);
}
