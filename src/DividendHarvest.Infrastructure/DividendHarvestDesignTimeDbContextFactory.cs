using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DividendHarvest.Infrastructure;

internal sealed class DividendHarvestDesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<DividendHarvestDbContext>
{
    public DividendHarvestDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DividendHarvestDbContext>()
            .UseSqlite("Data Source=portwise.design.db")
            .Options;

        return new DividendHarvestDbContext(options);
    }
}
