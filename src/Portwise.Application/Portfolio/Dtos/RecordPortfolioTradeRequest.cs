namespace Portwise.Application.Portfolio.Dtos;

/// <summary>Request to record a simulated portfolio trade.</summary>
/// <param name="SecurityCode">Security code to trade.</param>
/// <param name="ExchangeCode">Exchange code for the security.</param>
/// <param name="TradeDate">Trade execution date.</param>
/// <param name="TradeDirectionCode">Trade direction code.</param>
/// <param name="ShareQuantity">Number of shares to trade.</param>
/// <param name="PricePerShare">Execution price per share.</param>
/// <param name="TransactionFeeAmount">Transaction fee amount.</param>
/// <param name="SourceRecordId">Optional source-system record identifier.</param>
public sealed record RecordPortfolioTradeRequest(
    string SecurityCode,
    string ExchangeCode,
    DateOnly TradeDate,
    string TradeDirectionCode,
    int ShareQuantity,
    decimal PricePerShare,
    decimal TransactionFeeAmount,
    string? SourceRecordId);
