namespace Portwise.Application.Portfolio.Dtos;

/// <summary>Aggregated cash budget information for the configured portfolio.</summary>
/// <param name="PortfolioId">Identifier of the portfolio.</param>
/// <param name="PortfolioName">Display name of the portfolio.</param>
/// <param name="TotalInflowAmount">Total cash inflow recorded to date.</param>
/// <param name="TotalOutflowAmount">Total cash outflow recorded to date.</param>
/// <param name="CashBalanceAmount">Current cash balance.</param>
/// <param name="EntryCount">Number of ledger entries included in the calculation.</param>
/// <param name="ComputedAt">UTC timestamp when the summary was computed.</param>
public sealed record BudgetSummary(
    Guid PortfolioId,
    string PortfolioName,
    decimal TotalInflowAmount,
    decimal TotalOutflowAmount,
    decimal CashBalanceAmount,
    int EntryCount,
    DateTimeOffset ComputedAt);
