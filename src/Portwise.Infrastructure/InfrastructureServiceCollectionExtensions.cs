using Portwise.Application.Contracts;
using Portwise.Application.Stocks.Contracts;
using Portwise.Domain.Contracts;
using Portwise.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using Polly;
using Polly.Retry;
using Polly.Timeout;

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

        services.AddSingleton<
            IValidateOptions<FtShare.FtShareOptions>,
            FtShare.FtShareOptionsValidator>();
        services
            .AddOptions<FtShare.FtShareOptions>()
            .Bind(configuration.GetSection(FtShare.FtShareOptions.SectionName))
            .ValidateOnStart();
        services.AddTransient<FtShare.FtShareResponseStreamHandler>();
        services.AddResiliencePipeline<string>(
            FtShare.FtShareMcpToolInvoker.ExchangePipelineName,
            (builder, context) =>
            {
                var configuredOptions = context.GetOptions<FtShare.FtShareOptions>();
                context.EnableReloads<FtShare.FtShareOptions>();
                builder.AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = configuredOptions.MaxRetryCount,
                    Delay = configuredOptions.RetryDelay,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldHandle = new PredicateBuilder()
                        .Handle<FtShare.FtShareResponseStreamException>()
                        .Handle<ClientTransportClosedException>()
                });
            });
        var ftShareHttpClient = services
            .AddHttpClient(FtShare.FtShareMcpToolInvoker.HttpClientName);
        ftShareHttpClient.AddHttpMessageHandler<FtShare.FtShareResponseStreamHandler>();
        ftShareHttpClient
            .AddStandardResilienceHandler()
            .Configure((resilienceOptions, serviceProvider) =>
            {
                var configuredOptions = serviceProvider
                    .GetRequiredService<IOptions<FtShare.FtShareOptions>>()
                    .Value;

                resilienceOptions.TotalRequestTimeout.Timeout = configuredOptions.HttpRequestTimeout;
                resilienceOptions.AttemptTimeout.Timeout = configuredOptions.RequestTimeout;
                resilienceOptions.CircuitBreaker.SamplingDuration = TimeSpan.FromTicks(
                    checked(configuredOptions.RequestTimeout.Ticks * 2));
                resilienceOptions.Retry.MaxRetryAttempts = configuredOptions.MaxRetryCount;
                resilienceOptions.Retry.Delay = configuredOptions.RetryDelay;
                resilienceOptions.Retry.BackoffType = DelayBackoffType.Exponential;
                resilienceOptions.Retry.UseJitter = true;
                resilienceOptions.Retry.ShouldRetryAfterHeader = true;
                resilienceOptions.Retry.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<IOException>()
                    .Handle<TimeoutRejectedException>()
                    .HandleResult(response =>
                        response.StatusCode is
                            System.Net.HttpStatusCode.RequestTimeout or
                            System.Net.HttpStatusCode.TooManyRequests
                            || (int)response.StatusCode >= 500);
            });
        services.AddScoped<IFtShareMcpToolInvoker, FtShare.FtShareMcpToolInvoker>();
        services.AddScoped<IStockDataProvider, FtShare.FtShareStockDataProvider>();

        return services;
    }
}
