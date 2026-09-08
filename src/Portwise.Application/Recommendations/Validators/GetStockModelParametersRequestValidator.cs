using Portwise.Application.Dtos;
using FluentValidation;

namespace Portwise.Application.Validators;

public sealed class GetStockModelParametersRequestValidator
    : AbstractValidator<GetStockModelParametersRequest>
{
    public GetStockModelParametersRequestValidator()
    {
        AShareValidationRules.Add(this, x => x.SecurityCode, x => x.ExchangeCode);
    }
}
