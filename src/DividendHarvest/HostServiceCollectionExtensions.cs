using System.Globalization;
using Asp.Versioning;
using DividendHarvest.Application;
using DividendHarvest.Application.Contracts;
using DividendHarvest.Background;
using DividendHarvest.Configuration;
using DividendHarvest.Contracts;
using DividendHarvest.Diagnostics;
using DividendHarvest.ExceptionHandling;
using DividendHarvest.HealthChecks;
using DividendHarvest.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Enrichers.Span;
using ApplicationDiagnosticContext = DividendHarvest.Application.Contracts.IDiagnosticContext;

namespace DividendHarvest;

public static class HostServiceCollectionExtensions
{
    public static WebApplicationBuilder AddDividendHarvest(
        this WebApplicationBuilder builder)
    {
        builder.Services
            .AddDividendHarvestApplication()
            .AddDividendHarvestInfrastructure(builder.Configuration)
            .AddDividendHarvestHost(builder.Configuration);
        return builder;
    }

    public static IServiceCollection AddDividendHarvestHost(
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
            .AddOpenApi(options => options.Document.AddDocumentTransformer(
                (document, _, _) =>
                {
                    document.Info.Title = "Portwise API";
                    document.Info.Description = "Portwise A 股策略参考 API。";
                    return Task.CompletedTask;
                }));
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
