using Portwise.Application.Contracts;
using Portwise.Application.DividendStrategy;
using Portwise.Application.Localization;
using Portwise.Application.Portfolio;
using Portwise.Application.Setup;
using Portwise.Application.Stocks;
using Portwise.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Portwise.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddPortwiseApplication(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<SetupRequestValidator>();
        services.AddScoped<ISetupAppService, SetupAppService>();
        services.AddScoped<IStockWatchlistAppService, StockWatchlistAppService>();
        services.AddScoped<IStockModelParameterAppService, StockModelParameterAppService>();
        services.AddScoped<IStockPriceObservationAppService, StockPriceObservationAppService>();
        services.AddScoped<IStockDividendEventAppService, StockDividendEventAppService>();
        services.AddScoped<IStockFinancialSnapshotAppService, StockFinancialSnapshotAppService>();
        services.AddScoped<IStockFactSyncAppService, StockFactSyncAppService>();
        services.AddScoped<IStockAnalysisAppService, StockAnalysisAppService>();
        services.AddScoped<IPortfolioAllocationAppService, PortfolioAllocationAppService>();
        services.AddScoped<IStockRecommendationAppService, StockRecommendationAppService>();
        services.AddScoped<IBudgetAppService, BudgetAppService>();
        services.AddScoped<IPortfolioRecommendationAppService, PortfolioRecommendationAppService>();
        services.AddScoped<IRecommendationSnapshotAppService, RecommendationSnapshotAppService>();
        services.AddScoped<IPortfolioTradeAppService, PortfolioTradeAppService>();
        services.AddScoped<IStockDailyDataSyncAppService, StockDailyDataSyncAppService>();
        services.AddSingleton<IApplicationErrorCatalog, ApplicationErrorCatalog>();
        services.AddSingleton<IApplicationErrorLocalizer, ApplicationErrorLocalizer>();
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
