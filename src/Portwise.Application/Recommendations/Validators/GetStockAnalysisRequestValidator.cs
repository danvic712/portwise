using Portwise.Application.Dtos;
using FluentValidation;

namespace Portwise.Application.Validators;

public sealed class GetStockAnalysisRequestValidator
    : AbstractValidator<GetStockAnalysisRequest>
{
    public GetStockAnalysisRequestValidator()
    {
        AShareValidationRules.Add(this, x => x.SecurityCode, x => x.ExchangeCode);
    }
}
