using Portwise.Application.Dtos;

namespace Portwise.Application.Contracts;

public interface IStockAnalysisAppService
{
    Task<StockAnalysisResult> GetAsync(
        GetStockAnalysisRequest request,
        CancellationToken cancellationToken);
}
