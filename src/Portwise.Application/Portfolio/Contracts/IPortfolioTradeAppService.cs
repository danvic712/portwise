using Portwise.Application.Dtos;

namespace Portwise.Application.Contracts;

public interface IPortfolioTradeAppService
{
    Task<PortfolioTradeResult> RecordAsync(
        RecordPortfolioTradeRequest request,
        CancellationToken cancellationToken);
}
