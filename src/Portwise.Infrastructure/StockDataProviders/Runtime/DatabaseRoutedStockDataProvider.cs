using Portwise.Application.StockDataProviders.Contracts;
using Portwise.Application.Stocks.Contracts;
using Portwise.Application.Stocks.Dtos;
using Portwise.Domain.Enums;
using Portwise.Domain.Securities;

namespace Portwise.Infrastructure.StockDataProviders.Runtime;

internal sealed class DatabaseRoutedStockDataProvider(
    IStockDataProviderRuntimeResolver runtimeResolver) : IStockDataProvider
{
    public async Task<StockData?> GetAsync(
        AShareReference reference,
        CancellationToken cancellationToken)
    {
        var provider = await runtimeResolver.ResolveAsync(
            StockDataCapability.Profile,
            cancellationToken);
        return provider is null ? null : await provider.GetAsync(reference, cancellationToken);
    }

    public async Task<StockMarketData?> GetMarketDataAsync(
        AShareReference reference,
        CancellationToken cancellationToken)
    {
        var provider = await runtimeResolver.ResolveAsync(
            StockDataCapability.Market,
            cancellationToken);
        return provider is null
            ? null
            : await provider.GetMarketDataAsync(reference, cancellationToken);
    }

    public async Task<IReadOnlyList<StockDividendData>?> GetDividendEventsAsync(
        AShareReference reference,
        CancellationToken cancellationToken)
    {
        var provider = await runtimeResolver.ResolveAsync(
            StockDataCapability.Dividend,
            cancellationToken);
        return provider is null
            ? null
            : await provider.GetDividendEventsAsync(reference, cancellationToken);
    }

    public async Task<IReadOnlyList<StockFinancialData>?> GetFinancialSnapshotsAsync(
        AShareReference reference,
        CancellationToken cancellationToken)
    {
        var provider = await runtimeResolver.ResolveAsync(
            StockDataCapability.Financial,
            cancellationToken);
        return provider is null
            ? null
            : await provider.GetFinancialSnapshotsAsync(reference, cancellationToken);
    }
}
