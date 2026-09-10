using System.Text.Json;
using Portwise.Application.Contracts;
using Portwise.Application.Diagnostics;
using Portwise.Application.Stocks.Contracts;
using Portwise.Application.Stocks.Dtos;
using Portwise.Domain.Codes;
using Portwise.Domain.Securities;
using Portwise.Infrastructure.Contracts;
using Portwise.Infrastructure.Exceptions;
using Microsoft.Extensions.Options;

namespace Portwise.Infrastructure.FtShare;

public sealed class FtShareStockDataProvider(
    IFtShareMcpToolInvoker toolInvoker,
    IOptions<FtShareOptions> options,
    IDiagnosticContext diagnosticContext,
    TimeProvider timeProvider) : IStockDataProvider
{
    public async Task<StockData?> GetAsync(
        AShareReference reference,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(reference);

        var currentOptions = options.Value;
        var payload = await InvokeStockToolAsync(
            reference,
            currentOptions,
            currentOptions.StockProfileToolName,
            cancellationToken,
            "profile",
            "FTShare MCP profile request timed out.");
        var profile = FtSharePayloadReader.ReadOne(
            payload,
            FtShareJsonContext.Default.FtShareProfileWire,
            FtSharePayloadNormalizer.ProfileLeafPropertyNames,
            FtSharePayloadNormalizer.ProfileEnvelopeNames);
        return FtSharePayloadNormalizer.NormalizeProfile(reference, profile);
    }

    public async Task<StockMarketData?> GetMarketDataAsync(
        AShareReference reference,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(reference);

        var currentOptions = options.Value;
        var payload = await InvokeStockToolAsync(
            reference,
            currentOptions,
            currentOptions.StockMarketDataToolName,
            cancellationToken,
            "market",
            "FTShare MCP market-data request timed out.");
        var market = FtSharePayloadReader.ReadOne(
            payload,
            FtShareJsonContext.Default.FtShareMarketWire,
            FtSharePayloadNormalizer.MarketLeafPropertyNames,
            FtSharePayloadNormalizer.MarketEnvelopeNames);
        return FtSharePayloadNormalizer.NormalizeMarket(reference, market);
    }

    public async Task<IReadOnlyList<StockDividendData>?> GetDividendEventsAsync(
        AShareReference reference,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(reference);

        var currentOptions = options.Value;
        var payload = await InvokeStockToolAsync(
            reference,
            currentOptions,
            currentOptions.StockDividendEventsToolName,
            cancellationToken,
            "dividend",
            "FTShare MCP dividend request timed out.");
        var dividends = FtSharePayloadReader.ReadMany(
            payload,
            FtShareJsonContext.Default.FtShareDividendWire,
            FtSharePayloadNormalizer.DividendLeafPropertyNames,
            FtSharePayloadNormalizer.DividendEnvelopeNames);
        return FtSharePayloadNormalizer.NormalizeDividends(reference, dividends, timeProvider);
    }

    public async Task<IReadOnlyList<StockFinancialData>?> GetFinancialSnapshotsAsync(
        AShareReference reference,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(reference);

        var currentOptions = options.Value;
        var payload = await InvokeStockToolAsync(
            reference,
            currentOptions,
            currentOptions.StockFinancialSnapshotsToolName,
            cancellationToken,
            "financial",
            "FTShare MCP financial-data request timed out.");
        var financials = FtSharePayloadReader.ReadMany(
            payload,
            FtShareJsonContext.Default.FtShareFinancialWire,
            FtSharePayloadNormalizer.FinancialLeafPropertyNames,
            FtSharePayloadNormalizer.FinancialEnvelopeNames);
        return FtSharePayloadNormalizer.NormalizeFinancials(reference, financials, timeProvider);
    }

    private async Task<JsonElement?> InvokeStockToolAsync(
        AShareReference reference,
        FtShareOptions currentOptions,
        string toolName,
        CancellationToken cancellationToken,
        string dataKind,
        string timeoutMessage)
    {
        var arguments = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            [currentOptions.SecurityCodeArgumentName] = reference.SecurityCode,
            [currentOptions.ExchangeCodeArgumentName] = reference.ExchangeCode
        };

        return await InvokeToolAsync(
            toolName,
            arguments,
            cancellationToken,
            reference,
            dataKind,
            timeoutMessage);
    }

    private async Task<JsonElement?> InvokeToolAsync(
        string toolName,
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken cancellationToken,
        AShareReference reference,
        string dataKind,
        string timeoutMessage)
    {
        using var diagnosticScope = diagnosticContext.BeginScope(new DiagnosticScope(
            "ftshare_mcp",
            SecurityCode: reference.SecurityCode,
            ExchangeCode: reference.ExchangeCode,
            DataKind: dataKind));

        try
        {
            return await toolInvoker.InvokeAsync(
                toolName,
                arguments,
                cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new FtShareProviderException(new TimeoutException(timeoutMessage));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new FtShareProviderException(exception);
        }
    }
}
