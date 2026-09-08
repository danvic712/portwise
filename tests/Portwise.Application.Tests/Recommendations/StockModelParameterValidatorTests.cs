using Portwise.Application.Recommendations.Dtos;
using Portwise.Application.Recommendations.Validators;
using Xunit;

namespace Portwise.Application.Tests;

public sealed class StockModelParameterValidatorTests
{
    [Fact]
    public async Task ValidateAsync_rejects_yield_thresholds_that_are_not_descending()
    {
        var validator = new SaveStockModelParametersRequestValidator();
        var result = await validator.ValidateAsync(CreateRequest(
            accumulationYieldThreshold: 0.06m,
            partialTrimYieldThreshold: 0.07m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == "PartialTrimYieldThreshold"
            && error.ErrorMessage == "Accumulation yield threshold must exceed the partial-trim threshold.");
    }

    [Fact]
    public async Task ValidateAsync_rejects_ratios_above_one()
    {
        var validator = new SaveStockModelParametersRequestValidator();
        var result = await validator.ValidateAsync(CreateRequest(strongBuyBudgetRatio: 1.01m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == "StrongBuyBudgetRatio"
            && error.ErrorMessage == "Strong-buy budget ratio must be between 0 and 1.");
    }

    private static SaveStockModelParametersRequest CreateRequest(
        decimal accumulationYieldThreshold = 0.06m,
        decimal partialTrimYieldThreshold = 0.04m,
        decimal strongBuyBudgetRatio = 0.5m)
        => new(
            "000001",
            "SZSE",
            "v1",
            0.08m,
            accumulationYieldThreshold,
            partialTrimYieldThreshold,
            0.03m,
            strongBuyBudgetRatio,
            0.25m,
            0.25m,
            0.5m,
            0.2m,
            0.4m,
            0.1m,
            1000m,
            5000m,
            0.001m,
            5m,
            100,
            new DateOnly(2026, 9, 2));
}
