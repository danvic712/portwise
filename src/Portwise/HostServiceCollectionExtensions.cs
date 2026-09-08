using System.Globalization;
using Asp.Versioning;
using Portwise.Application;
using Portwise.Application.Contracts;
using Portwise.Application.Stocks.Contracts;
using Portwise.Background;
using Portwise.Configuration;
using Portwise.Contracts;
using Portwise.Diagnostics;
using Portwise.ExceptionHandling;
using Portwise.HealthChecks;
using Portwise.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Enrichers.Span;
using ApplicationDiagnosticContext = Portwise.Application.Contracts.IDiagnosticContext;

namespace Portwise;

public static class HostServiceCollectionExtensions
{
    public static WebApplicationBuilder AddPortwise(
        this WebApplicationBuilder builder)
    {
        builder.Services
            .AddPortwiseApplication()
            .AddPortwiseInfrastructure(builder.Configuration)
            .AddPortwiseHost(builder.Configuration);
        return builder;
    }

    public static IServiceCollection AddPortwiseHost(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSerilog((serviceProvider, loggerConfiguration) =>
            loggerConfiguration
                .ReadFrom.Configuration(configuration)
                .ReadFrom.Services(serviceProvider)
                .Enrich.FromLogContext()
                .Enrich.WithSpan());
        services.AddProblemDetails();
        services.AddLocalization();
        services.AddOptions<RequestLocalizationOptions>()
            .Configure<IApplicationErrorCatalog>((options, catalog) =>
            {
                var cultures = catalog.SupportedCultureNames
                    .Select(CultureInfo.GetCultureInfo)
                    .ToList();
                var defaultCulture = CultureInfo.GetCultureInfo(catalog.DefaultCultureName);
                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = cultures;
                options.SupportedUICultures = cultures;
            });
        services.AddSingleton<ApplicationDiagnosticContext, ActivityDiagnosticContext>();
        services.AddSingleton<IHttpErrorRenderer, ProblemDetailsErrorRenderer>();
        services.AddExceptionHandler<ApplicationExceptionHandler>();
        services.AddPortwiseOpenApiDescription();
        services.AddSingleton<IValidateOptions<DailySyncOptions>, DailySyncOptionsValidator>();
        services
            .AddOptions<DailySyncOptions>()
            .Bind(configuration.GetSection(DailySyncOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IStockDataSyncRunner, StockDataSyncRunner>();
        services.AddSingleton<StockDataSyncTaskQueue>();
        services.AddSingleton<IStockDataSyncScheduler>(serviceProvider =>
            serviceProvider.GetRequiredService<StockDataSyncTaskQueue>());
        services.AddHostedService<StockDataSyncBackgroundService>();
        services.AddHostedService<DailyStockDataSyncHostedService>();
        services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
            .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);

        return services;
    }
}
