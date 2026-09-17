using Portwise.Application.Stocks.Dtos;

namespace Portwise.Application.Stocks.Contracts;

/// <summary>Reads and updates the database-backed stock synchronization schedule.</summary>
public interface IStockDataSyncSettingsAppService
{
    Task<StockDataSyncSettingsResponse> GetAsync(CancellationToken cancellationToken);

    Task<StockDataSyncSettingsResponse> UpdateAsync(
        UpdateStockDataSyncSettingsRequest request,
        CancellationToken cancellationToken);
}
