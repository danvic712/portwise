using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Portwise.OpenApi;

namespace Portwise;

public static class OpenApiServiceCollectionExtensions
{
    public static IServiceCollection AddPortwiseOpenApiDescription(
        this IServiceCollection services)
    {
        services.AddControllers();
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
                options.Document.AddOperationTransformer<XmlDocumentationOperationTransformer>();
                options.Document.AddSchemaTransformer<XmlDocumentationSchemaTransformer>();
            });

        return services;
    }

    public static WebApplication MapPortwiseOpenApi(this WebApplication app)
    {
        app.MapOpenApi().WithDocumentPerVersion();
        return app;
    }
}
