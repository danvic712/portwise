using Portwise.Application.Configuration.Contracts;
using Portwise.Application.StockDataProviders.Contracts;
using Portwise.Application.Stocks.Contracts;
using Portwise.Domain.Contracts;
using Portwise.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Portwise.Infrastructure.StockDataProviders.Runtime;
using Portwise.Application.Inference.Contracts;
using Portwise.Infrastructure.Inference;

namespace Portwise.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddPortwiseInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:Default is required for PostgreSQL.");

        services.AddDbContext<PortwiseDbContext>(options =>
            options.UsePortwisePostgreSql(connectionString));
        services.AddScoped<IUow, Repositories.EFUow>();
        services.AddScoped<IDatabaseLifecycle, DatabaseLifecycle>();
        services.AddSingleton<ISecretProtector, DataProtection.DataProtectionSecretProtector>();

        services.AddTransient<FtShare.FtShareResponseStreamHandler>();
        var ftShareHttpClient = services
            .AddHttpClient(
                FtShare.FtShareMcpToolInvoker.HttpClientName,
                client => client.Timeout = Timeout.InfiniteTimeSpan);
        ftShareHttpClient.AddHttpMessageHandler<FtShare.FtShareResponseStreamHandler>();
        services.AddScoped<IFtShareMcpToolInvoker, FtShare.FtShareMcpToolInvoker>();
        services.AddScoped<IStockDataProviderRuntimeAdapter, FtShareStockDataProviderRuntimeAdapter>();
        services.AddScoped<StockDataProviderRuntimeResolver>();
        services.AddScoped<IStockDataProviderRuntimeResolver>(serviceProvider =>
            serviceProvider.GetRequiredService<StockDataProviderRuntimeResolver>());
        services.AddScoped<IStockDataProviderConnectionVerifier>(serviceProvider =>
            serviceProvider.GetRequiredService<StockDataProviderRuntimeResolver>());
        services.AddScoped<IStockDataProvider, DatabaseRoutedStockDataProvider>();

        services.AddHttpClient(
            OpenAiCompatibleInferenceProviderConnectionVerifier.HttpClientName,
            client => client.Timeout = Timeout.InfiniteTimeSpan);
        services.AddScoped<
            IInferenceProviderConnectionVerifier,
            OpenAiCompatibleInferenceProviderConnectionVerifier>();

        return services;
    }
}
