using DividendHarvest.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace DividendHarvest.Infrastructure;

internal sealed class DatabaseLifecycle(DividendHarvestDbContext dbContext) : IDatabaseLifecycle
{
    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        => dbContext.Database.CanConnectAsync(cancellationToken);

    public Task MigrateAsync(CancellationToken cancellationToken = default)
        => dbContext.Database.MigrateAsync(cancellationToken);
}
