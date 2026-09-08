using Portwise.Domain.Models;
using Portwise.Domain.Recommendations;
using Xunit;

namespace Portwise.Domain.Tests;

public sealed class RecommendationModuleTests
{
    [Fact]
    public void CalculateStock_composes_dividend_reliability_and_price_confirmation()
    {
        var securityId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var parameters = CreateParameters(portfolioId, securityId);
        var observations = new[]
        {
            CreateObservation(securityId, new DateOnly(2026, 9, 2), 4m, "price-2"),
            CreateObservation(securityId, new DateOnly(2026, 9, 1), 4m, "price-1")
        };
        var dividends = Enumerable.Range(2021, 5)
            .Select(year => CreateDividend(securityId, new DateOnly(year, 6, 1), $"dividend-{year}"))
            .Append(CreateDividend(securityId, new DateOnly(2025, 10, 1), "dividend-2025-ttm"))
            .Append(CreateDividend(securityId, new DateOnly(2026, 6, 1), "dividend-2026"))
            .ToArray();
        var financialSnapshot = FinancialSnapshot.Create(
            securityId,
            new DateOnly(2025, 12, 31),
            new DateTimeOffset(2026, 1, 2, 8, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero),
            0.80m,
            0.45m,
            0.40m,
            0.90m,
            0.12m,
            "FTShare",
            "financial-2025",
            "valid");

        var result = RecommendationModule.CalculateStock(
            new StockRecommendationInput(
                securityId,
                parameters,
                observations,
                dividends,
                [financialSnapshot],
                new PortfolioPosition
                {
                    PortfolioId = portfolioId,
                    SecurityId = securityId,
                    HeldShares = 100,
                    CoreShares = 60,
                    TargetShares = 200,
                    AverageCostPerShare = 3.50m
                },
                new DateOnly(2026, 9, 2),
                new DateTimeOffset(2026, 9, 2, 12, 0, 0, TimeSpan.Zero)));

        Assert.Equal(securityId, result.SecurityId);
        Assert.Equal("available", result.ModelStatusCode);
        Assert.Equal("passed", result.DividendReliabilityCode);
        Assert.Equal(0.40m, result.ModelDividendPerShare);
        Assert.Equal(0.10m, result.DividendYield);
        Assert.Equal("strong_buy", result.PriceZoneCode);
        Assert.True(result.PriceZoneConfirmed);
        Assert.Equal("strong_buy", result.RecommendationCode);
        Assert.Equal(40, result.SatelliteShares);
    }

    [Fact]
    public void CalculateStock_returns_unavailable_when_ttm_dividend_is_missing()
    {
        var securityId = Guid.NewGuid();

        var result = RecommendationModule.CalculateStock(
            new StockRecommendationInput(
                securityId,
                CreateParameters(Guid.NewGuid(), securityId),
                [CreateObservation(securityId, new DateOnly(2026, 9, 2), 4m, "price-1")],
                [],
                [],
                null,
                new DateOnly(2026, 9, 2),
                new DateTimeOffset(2026, 9, 2, 12, 0, 0, TimeSpan.Zero)));

        Assert.Equal("unavailable", result.ModelStatusCode);
        Assert.Equal("unavailable", result.DividendReliabilityCode);
        Assert.Null(result.ModelDividendPerShare);
        Assert.Equal("no_action", result.RecommendationCode);
    }

    [Fact]
    public void AllocatePortfolio_composes_budget_and_trade_quantity_rules()
    {
        var securityId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var parameters = CreateParameters(portfolioId, securityId);

        var result = RecommendationModule.AllocatePortfolio(
            new PortfolioRecommendationInput(
                portfolioId,
                5000m,
                [new PortfolioRecommendationStockInput(
                    securityId,
                    "banking",
                    "available",
                    "passed",
                    4m,
                    "strong_buy",
                    0,
                    0,
                    500,
                    parameters.Id,
                    "股息可靠性检查通过。")],
                [parameters],
                new DateOnly(2026, 9, 2),
                new DateTimeOffset(2026, 9, 2, 12, 0, 0, TimeSpan.Zero)));

        var recommendation = Assert.Single(result.Stocks);
        Assert.Equal(portfolioId, result.PortfolioId);
        Assert.Equal(5000m, result.StartingAvailableBudgetAmount);
        Assert.Equal(500, recommendation.SuggestedBuyShares);
        Assert.Equal(2000m, recommendation.SuggestedTradeAmount);
        Assert.Equal(5m, recommendation.EstimatedTransactionFeeAmount);
        Assert.Equal(2995m, result.RemainingAvailableBudgetAmount);
    }

    private static PriceObservation CreateObservation(
        Guid securityId,
        DateOnly tradingDate,
        decimal closePrice,
        string sourceRecordId)
        => PriceObservation.Create(
            securityId,
            tradingDate,
            closePrice,
            tradingDate.ToDateTime(new TimeOnly(7, 0), DateTimeKind.Utc),
            "FTShare",
            sourceRecordId,
            "valid");

    private static DividendEvent CreateDividend(
        Guid securityId,
        DateOnly exDividendDate,
        string sourceRecordId)
        => DividendEvent.Create(
            securityId,
            0.20m,
            "regular_cash",
            "implemented",
            exDividendDate.AddDays(-10),
            exDividendDate,
            exDividendDate.AddDays(20),
            false,
            exDividendDate.ToDateTime(new TimeOnly(8, 0), DateTimeKind.Utc),
            exDividendDate.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc),
            "FTShare",
            sourceRecordId,
            "valid");

    private static ModelParameterSet CreateParameters(Guid portfolioId, Guid securityId)
        => ModelParameterSet.Create(
            portfolioId,
            securityId,
            "v1",
            0.08m,
            0.06m,
            0.04m,
            0.03m,
            0.5m,
            0.25m,
            0.25m,
            0.5m,
            0.5m,
            0.8m,
            0m,
            3000m,
            5000m,
            0.001m,
            5m,
            100,
            new DateOnly(2026, 1, 1));
}
