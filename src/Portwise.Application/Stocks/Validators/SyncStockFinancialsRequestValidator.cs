using Portwise.Application.Stocks.Dtos;
using Portwise.Application.Validators;
using FluentValidation;

namespace Portwise.Application.Stocks.Validators;

public sealed class SyncStockFinancialsRequestValidator
    : AbstractValidator<SyncStockFinancialsRequest>
{
    public SyncStockFinancialsRequestValidator()
    {
        AShareValidationRules.Add(this, x => x.SecurityCode, x => x.ExchangeCode);
    }
}
