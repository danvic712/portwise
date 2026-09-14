using FluentValidation;
using Portwise.Application.Exceptions;
using Portwise.Application.Contracts;
using Portwise.Application.Stocks.Contracts;
using Portwise.Application.Stocks.Dtos;
using Portwise.Application.Validators;
using Portwise.Domain.Contracts;
using Portwise.Domain.Codes;
using Portwise.Domain.Exceptions;
using Portwise.Domain.Models;
using Portwise.Domain.Securities;
using PortfolioEntity = Portwise.Domain.Models.Portfolio;

namespace Portwise.Application.Stocks;

public sealed class StockWatchlistAppService(
    IUow uow,
    TimeProvider timeProvider,
    IValidator<AddStockRequest> addStockRequestValidator) : IStockWatchlistAppService
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
                var item = StocksMapper.ToStockWatchlistItem(
                    security,
                    holdingsBySecurityId.TryGetValue(security.Id, out var position)
                        ? StocksMapper.ToStockHoldingSnapshot(position)
                        : null);
                return item with
                {
                    SecurityName = string.IsNullOrWhiteSpace(item.SecurityName)
                        ? string.Empty
                        : item.SecurityName
                };
            })
            .ToArray();
    }

    public async Task<StockWatchlistItem> AddAsync(
        AddStockRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var validationResult = await addStockRequestValidator.ValidateAsync(
            request,
            cancellationToken);
        if (!validationResult.IsValid)
        {
            throw ApplicationErrors.Validation(
                ApplicationErrorCodes.StockWatchlistValidationFailed,
                ValidationErrorFormatter.Format(validationResult));
        }

        var portfolio = await uow.Get<PortfolioEntity>()
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);
        if (portfolio is null)
        {
            throw ApplicationErrors.Simple(ApplicationErrorCodes.InitializationNotCompleted);
        }

        var reference = AShareReference.Create(
            request.SecurityCode,
            request.ExchangeCode);
        var securityRepository = uow.Get<Security>();
        var existingSecurity = await securityRepository.SingleOrDefaultAsync(
            security => security.SecurityCode == reference.SecurityCode
                && security.ExchangeCode == reference.ExchangeCode,
            cancellationToken);
        if (existingSecurity is not null)
        {
            throw ApplicationErrors.WithSecurityReference(
                ApplicationErrorCodes.StockAlreadyConfigured,
                reference.SecurityCode,
                reference.ExchangeCode);
        }

        var security = new Security
        {
            Id = Guid.CreateVersion7(),
            SecurityCode = reference.SecurityCode,
            ExchangeCode = reference.ExchangeCode,
            SecurityName = string.Empty,
            MarketCode = MarketCodes.AShare,
            CurrencyCode = CurrencyCodes.Cny
        };
        var position = new PortfolioPosition
        {
            PortfolioId = portfolio.Id,
            SecurityId = security.Id,
            HeldShares = request.HeldShares,
            CoreShares = request.HeldShares,
            TargetShares = request.HeldShares,
            AverageCostPerShare = 0m
        };

        await securityRepository.AddAsync(security, cancellationToken);
        await uow.Get<PortfolioPosition>().AddAsync(position, cancellationToken);
        await uow.Get<StockDataSyncJob>().AddAsync(
            StockDataSyncJob.Create("watchlist", timeProvider.GetUtcNow()),
            cancellationToken);
        try
        {
            await uow.CommitAsync(cancellationToken);
        }
        catch (UnitOfWorkCommitException exception)
            when (exception.IsUniqueConstraintViolation)
        {
            throw ApplicationErrors.WithSecurityReference(
                ApplicationErrorCodes.StockAlreadyConfigured,
                reference.SecurityCode,
                reference.ExchangeCode,
                exception);
        }

        return StocksMapper.ToStockWatchlistItem(
            security,
            StocksMapper.ToStockHoldingSnapshot(position));
    }
}
