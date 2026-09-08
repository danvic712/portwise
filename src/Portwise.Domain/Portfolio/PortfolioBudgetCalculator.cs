using Portwise.Domain.Models;

namespace Portwise.Domain.Portfolio;

public static class PortfolioBudgetCalculator
{
    public static decimal CalculateAvailableBudget(
        decimal cashBalanceAmount,
        decimal portfolioMarketValue,
        decimal cashReserveRatio)
    {
        if (portfolioMarketValue < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(portfolioMarketValue),
                portfolioMarketValue,
                "Portfolio market value cannot be negative.");
        }

        if (cashReserveRatio is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cashReserveRatio),
                cashReserveRatio,
                "Cash reserve ratio must be between 0 and 1.");
        }

        return Math.Max(
            cashBalanceAmount - portfolioMarketValue * cashReserveRatio,
            0m);
    }

    public static decimal CalculateCashBalance(
        IEnumerable<CashLedgerEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        return entries
            .Sum(entry => entry.CashDirectionCode == CashLedgerCodes.Inflow
                ? entry.CashAmount
                : -entry.CashAmount);
    }

    public static decimal CalculateCurrentCashReserveRatio(
        IEnumerable<ModelParameterSet> parameters,
        DateOnly currentDate)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        if (currentDate == DateOnly.MinValue)
        {
            throw new ArgumentException("Parameter date is required.", nameof(currentDate));
        }

        return parameters
            .Where(parameter => parameter.EffectiveFromDate <= currentDate)
            .GroupBy(parameter => parameter.SecurityId)
            .Select(group => group
                .OrderByDescending(parameter => parameter.EffectiveFromDate)
                .First()
                .CashReserveRatio)
            .DefaultIfEmpty(0m)
            .Max();
    }

    public static bool HasCompleteMarketValue(
        IEnumerable<PortfolioPosition> positions,
        IReadOnlySet<Guid> pricedSecurityIds)
    {
        ArgumentNullException.ThrowIfNull(positions);
        ArgumentNullException.ThrowIfNull(pricedSecurityIds);

        return positions
            .Where(position => position.HeldShares > 0)
            .All(position => pricedSecurityIds.Contains(position.SecurityId));
    }
}
