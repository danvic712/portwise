using Portwise.Application.Contracts;
using Portwise.Application.Dtos;
using Portwise.Application.Mapping;
using Portwise.Domain.Contracts;
using Portwise.Domain.Models;

namespace Portwise.Application.Stocks;

public sealed class StockWatchlistAppService(IUow uow) : IStockWatchlistAppService
{
    public async Task<IReadOnlyList<StockWatchlistItem>> GetAsync(
        CancellationToken cancellationToken)
    {
        var securities = await uow.Get<Security>()
                .ListAsync(
                    orderBy: [
                        security => security.SecurityCode,
                        security => security.ExchangeCode
                    ],
                    cancellationToken: cancellationToken);
        var positions = await uow.Get<PortfolioPosition>()
            .ListAsync(cancellationToken: cancellationToken);
        var holdingsBySecurityId = positions.ToDictionary(position => position.SecurityId);

        return securities
            .Select(security =>
            {
                var item = ApplicationMapper.ToStockWatchlistItem(
                    security,
                    holdingsBySecurityId.TryGetValue(security.Id, out var position)
                        ? ApplicationMapper.ToStockHoldingSnapshot(position)
                        : null);
                return item with
                {
                    SecurityName = string.IsNullOrWhiteSpace(item.SecurityName)
                        ? $"待同步 {security.SecurityCode}"
                        : item.SecurityName
                };
            })
            .ToArray();
    }
}
