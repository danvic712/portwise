using Portwise.Application.Dtos;
using FluentValidation;

namespace Portwise.Application.Validators;

public sealed class SyncStockDividendsRequestValidator
    : AbstractValidator<SyncStockDividendsRequest>
{
    public SyncStockDividendsRequestValidator()
    {
        AShareValidationRules.Add(this, x => x.SecurityCode, x => x.ExchangeCode);
    }
}
