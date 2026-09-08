using Portwise.Application.Contracts;
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
            ?? "Data Source=portwise.db";

        services.AddDbContext<PortwiseDbContext>(options =>
            options.UseSqlite(connectionString));
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
