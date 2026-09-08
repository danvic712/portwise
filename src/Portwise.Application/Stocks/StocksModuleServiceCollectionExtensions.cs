using Portwise.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Portwise.Application.Stocks;

public static class StocksModuleServiceCollectionExtensions
{
    public static IServiceCollection AddPortwiseStocksModule(
        this IServiceCollection services)
    {
        services.AddScoped<IStockWatchlistAppService, StockWatchlistAppService>();
        services.AddScoped<IStockPriceObservationAppService, StockPriceObservationAppService>();
        services.AddScoped<IStockDividendEventAppService, StockDividendEventAppService>();
        services.AddScoped<IStockFinancialSnapshotAppService, StockFinancialSnapshotAppService>();
        services.AddScoped<IStockFactSyncAppService, StockFactSyncAppService>();
        services.AddScoped<IStockDailyDataSyncAppService, StockDailyDataSyncAppService>();
        return services;
    }
}
