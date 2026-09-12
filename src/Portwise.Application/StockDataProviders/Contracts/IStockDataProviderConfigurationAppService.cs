using Portwise.Application.StockDataProviders.Dtos;

namespace Portwise.Application.StockDataProviders.Contracts;

/// <summary>
/// Manages stock data provider instances, credentials, routes, and connection verification.
/// </summary>
public interface IStockDataProviderConfigurationAppService
{
    Task<StockDataProvidersResponse> GetProvidersAsync(CancellationToken cancellationToken);

    Task<StockDataProviderDto> CreateProviderAsync(
        CreateStockDataProviderRequest request,
        CancellationToken cancellationToken);

    Task<StockDataProviderDto> UpdateProviderAsync(
        Guid providerId,
        UpdateStockDataProviderRequest request,
        CancellationToken cancellationToken);

    Task DeleteProviderAsync(
        Guid providerId,
        long expectedRevision,
        CancellationToken cancellationToken);

    Task<VerifyStockDataProviderResponse> VerifyProviderAsync(
        Guid providerId,
        VerifyStockDataProviderRequest request,
        CancellationToken cancellationToken);

    Task<StockDataRoutesResponse> GetRoutesAsync(CancellationToken cancellationToken);

    Task<StockDataRoutesResponse> UpdateRoutesAsync(
        UpdateStockDataRoutesRequest request,
        CancellationToken cancellationToken);
}
