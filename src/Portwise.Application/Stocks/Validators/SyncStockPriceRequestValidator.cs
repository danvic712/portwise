using Portwise.Application.Stocks.Dtos;
using Portwise.Application.Validators;
using FluentValidation;

namespace Portwise.Application.Stocks.Validators;

public sealed class SyncStockPriceRequestValidator
    : AbstractValidator<SyncStockPriceRequest>
{
    public SyncStockPriceRequestValidator()
    {
        AShareValidationRules.Add(this, x => x.SecurityCode, x => x.ExchangeCode);
    }
}
