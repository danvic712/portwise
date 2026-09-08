using Portwise.Application.Recommendations.Dtos;

namespace Portwise.Application.Recommendations.Contracts;

public interface IStockModelParameterAppService
{
    Task<StockModelParameterSet?> GetAsync(
        GetStockModelParametersRequest request,
        CancellationToken cancellationToken);

    Task<StockModelParameterSet> SaveAsync(
        SaveStockModelParametersRequest request,
        CancellationToken cancellationToken);
}
