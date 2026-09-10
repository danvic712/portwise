using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Portwise;

public static class OpenApiServiceCollectionExtensions
{
    public static IServiceCollection AddPortwiseOpenApiDescription(
        this IServiceCollection services)
    {
        services.AddControllers();

        // Keep this direct literal call so ASP.NET Core's OpenAPI source generator
        // can add the compile-time XML comment transformers for all project references.
        // Register it before API Versioning so the version-aware provider remains the
        // final unkeyed document provider for runtime and build-time generation.
        services.AddOpenApi();

        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = false;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            })
            .AddOpenApi(options =>
            {
                options.Document.AddDocumentTransformer(
                    (document, _, _) =>
                    {
                        document.Info.Title = "Portwise API";
                        document.Info.Description = "Personal portfolio strategy reference API.";
                        return Task.CompletedTask;
                    });
            });

        return services;
    }

    public static WebApplication MapPortwiseOpenApi(this WebApplication app)
    {
        app.MapOpenApi().WithDocumentPerVersion();
        return app;
    }
}
