using Portwise.Application.Configuration;
using Portwise.Application.Configuration.Contracts;
using Portwise.Application.Contracts;
using Portwise.Application.Stocks.Contracts;
using Portwise.Domain.Contracts;
using Portwise.Domain.Enums;
using Portwise.Domain.Models;
using Portwise.Infrastructure.Contracts;
using Portwise.Infrastructure.FtShare;

namespace Portwise.Infrastructure.StockDataProviders.Runtime;

internal sealed class FtShareStockDataProviderRuntimeAdapter(
    IUow uow,
    ISecretProtector secretProtector,
    IFtShareMcpToolInvoker toolInvoker,
    IDiagnosticContext diagnosticContext,
    TimeProvider timeProvider) : IStockDataProviderRuntimeAdapter
{
    public StockDataProviderKind ProviderKind => StockDataProviderKind.FtShare;

    public async Task<IStockDataProvider?> CreateAsync(
        StockDataProvider provider,
        CancellationToken cancellationToken)
    {
        var options = await ResolveOptionsAsync(provider, cancellationToken);
        return options is null
            ? null
            : new FtShareStockDataProvider(
                toolInvoker,
                options,
                diagnosticContext,
                timeProvider);
    }

    public async Task<bool> VerifyAsync(
        StockDataProvider provider,
        CancellationToken cancellationToken)
    {
        var options = await ResolveOptionsAsync(provider, cancellationToken);
        if (options is null)
        {
            return false;
        }

        await toolInvoker.VerifyAsync(options, cancellationToken);
        return true;
    }

    private async Task<FtShareOptions?> ResolveOptionsAsync(
        StockDataProvider provider,
        CancellationToken cancellationToken)
    {
        if (provider.ProtectedCredentials is not { Length: > 0 } protectedCredentials
            || !secretProtector.TryUnprotect(
                protectedCredentials,
                SecretProtectionPurpose.StockDataProviderCredentials,
                out var apiKey)
            || string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        var settings = await uow.Get<FtShareProviderSettings>()
            .SingleOrDefaultAsync(
                item => item.ProviderDefinitionId == provider.ProviderDefinitionId,
                cancellationToken);
        if (settings is null || !IsValid(settings))
        {
            return null;
        }

        return new FtShareOptions
        {
            McpEndpoint = settings.McpEndpoint,
            ApiKey = apiKey,
            StockProfileToolName = settings.StockProfileToolName,
            StockMarketDataToolName = settings.StockMarketDataToolName,
            StockDividendEventsToolName = settings.StockDividendEventsToolName,
            StockFinancialSnapshotsToolName = settings.StockFinancialSnapshotsToolName,
            SecurityCodeArgumentName = settings.SecurityCodeArgumentName,
            ExchangeCodeArgumentName = settings.ExchangeCodeArgumentName,
            RequestTimeoutSeconds = settings.RequestTimeoutSeconds,
            MaxRetryCount = settings.MaxRetryCount,
            RetryDelayMilliseconds = settings.RetryDelayMilliseconds
        };
    }

    private static bool IsValid(FtShareProviderSettings settings) =>
        Uri.TryCreate(settings.McpEndpoint, UriKind.Absolute, out var endpoint)
        && endpoint.Scheme is "http" or "https"
        && !string.IsNullOrWhiteSpace(settings.StockProfileToolName)
        && !string.IsNullOrWhiteSpace(settings.StockMarketDataToolName)
        && !string.IsNullOrWhiteSpace(settings.StockDividendEventsToolName)
        && !string.IsNullOrWhiteSpace(settings.StockFinancialSnapshotsToolName)
        && !string.IsNullOrWhiteSpace(settings.SecurityCodeArgumentName)
        && !string.IsNullOrWhiteSpace(settings.ExchangeCodeArgumentName)
        && settings.RequestTimeoutSeconds is >= 1 and <= 300
        && settings.MaxRetryCount is >= 0 and <= 5
        && settings.RetryDelayMilliseconds is >= 0 and <= 10_000;
}
