namespace Portwise.Application.Recommendations.Dtos;

public sealed record GetStockAnalysisRequest(
    string SecurityCode,
    string ExchangeCode);
