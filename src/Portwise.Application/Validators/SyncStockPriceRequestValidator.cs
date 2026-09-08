using Portwise.Application.Dtos;
using FluentValidation;

namespace Portwise.Application.Validators;

public sealed class SyncStockPriceRequestValidator
    : AbstractValidator<SyncStockPriceRequest>
{
    public SyncStockPriceRequestValidator()
    {
        AShareValidationRules.Add(this, x => x.SecurityCode, x => x.ExchangeCode);
    }
}
