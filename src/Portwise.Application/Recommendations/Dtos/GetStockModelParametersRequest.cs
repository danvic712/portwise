namespace Portwise.Application.Recommendations.Dtos;

public sealed record GetStockModelParametersRequest(
    string SecurityCode,
    string ExchangeCode);
