using Portwise.Application.Stocks.Dtos;
using Portwise.Application.Validators;
using FluentValidation;

namespace Portwise.Application.Stocks.Validators;

public sealed class SyncStockDividendsRequestValidator
    : AbstractValidator<SyncStockDividendsRequest>
{
    public SyncStockDividendsRequestValidator()
    {
        AShareValidationRules.Add(this, x => x.SecurityCode, x => x.ExchangeCode);
    }
}
