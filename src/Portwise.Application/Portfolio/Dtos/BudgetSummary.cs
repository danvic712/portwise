namespace Portwise.Application.Portfolio.Dtos;

public sealed record BudgetSummary(
    Guid PortfolioId,
    string PortfolioName,
    decimal TotalInflowAmount,
    decimal TotalOutflowAmount,
    decimal CashBalanceAmount,
    int EntryCount,
    DateTimeOffset ComputedAt);
