using Portwise.Application.Setup.Dtos;
using Portwise.Domain.Securities;
using FluentValidation;

namespace Portwise.Application.Setup.Validators;

public sealed class SetupRequestValidator : AbstractValidator<SetupRequest>
{
    public SetupRequestValidator(IValidator<SetupStockRequest> stockValidator)
    {
        RuleFor(x => x.PortfolioName)
            .Cascade(CascadeMode.Stop)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Portfolio name must contain 1 to 100 characters.")
            .Must(value => value.Trim().Length <= 100)
            .WithMessage("Portfolio name must contain 1 to 100 characters.");

        RuleFor(x => x.Stocks)
            .Must(stocks => stocks is { Count: > 0 })
            .WithMessage("At least one A-share security must be configured.");

        RuleFor(x => x.Stocks)
            .Must(HaveUniqueStockReferences)
            .WithMessage("The same security cannot be configured more than once.");

        RuleForEach(x => x.Stocks)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Stock configuration is required.")
            .SetValidator(stockValidator);
    }

    private static bool HaveUniqueStockReferences(IReadOnlyList<SetupStockRequest>? stocks)
    {
        if (stocks is null)
        {
            return true;
        }

        var references = new HashSet<AShareReference>();
        foreach (var stock in stocks)
        {
            if (stock is null)
            {
                continue;
            }

            try
            {
                if (!references.Add(AShareReference.Create(stock.SecurityCode, stock.ExchangeCode)))
                {
                    return false;
                }
            }
            catch (ArgumentException)
            {
                // The child validator reports the malformed stock reference.
            }
        }

        return true;
    }
}
