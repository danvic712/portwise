namespace Portwise.Application.Recommendations.Dtos;

/// <summary>Identifies the stock for which analysis is requested.</summary>
/// <param name="SecurityCode">Six-digit security code.</param>
/// <param name="ExchangeCode">Exchange code.</param>
public sealed record GetStockAnalysisRequest(
    string SecurityCode,
    string ExchangeCode);
