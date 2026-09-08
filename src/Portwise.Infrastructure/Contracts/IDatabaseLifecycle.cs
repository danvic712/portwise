namespace Portwise.Infrastructure.Contracts;

public interface IDatabaseLifecycle
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);

    Task MigrateAsync(CancellationToken cancellationToken = default);
}
