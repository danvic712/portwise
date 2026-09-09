using Portwise.Application.Contracts;
using Portwise.Application.Stocks.Contracts;
using Portwise.Domain.Contracts;
using Portwise.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        services.AddScoped<IFtShareMcpToolInvoker, FtShare.FtShareMcpToolInvoker>();
        services.AddScoped<IStockDataProvider, FtShare.FtShareStockDataProvider>();

        return services;
    }
}
