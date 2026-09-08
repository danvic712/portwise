using Portwise.Application.Setup.Dtos;
using Portwise.Application.Setup.Validators;
using Xunit;

namespace Portwise.Application.Tests;

public sealed class SetupRequestValidatorTests
{
    [Fact]
    public async Task ValidateAsync_rejects_core_shares_above_held_shares()
    {
        var validator = CreateValidator();
        var request = new SetupRequest(
            "长期股息组合",
            [new SetupStockRequest(
                "000001",
                "SZSE",
                new InitialHoldingInput(100, 101, 120, 10m))]);

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Stocks[0].InitialHolding.CoreShares"
            && error.ErrorMessage == "Core shares cannot be negative or exceed held shares.");
    }

    [Fact]
    public async Task ValidateAsync_rejects_invalid_stock_reference()
    {
        var validator = CreateValidator();
        var request = new SetupRequest(
            "长期股息组合",
            [new SetupStockRequest("123", "NYSE", null)]);

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Stocks[0].SecurityCode"
            && error.ErrorMessage == "Security code must contain exactly 6 digits.");
        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Stocks[0].ExchangeCode"
            && error.ErrorMessage == "Exchange code must be SSE, SZSE or BSE.");
    }

    [Fact]
    public async Task ValidateAsync_rejects_empty_portfolio_name_and_stock_list()
    {
        var validator = CreateValidator();
        var request = new SetupRequest("   ", []);

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == "PortfolioName"
            && error.ErrorMessage == "Portfolio name must contain 1 to 100 characters.");
        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Stocks"
            && error.ErrorMessage == "At least one A-share security must be configured.");
    }

    [Fact]
    public async Task ValidateAsync_rejects_negative_initial_holding_values()
    {
        var validator = CreateValidator();
        var request = new SetupRequest(
            "长期股息组合",
            [new SetupStockRequest(
                "000001",
                "SZSE",
                new InitialHoldingInput(-1, -2, -3, -1m))]);

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Stocks[0].InitialHolding.HeldShares"
            && error.ErrorMessage == "Held shares cannot be negative.");
        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Stocks[0].InitialHolding.CoreShares"
            && error.ErrorMessage == "Core shares cannot be negative or exceed held shares.");
        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Stocks[0].InitialHolding.TargetShares"
            && error.ErrorMessage == "Target shares cannot be negative.");
        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Stocks[0].InitialHolding.AverageCostPerShare"
            && error.ErrorMessage == "Average cost per share cannot be negative.");
    }

    [Fact]
    public async Task ValidateAsync_rejects_duplicate_normalized_stock_references()
    {
        var validator = CreateValidator();
        var request = new SetupRequest(
            "长期股息组合",
            [
                new SetupStockRequest("000001", "szse", null),
                new SetupStockRequest(" 000001 ", "SZSE", null)
            ]);

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Stocks"
            && error.ErrorMessage == "The same security cannot be configured more than once.");
    }

    [Fact]
    public async Task ValidateAsync_rejects_null_stock_items()
    {
        var validator = CreateValidator();
        var request = new SetupRequest(
            "长期股息组合",
            [null!]);

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Stocks[0]"
            && error.ErrorMessage == "Stock configuration is required.");
    }

    [Fact]
    public async Task ValidateAsync_rejects_null_stock_collection()
    {
        var validator = CreateValidator();
        var request = new SetupRequest("长期股息组合", null!);

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == "Stocks"
            && error.ErrorMessage == "At least one A-share security must be configured.");
    }

    private static SetupRequestValidator CreateValidator()
        => new(new SetupStockRequestValidator(new InitialHoldingInputValidator()));
}
