using Portwise.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Portwise.Application.Setup;

public static class SetupModuleServiceCollectionExtensions
{
    public static IServiceCollection AddPortwiseSetupModule(
        this IServiceCollection services)
    {
        services.AddScoped<ISetupAppService, SetupAppService>();
        return services;
    }
}
