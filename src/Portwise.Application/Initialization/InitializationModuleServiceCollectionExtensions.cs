using Microsoft.Extensions.DependencyInjection;
using Portwise.Application.Initialization.Contracts;

namespace Portwise.Application.Initialization;

public static class InitializationModuleServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Initialization application module.
    /// </summary>
    /// <param name="services">The application service collection.</param>
    /// <returns>The same service collection for fluent registration.</returns>
    public static IServiceCollection AddPortwiseInitializationModule(
        this IServiceCollection services)
    {
        services.AddScoped<IInitializationAppService, InitializationAppService>();
        return services;
    }
}
