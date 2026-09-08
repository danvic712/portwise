using Portwise.Application.Recommendations.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Portwise.Application.Recommendations;

public static class RecommendationsModuleServiceCollectionExtensions
{
    public static IServiceCollection AddPortwiseRecommendationsModule(
        this IServiceCollection services)
    {
        services.AddScoped<IStockModelParameterAppService, StockModelParameterAppService>();
        services.AddScoped<IStockAnalysisAppService, StockAnalysisAppService>();
        services.AddScoped<IPortfolioAllocationAppService, PortfolioAllocationAppService>();
        services.AddScoped<IStockRecommendationAppService, StockRecommendationAppService>();
        services.AddScoped<IPortfolioRecommendationAppService, PortfolioRecommendationAppService>();
        services.AddScoped<IRecommendationSnapshotAppService, RecommendationSnapshotAppService>();
        return services;
    }
}
