using Portwise.Application.Setup.Dtos;
using FluentValidation;

namespace Portwise.Application.Setup.Validators;

public sealed class InitialHoldingInputValidator : AbstractValidator<InitialHoldingInput>
{
    public InitialHoldingInputValidator()
    {
        RuleFor(x => x.HeldShares)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Held shares cannot be negative.");

        RuleFor(x => x.CoreShares)
            .Must((input, coreShares) => coreShares >= 0 && coreShares <= input.HeldShares)
            .WithMessage("Core shares cannot be negative or exceed held shares.");

        RuleFor(x => x.TargetShares)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Target shares cannot be negative.");

        RuleFor(x => x.AverageCostPerShare)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Average cost per share cannot be negative.");
    }
}
