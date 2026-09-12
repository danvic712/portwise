using Microsoft.Extensions.Logging;
using Portwise.Application.StockDataProviders.Contracts;
using Portwise.Application.Stocks.Contracts;
using Portwise.Domain.Contracts;
using Portwise.Domain.Enums;
using Portwise.Domain.Models;

namespace Portwise.Infrastructure.StockDataProviders.Runtime;

internal sealed class StockDataProviderRuntimeResolver(
    IUow uow,
    IEnumerable<IStockDataProviderRuntimeAdapter> adapters,
    ILogger<StockDataProviderRuntimeResolver> logger)
    : IStockDataProviderRuntimeResolver, IStockDataProviderConnectionVerifier
{
    public async Task<IStockDataProvider?> ResolveAsync(
        StockDataCapability capability,
        CancellationToken cancellationToken)
    {
        var route = await uow.Get<StockDataRoute>()
            .SingleOrDefaultAsync(item => item.Capability == capability, cancellationToken);
        return route?.ProviderId is { } providerId
            ? await CreateProviderAsync(providerId, cancellationToken)
            : null;
    }

    public async Task<bool> VerifyAsync(
        Guid providerId,
        CancellationToken cancellationToken)
    {
        var resolved = await ResolveAdapterAsync(providerId, cancellationToken);
        if (resolved is null)
        {
            return false;
        }

        try
        {
            return await resolved.Value.Adapter.VerifyAsync(
                resolved.Value.Provider,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "Stock data provider verification failed for provider {ProviderId} with cause type {CauseType}.",
                providerId,
                exception.GetType().Name);
            return false;
        }
    }

    private async Task<IStockDataProvider?> CreateProviderAsync(
        Guid providerId,
        CancellationToken cancellationToken)
    {
        var resolved = await ResolveAdapterAsync(providerId, cancellationToken);
        return resolved is null
            ? null
            : await resolved.Value.Adapter.CreateAsync(
                resolved.Value.Provider,
                cancellationToken);
    }

    private async Task<ResolvedAdapter?> ResolveAdapterAsync(
        Guid providerId,
        CancellationToken cancellationToken)
    {
        var provider = await uow.Get<StockDataProvider>()
            .SingleOrDefaultAsync(item => item.Id == providerId, cancellationToken);
        if (provider is null)
        {
            return null;
        }

        var definition = await uow.Get<StockDataProviderDefinition>()
            .SingleOrDefaultAsync(
                item => item.Id == provider.ProviderDefinitionId,
                cancellationToken);
        if (definition is not { IsEnabled: true })
        {
            return null;
        }

        var adapter = adapters.SingleOrDefault(
            candidate => candidate.ProviderKind == definition.ProviderKind);
        return adapter is null ? null : new ResolvedAdapter(provider, adapter);
    }

    private readonly record struct ResolvedAdapter(
        StockDataProvider Provider,
        IStockDataProviderRuntimeAdapter Adapter);
}
