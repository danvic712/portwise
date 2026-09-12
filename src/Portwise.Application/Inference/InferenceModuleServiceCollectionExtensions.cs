using Microsoft.Extensions.DependencyInjection;
using Portwise.Application.Inference.Contracts;

namespace Portwise.Application.Inference;

public static class InferenceModuleServiceCollectionExtensions
{
    public static IServiceCollection AddPortwiseInferenceModule(this IServiceCollection services)
    {
        services.AddScoped<IInferenceConfigurationAppService, InferenceConfigurationAppService>();
        return services;
    }
}
