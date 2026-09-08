using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Portwise.Infrastructure;

internal sealed class PortwiseDesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<PortwiseDbContext>
{
    public PortwiseDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PortwiseDbContext>()
            .UseSqlite("Data Source=portwise.design.db")
            .Options;

        return new PortwiseDbContext(options);
    }
}
