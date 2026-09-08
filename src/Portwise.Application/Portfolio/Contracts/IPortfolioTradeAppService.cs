using Portwise.Application.Portfolio.Dtos;

namespace Portwise.Application.Portfolio.Contracts;

public interface IPortfolioTradeAppService
{
    Task<PortfolioTradeResult> RecordAsync(
        RecordPortfolioTradeRequest request,
        CancellationToken cancellationToken);
}
