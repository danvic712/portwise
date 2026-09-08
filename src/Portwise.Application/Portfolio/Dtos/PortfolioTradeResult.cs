namespace Portwise.Application.Portfolio.Dtos;

/// <summary>Result returned after a portfolio trade is recorded.</summary>
/// <param name="PortfolioTradeId">Identifier of the created trade.</param>
/// <param name="PortfolioId">Identifier of the portfolio.</param>
/// <param name="SecurityCode">Traded security code.</param>
/// <param name="ExchangeCode">Traded exchange code.</param>
/// <param name="TradeDate">Trade execution date.</param>
/// <param name="TradeDirectionCode">Normalized trade direction code.</param>
/// <param name="ShareQuantity">Number of shares traded.</param>
/// <param name="PricePerShare">Execution price per share.</param>
/// <param name="TransactionFeeAmount">Transaction fee charged.</param>
/// <param name="HeldShares">Shares held after the trade.</param>
/// <param name="CoreShares">Core shares after the trade.</param>
/// <param name="TargetShares">Target shares after the trade.</param>
/// <param name="AverageCostPerShare">Average cost per share after the trade.</param>
/// <param name="TradePrincipalAmount">Trade principal amount.</param>
public sealed record PortfolioTradeResult(
    Guid PortfolioTradeId,
    Guid PortfolioId,
    string SecurityCode,
    string ExchangeCode,
    DateOnly TradeDate,
    string TradeDirectionCode,
    int ShareQuantity,
    decimal PricePerShare,
    decimal TransactionFeeAmount,
    int HeldShares,
    int CoreShares,
    int TargetShares,
    decimal AverageCostPerShare,
    decimal TradePrincipalAmount);
