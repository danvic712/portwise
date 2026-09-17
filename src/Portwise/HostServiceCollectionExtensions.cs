using System.Globalization;
using Asp.Versioning;
using Portwise.Application;
using Portwise.Application.Contracts;
using Portwise.Background;
using Portwise.Configuration;
using Portwise.Contracts;
using Portwise.Diagnostics;
using Portwise.ExceptionHandling;
using Portwise.HealthChecks;
using Portwise.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
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
            .AddPortwiseHost(builder.Configuration, builder.Environment);
        return builder;
    }

    public static IServiceCollection AddPortwiseHost(
        this IServiceCollection services,
        IConfiguration configuration)
        => AddPortwiseHost(
            services,
            configuration,
            Directory.GetCurrentDirectory());

    public static IServiceCollection AddPortwiseHost(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(hostEnvironment);
        return AddPortwiseHost(
            services,
            configuration,
            hostEnvironment.ContentRootPath);
    }

    private static IServiceCollection AddPortwiseHost(
        IServiceCollection services,
        IConfiguration configuration,
        string contentRootPath)
    {
        var dataProtectionOptions = configuration
            .GetSection(PortwiseDataProtectionOptions.SectionName)
            .Get<PortwiseDataProtectionOptions>() ?? new PortwiseDataProtectionOptions();
        if (string.IsNullOrWhiteSpace(dataProtectionOptions.KeysPath))
        {
            throw new InvalidOperationException("DataProtection:KeysPath cannot be empty.");
        }

        var keysPath = ResolveKeysPath(
            dataProtectionOptions.KeysPath,
            contentRootPath);
        Directory.CreateDirectory(keysPath);
        services.AddDataProtection()
            .SetApplicationName("Portwise")
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath));
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
        services.AddExceptionHandler<ApplicationExceptionHandler>();
        services.AddPortwiseOpenApiDescription();
        services.AddSingleton<IStockDataSyncRunner, StockDataSyncRunner>();
        services.AddHostedService<StockDataSyncBackgroundService>();
        services.AddHostedService<DailyStockDataSyncHostedService>();
        services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
            .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);

        return services;
    }

    private static string ResolveKeysPath(
        string configuredPath,
        string contentRootPath)
    {
        if (Path.IsPathRooted(configuredPath))
        {
            return Path.GetFullPath(configuredPath);
        }

        if (string.IsNullOrWhiteSpace(contentRootPath))
        {
            throw new InvalidOperationException(
                "The application content root is required for a relative DataProtection:KeysPath.");
        }

        return Path.GetFullPath(configuredPath, contentRootPath);
    }
}
