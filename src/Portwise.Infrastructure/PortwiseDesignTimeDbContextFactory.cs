using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Portwise.Infrastructure;

internal sealed class PortwiseDesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<PortwiseDbContext>
{
    public PortwiseDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? PortwisePostgreSqlOptions.DefaultConnectionString;
        var optionsBuilder = new DbContextOptionsBuilder<PortwiseDbContext>();
        optionsBuilder.UsePortwisePostgreSql(connectionString);

        return new PortwiseDbContext(optionsBuilder.Options);
    }
}
