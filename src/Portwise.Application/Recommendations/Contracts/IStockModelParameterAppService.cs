using Portwise.Application.Dtos;

namespace Portwise.Application.Contracts;

public interface IStockModelParameterAppService
{
    Task<StockModelParameterSet?> GetAsync(
        GetStockModelParametersRequest request,
        CancellationToken cancellationToken);

    Task<StockModelParameterSet> SaveAsync(
        SaveStockModelParametersRequest request,
        CancellationToken cancellationToken);
}
