using Portwise.Application.Recommendations.Dtos;
using Portwise.Application.Validators;
using FluentValidation;

namespace Portwise.Application.Recommendations.Validators;

public sealed class GetStockAnalysisRequestValidator
    : AbstractValidator<GetStockAnalysisRequest>
{
    public GetStockAnalysisRequestValidator()
    {
        AShareValidationRules.Add(this, x => x.SecurityCode, x => x.ExchangeCode);
    }
}
