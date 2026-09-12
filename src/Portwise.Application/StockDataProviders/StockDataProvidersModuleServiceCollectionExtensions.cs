using Microsoft.Extensions.DependencyInjection;
using Portwise.Application.StockDataProviders.Contracts;

namespace Portwise.Application.StockDataProviders;

public static class StockDataProvidersModuleServiceCollectionExtensions
{
    public static IServiceCollection AddPortwiseStockDataProvidersModule(
        this IServiceCollection services)
    {
        services.AddScoped<
            IStockDataProviderConfigurationAppService,
            StockDataProviderConfigurationAppService>();
        return services;
    }
}
