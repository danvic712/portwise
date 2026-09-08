using Asp.Versioning.ApiExplorer;
using Portwise.Application.Contracts;
using Portwise.Application.Diagnostics;
using Portwise.Infrastructure.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ApplicationDiagnosticContext = Portwise.Application.Contracts.IDiagnosticContext;

namespace Portwise;

public static class WebApplicationExtensions
{
    public static WebApplication UsePortwise(this WebApplication app)
    {
        app.UsePortwiseDiagnosticContext();
        app.UseRequestLocalization();
        app.UseExceptionHandler();
        app.UseSerilogRequestLogging();
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.MapOpenApiEndpoints();
        app.MapControllers();
        app.MapFallbackToFile("index.html");
        app.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live")
        });
        app.MapHealthChecks("/readyz", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        return app;
    }

    private static WebApplication MapOpenApiEndpoints(this WebApplication app)
    {
        app.MapPortwiseOpenApi();

        var apiVersionDescriptionProvider = app.Services
            .GetRequiredService<IApiVersionDescriptionProvider>();

        app.UseSwaggerUI(options =>
        {
            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/openapi/{description.GroupName}.json",
                    $"Portwise API {description.GroupName}");
            }

            options.RoutePrefix = "swagger";
        });

        return app;
    }

    private static IApplicationBuilder UsePortwiseDiagnosticContext(
        this IApplicationBuilder app)
    {
        app.Use(async (httpContext, next) =>
        {
            var diagnosticContext = httpContext.RequestServices
                .GetRequiredService<ApplicationDiagnosticContext>();
            var correlationId = httpContext.TraceIdentifier;
            httpContext.Response.Headers["X-Correlation-Id"] = correlationId;

            using var diagnosticScope = diagnosticContext.BeginScope(new DiagnosticScope(
                "http_request",
                CorrelationId: correlationId));
            await next();
        });

        return app;
    }

    public static async Task MigratePortwiseDatabaseAsync(
        this WebApplication app,
        CancellationToken cancellationToken = default)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var databaseLifecycle = scope.ServiceProvider
            .GetRequiredService<IDatabaseLifecycle>();
        await databaseLifecycle.MigrateAsync(cancellationToken);
    }

    public static async Task RunPortwiseAsync(
        this WebApplication app,
        CancellationToken cancellationToken = default)
    {
        app.UsePortwise();
        await app.MigratePortwiseDatabaseAsync(cancellationToken);
        await app.RunAsync(cancellationToken);
    }
}
