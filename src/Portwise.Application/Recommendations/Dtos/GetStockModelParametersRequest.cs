namespace Portwise.Application.Recommendations.Dtos;

/// <summary>Identifies the stock whose active model parameters are requested.</summary>
/// <param name="SecurityCode">Six-digit security code.</param>
/// <param name="ExchangeCode">Exchange code.</param>
public sealed record GetStockModelParametersRequest(
    string SecurityCode,
    string ExchangeCode);
