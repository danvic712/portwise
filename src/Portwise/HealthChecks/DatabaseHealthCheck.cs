using Portwise.Infrastructure.Contracts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Portwise.HealthChecks;

public sealed class DatabaseHealthCheck(IServiceScopeFactory serviceScopeFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var databaseLifecycle = scope.ServiceProvider
                .GetRequiredService<IDatabaseLifecycle>();
            return await databaseLifecycle.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Database is unavailable.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Database health check failed.", exception);
        }
    }
}
