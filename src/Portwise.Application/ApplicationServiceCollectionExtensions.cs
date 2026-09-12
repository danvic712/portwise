using Portwise.Application.Contracts;
using Portwise.Application.Localization;
using Portwise.Application.Portfolio;
using Portwise.Application.Preferences;
using Portwise.Application.Inference;
using Portwise.Application.Initialization;
using Portwise.Application.Recommendations;
using Portwise.Application.Stocks;
using Portwise.Application.StockDataProviders;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Portwise.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddPortwiseApplication(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<InitializationAppService>();
        services.AddPortwiseStocksModule();
        services.AddPortwiseStockDataProvidersModule();
        services.AddPortwisePortfolioModule();
        services.AddPortwisePreferencesModule();
        services.AddPortwiseInferenceModule();
        services.AddPortwiseInitializationModule();
        services.AddPortwiseRecommendationsModule();
        services.AddSingleton<IApplicationErrorCatalog, ApplicationErrorCatalog>();
        services.AddSingleton<IApplicationErrorLocalizer, ApplicationErrorLocalizer>();
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
