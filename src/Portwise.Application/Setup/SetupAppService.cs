using Portwise.Application.Setup.Contracts;
using Portwise.Application.Setup.Dtos;
using Portwise.Application.Stocks.Contracts;
using Portwise.Application.Exceptions;
using Portwise.Application.Validators;
using Portwise.Domain.Contracts;
using Portwise.Domain.Exceptions;
using Portwise.Domain.Models;
using Portwise.Domain.Portfolio;
using Portwise.Domain.Securities;
using FluentValidation;
using PortfolioEntity = Portwise.Domain.Models.Portfolio;

namespace Portwise.Application.Setup;

public sealed class SetupAppService(
    IUow uow,
    IStockDataSyncScheduler stockDataSyncScheduler,
    IValidator<SetupRequest> requestValidator) : ISetupAppService
{
    public async Task<SetupStatusDto> GetStatusAsync(CancellationToken cancellationToken)
    {
        var isComplete = await uow.Get<PortfolioEntity>()
            .AnyAsync(cancellationToken: cancellationToken);

        return isComplete
            ? new SetupStatusDto(true, [])
            : new SetupStatusDto(false, ["portfolio", "stocks"]);
    }

    public async Task<SetupResult> InitializeAsync(
        SetupRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = await requestValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw ApplicationErrors.Validation(
                ApplicationErrorCodes.SetupValidationFailed,
                ValidationErrorFormatter.Format(validationResult));
        }

        if (await uow.Get<PortfolioEntity>()
            .AnyAsync(cancellationToken: cancellationToken))
        {
            throw ApplicationErrors.Simple(ApplicationErrorCodes.SetupAlreadyCompleted);
        }

        var portfolioName = request.PortfolioName.Trim();

        var references = request.Stocks
            .Select(stock =>
            {
                try
                {
                    return AShareReference.Create(stock.SecurityCode, stock.ExchangeCode);
                }
                catch (ArgumentException exception)
                {
                    throw ApplicationErrors.Validation(
                        ApplicationErrorCodes.SetupValidationFailed,
                        exception.Message);
                }
            })
            .ToArray();

        var portfolioId = Guid.NewGuid();

        var portfolioRepository = uow.Get<PortfolioEntity>();
        var securityRepository = uow.Get<Security>();
        var positionRepository = uow.Get<PortfolioPosition>();

        await portfolioRepository.AddAsync(
                new PortfolioEntity
                {
                    Id = portfolioId,
                    Name = portfolioName,
                    CurrencyCode = "CNY"
                },
            cancellationToken);

        for (var index = 0; index < references.Length; index++)
        {
            var reference = references[index];
            var requestStock = request.Stocks[index];
            var securityId = Guid.NewGuid();
            var initialHolding = requestStock.InitialHolding is null
                ? null
                : CreateInitialHolding(requestStock.InitialHolding);
            await securityRepository.AddAsync(
                new Security
                {
                    Id = securityId,
                    SecurityCode = reference.SecurityCode,
                    ExchangeCode = reference.ExchangeCode,
                    SecurityName = string.Empty,
                    MarketCode = "A-share",
                    CurrencyCode = "CNY"
                },
                cancellationToken);

            if (initialHolding is not null)
            {
                await positionRepository.AddAsync(
                    new PortfolioPosition
                    {
                        PortfolioId = portfolioId,
                        SecurityId = securityId,
                        HeldShares = initialHolding.HeldShares,
                        CoreShares = initialHolding.CoreShares,
                        TargetShares = initialHolding.TargetShares,
                        AverageCostPerShare = initialHolding.AverageCostPerShare
                    },
                    cancellationToken);
            }
        }

        try
        {
            await uow.CommitAsync(cancellationToken);
        }
        catch (UnitOfWorkCommitException exception) when (exception.IsUniqueConstraintViolation)
        {
            throw ApplicationErrors.Simple(
                ApplicationErrorCodes.SetupAlreadyCompleted,
                exception);
        }
        var stockDataSyncScheduled = stockDataSyncScheduler.TrySchedule();

        return new SetupResult(
            portfolioId,
            portfolioName,
            stockDataSyncScheduled,
            references
                .Select(reference => new SetupStockResult(
                    reference.SecurityCode,
                    reference.ExchangeCode,
                    null))
                .ToArray());
    }

    private static InitialHolding CreateInitialHolding(InitialHoldingInput input)
    {
        try
        {
            return InitialHolding.Create(
                input.HeldShares,
                input.CoreShares,
                input.TargetShares,
                input.AverageCostPerShare);
        }
        catch (ArgumentException exception)
        {
            throw ApplicationErrors.Validation(
                ApplicationErrorCodes.SetupValidationFailed,
                exception.Message);
        }
    }
}
