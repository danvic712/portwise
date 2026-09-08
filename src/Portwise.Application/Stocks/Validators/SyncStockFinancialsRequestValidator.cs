using Portwise.Application.Dtos;
using FluentValidation;

namespace Portwise.Application.Validators;

public sealed class SyncStockFinancialsRequestValidator
    : AbstractValidator<SyncStockFinancialsRequest>
{
    public SyncStockFinancialsRequestValidator()
    {
        AShareValidationRules.Add(this, x => x.SecurityCode, x => x.ExchangeCode);
    }
}
