using Microsoft.Extensions.DependencyInjection;
using Portwise.Application.Preferences.Contracts;

namespace Portwise.Application.Preferences;

public static class PreferencesModuleServiceCollectionExtensions
{
    public static IServiceCollection AddPortwisePreferencesModule(
        this IServiceCollection services)
    {
        services.AddScoped<IPreferencesAppService, PreferencesAppService>();
        return services;
    }
}
