using Portwise.Application.Portfolio.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Portwise.Application.Portfolio;

public static class PortfolioModuleServiceCollectionExtensions
{
    public static IServiceCollection AddPortwisePortfolioModule(
        this IServiceCollection services)
    {
        services.AddScoped<IBudgetAppService, BudgetAppService>();
        services.AddScoped<IPortfolioTradeAppService, PortfolioTradeAppService>();
        return services;
    }
}
