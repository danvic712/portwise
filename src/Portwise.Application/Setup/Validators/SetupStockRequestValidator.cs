using Portwise.Application.Dtos;
using FluentValidation;

namespace Portwise.Application.Validators;

public sealed class SetupStockRequestValidator : AbstractValidator<SetupStockRequest>
{
    public SetupStockRequestValidator(IValidator<InitialHoldingInput> initialHoldingValidator)
    {
        AShareValidationRules.Add(this, x => x.SecurityCode, x => x.ExchangeCode);

        When(x => x.InitialHolding is not null, () =>
        {
            RuleFor(x => x.InitialHolding!)
                .SetValidator(initialHoldingValidator);
        });
    }
}
