using Portwise.Application.Contracts;
using Portwise.Application.Stocks.Validators;
using Portwise.Application.Stocks;
using Portwise.Application.Stocks.Contracts;
using Portwise.Application.Stocks.Dtos;
using Portwise.Domain.Contracts;
using Portwise.Domain.Models;
using Moq;
using Xunit;
using PortfolioEntity = Portwise.Domain.Models.Portfolio;

namespace Portwise.Application.Tests;

public sealed class StockWatchlistAppServiceTests
{
    [Fact]
    public async Task GetAsync_returns_sorted_stocks_with_matching_optional_holdings()
    {
        var firstSecurityId = Guid.NewGuid();
        var secondSecurityId = Guid.NewGuid();
        var securityRepository = CreateRepository<Security>([
            new Security
            {
                Id = firstSecurityId,
                SecurityCode = "600036",
                ExchangeCode = "SSE",
                SecurityName = "招商银行",
                MarketCode = "A-share",
                CurrencyCode = "CNY"
            },
            new Security
            {
                Id = secondSecurityId,
                SecurityCode = "000001",
                ExchangeCode = "SZSE",
                SecurityName = "平安银行",
                MarketCode = "A-share",
                CurrencyCode = "CNY"
            }
        ]);
        var positionRepository = CreateRepository<PortfolioPosition>([
            new PortfolioPosition
            {
                PortfolioId = Guid.NewGuid(),
                SecurityId = firstSecurityId,
                HeldShares = 100,
                CoreShares = 60,
                TargetShares = 200,
                AverageCostPerShare = 10.25m
            }
        ]);
        var unitOfWork = new Mock<IUow>();
        unitOfWork
            .Setup(x => x.Get<Security>())
            .Returns(securityRepository.Object);
        unitOfWork
            .Setup(x => x.Get<PortfolioPosition>())
            .Returns(positionRepository.Object);
        IStockWatchlistAppService service = new StockWatchlistAppService(
            unitOfWork.Object,
            new AddStockRequestValidator());

        var result = await service.GetAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("000001", result[0].SecurityCode);
        Assert.Equal("平安银行", result[0].SecurityName);
        Assert.Null(result[0].Holding);
        Assert.Equal("600036", result[1].SecurityCode);
        var holding = result[1].Holding;
        Assert.NotNull(holding);
        Assert.Equal(100, holding!.HeldShares);
        Assert.Equal(60, holding!.CoreShares);
        Assert.Equal(10.25m, holding!.AverageCostPerShare);
    }

    [Fact]
    public async Task GetAsync_uses_exchange_as_tie_breaker_for_same_security_code()
    {
        var securityRepository = CreateRepository<Security>([
            new Security
            {
                Id = Guid.NewGuid(),
                SecurityCode = "000001",
                ExchangeCode = "SZSE",
                SecurityName = "深交所股票",
                MarketCode = "A-share",
                CurrencyCode = "CNY"
            },
            new Security
            {
                Id = Guid.NewGuid(),
                SecurityCode = "000001",
                ExchangeCode = "SSE",
                SecurityName = "上交所股票",
                MarketCode = "A-share",
                CurrencyCode = "CNY"
            }
        ]);
        var positionRepository = CreateRepository<PortfolioPosition>([]);
        var unitOfWork = new Mock<IUow>();
        unitOfWork.Setup(x => x.Get<Security>()).Returns(securityRepository.Object);
        unitOfWork
            .Setup(x => x.Get<PortfolioPosition>())
            .Returns(positionRepository.Object);

        var result = await new StockWatchlistAppService(
                unitOfWork.Object,
                new AddStockRequestValidator())
            .GetAsync(CancellationToken.None);

        Assert.Equal(["SSE", "SZSE"], result.Select(item => item.ExchangeCode));
    }

    [Fact]
    public async Task GetAsync_uses_pending_name_when_profile_has_not_synced()
    {
        var securityRepository = CreateRepository<Security>([
            new Security
            {
                Id = Guid.NewGuid(),
                SecurityCode = "000001",
                ExchangeCode = "SZSE",
                SecurityName = string.Empty,
                MarketCode = "A-share",
                CurrencyCode = "CNY"
            }
        ]);
        var positionRepository = CreateRepository<PortfolioPosition>([]);
        var unitOfWork = new Mock<IUow>();
        unitOfWork.Setup(x => x.Get<Security>()).Returns(securityRepository.Object);
        unitOfWork
            .Setup(x => x.Get<PortfolioPosition>())
            .Returns(positionRepository.Object);

        var result = await new StockWatchlistAppService(
                unitOfWork.Object,
                new AddStockRequestValidator())
            .GetAsync(CancellationToken.None);

        var stock = Assert.Single(result);
        Assert.Equal(string.Empty, stock.SecurityName);
    }

    [Fact]
    public async Task AddAsync_creates_security_and_initial_position_in_one_commit()
    {
        var portfolio = new PortfolioEntity { Id = Guid.CreateVersion7(), Name = "长期组合" };
        var securityRepository = CreateRepository<Security>([]);
        var positionRepository = CreateRepository<PortfolioPosition>([]);
        var portfolioRepository = CreateRepository<PortfolioEntity>([portfolio]);
        var unitOfWork = new Mock<IUow>();
        unitOfWork.Setup(x => x.Get<Security>()).Returns(securityRepository.Object);
        unitOfWork.Setup(x => x.Get<PortfolioPosition>()).Returns(positionRepository.Object);
        unitOfWork.Setup(x => x.Get<PortfolioEntity>()).Returns(portfolioRepository.Object);
        unitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await new StockWatchlistAppService(
                unitOfWork.Object,
                new AddStockRequestValidator())
            .AddAsync(
                new AddStockRequest(" 000001 ", " szse ", 100),
                CancellationToken.None);

        Assert.Equal("000001", result.SecurityCode);
        Assert.Equal("SZSE", result.ExchangeCode);
        Assert.Equal(100, result.Holding?.HeldShares);
        var security = Assert.Single(
            securityRepository.Invocations
                .Where(invocation => invocation.Method.Name == nameof(IRepository<Security>.AddAsync))
                .Select(invocation => invocation.Arguments[0])
                .OfType<Security>());
        Assert.NotEqual(Guid.Empty, security.Id);
        var position = Assert.Single(
            positionRepository.Invocations
                .Where(invocation => invocation.Method.Name == nameof(IRepository<PortfolioPosition>.AddAsync))
                .Select(invocation => invocation.Arguments[0])
                .OfType<PortfolioPosition>());
        Assert.Equal(portfolio.Id, position.PortfolioId);
        Assert.Equal(security.Id, position.SecurityId);
        Assert.Equal(100, position.HeldShares);
        Assert.Equal(100, position.CoreShares);
        Assert.Equal(100, position.TargetShares);
        unitOfWork.Verify(item => item.CommitAsync(CancellationToken.None), Times.Once);
    }

    private static Mock<IRepository<TEntity>> CreateRepository<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : class
        => RepositoryMock.Create(entities);
}
