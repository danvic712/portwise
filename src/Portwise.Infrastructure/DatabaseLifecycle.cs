using Portwise.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Portwise.Infrastructure;

internal sealed class DatabaseLifecycle(PortwiseDbContext dbContext) : IDatabaseLifecycle
{
    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        => dbContext.Database.CanConnectAsync(cancellationToken);

    public Task MigrateAsync(CancellationToken cancellationToken = default)
        => dbContext.Database.MigrateAsync(cancellationToken);
}
