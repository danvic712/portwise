using Portwise.Application.Contracts;
using Portwise.Application.Recommendations.Contracts;
using Portwise.Application.Recommendations.Dtos;
using Portwise.Application.Stocks.Dtos;
using Portwise.Application.Exceptions;
using Portwise.Application.Validators;
using Portwise.Domain.Contracts;
using Portwise.Domain.Codes;
using Portwise.Domain.Models;
using Portwise.Domain.Recommendations;
using Portwise.Domain.Securities;
using FluentValidation;

namespace Portwise.Application.Recommendations;

public sealed class StockAnalysisAppService(
    IUow uow,
    IValidator<GetStockAnalysisRequest> requestValidator,
    TimeProvider timeProvider) : IStockAnalysisAppService
{
    public async Task<IReadOnlyList<StockAnalysisResult>> GetAsync(
        IReadOnlyList<StockWatchlistItem> stocks,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stocks);
        if (stocks.Count == 0)
        {
            return [];
        }

        var securityIds = stocks
            .Select(stock => stock.SecurityId)
            .Where(securityId => securityId != Guid.Empty)
            .Distinct()
            .ToArray();
        var computedAt = timeProvider.GetUtcNow();
        var currentDate = DateOnly.FromDateTime(computedAt.UtcDateTime);
        var parameters = await uow.Get<ModelParameterSet>().ListAsync(
            parameter => securityIds.Contains(parameter.SecurityId)
                && parameter.EffectiveFromDate <= currentDate,
            cancellationToken: cancellationToken);
        var parametersBySecurityId = parameters
            .GroupBy(parameter => parameter.SecurityId)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(parameter => parameter.EffectiveFromDate).First());
        var priceObservations = await uow.Get<PriceObservation>().ListAsync(
            observation => securityIds.Contains(observation.SecurityId)
                && observation.TradingDate <= currentDate
                && observation.DataQualityCode == DataQualityCodes.Valid,
            orderBy: [observation => observation.TradingDate],
            descending: true,
            cancellationToken: cancellationToken);
        var dividendEvents = await uow.Get<DividendEvent>().ListAsync(
            dividendEvent => securityIds.Contains(dividendEvent.SecurityId),
            cancellationToken: cancellationToken);
        var financialSnapshots = await uow.Get<FinancialSnapshot>().ListAsync(
            snapshot => securityIds.Contains(snapshot.SecurityId),
            cancellationToken: cancellationToken);
        var pricesBySecurityId = priceObservations
            .GroupBy(observation => observation.SecurityId)
            .ToDictionary(group => group.Key, group => (IReadOnlyCollection<PriceObservation>)group.ToArray());
        var dividendsBySecurityId = dividendEvents
            .GroupBy(dividendEvent => dividendEvent.SecurityId)
            .ToDictionary(group => group.Key, group => (IReadOnlyCollection<DividendEvent>)group.ToArray());
        var financialsBySecurityId = financialSnapshots
            .GroupBy(snapshot => snapshot.SecurityId)
            .ToDictionary(group => group.Key, group => (IReadOnlyCollection<FinancialSnapshot>)group.ToArray());

        return stocks
            .Select(stock =>
            {
                var position = stock.Holding is null
                    ? null
                    : new PortfolioPosition
                    {
                        SecurityId = stock.SecurityId,
                        HeldShares = stock.Holding.HeldShares,
                        CoreShares = stock.Holding.CoreShares,
                        TargetShares = stock.Holding.TargetShares
                    };
                var calculation = RecommendationModule.CalculateStock(
                    new StockRecommendationInput(
                        stock.SecurityId,
                        parametersBySecurityId.GetValueOrDefault(stock.SecurityId),
                        pricesBySecurityId.GetValueOrDefault(stock.SecurityId) ?? [],
                        dividendsBySecurityId.GetValueOrDefault(stock.SecurityId) ?? [],
                        financialsBySecurityId.GetValueOrDefault(stock.SecurityId) ?? [],
                        position,
                        currentDate,
                        computedAt));
                return ToResult(stock, calculation);
            })
            .ToArray();
    }

    public async Task<StockAnalysisResult> GetAsync(
        GetStockAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var validationResult = await requestValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw ApplicationErrors.Validation(
                ApplicationErrorCodes.StockAnalysisValidationFailed,
                ValidationErrorFormatter.Format(validationResult));
        }

        var reference = AShareReference.Create(request.SecurityCode, request.ExchangeCode);
        var security = await uow.Get<Security>()
            .SingleOrDefaultAsync(
                item =>
                    item.SecurityCode == reference.SecurityCode
                    && item.ExchangeCode == reference.ExchangeCode,
                cancellationToken);
        if (security is null)
        {
            throw ApplicationErrors.WithSecurityReference(
                ApplicationErrorCodes.StockNotConfigured,
                reference.SecurityCode,
                reference.ExchangeCode);
        }

        var computedAt = timeProvider.GetUtcNow();
        var currentDate = DateOnly.FromDateTime(computedAt.UtcDateTime);
        var parameters = await uow.Get<ModelParameterSet>()
            .FirstOrDefaultAsync(
                parameter =>
                    parameter.SecurityId == security.Id
                    && parameter.EffectiveFromDate <= currentDate,
                orderBy: parameter => parameter.EffectiveFromDate,
                descending: true,
                cancellationToken: cancellationToken);
        var priceObservations = await uow.Get<PriceObservation>()
            .ListAsync(
                observation =>
                    observation.SecurityId == security.Id
                    && observation.TradingDate <= currentDate
                    && observation.DataQualityCode == DataQualityCodes.Valid,
                orderBy: [observation => observation.TradingDate],
                descending: true,
                cancellationToken: cancellationToken);
        var dividendEvents = await uow.Get<DividendEvent>()
            .ListAsync(
                dividendEvent => dividendEvent.SecurityId == security.Id,
                cancellationToken: cancellationToken);
        var financialSnapshots = await uow.Get<FinancialSnapshot>()
            .ListAsync(
                snapshot => snapshot.SecurityId == security.Id,
                cancellationToken: cancellationToken);
        var position = await uow.Get<PortfolioPosition>()
            .FirstOrDefaultAsync(
                currentPosition => currentPosition.SecurityId == security.Id
                    && (parameters == null
                        || currentPosition.PortfolioId == parameters.PortfolioId),
                cancellationToken: cancellationToken);

        var calculation = RecommendationModule.CalculateStock(
            new StockRecommendationInput(
                security.Id,
                parameters,
                priceObservations,
                dividendEvents,
                financialSnapshots,
                position,
                currentDate,
                computedAt));

        return ToResult(security, reference, calculation);
    }

    private static StockAnalysisResult ToResult(
        StockWatchlistItem stock,
        StockRecommendationCalculation calculation)
        => new(
            stock.SecurityCode,
            stock.ExchangeCode,
            stock.SecurityName,
            calculation.ModelStatusCode,
            calculation.DividendReliabilityCode,
            calculation.ClosePrice,
            calculation.ModelDividendPerShare,
            calculation.DividendModeCode,
            calculation.DividendYield,
            calculation.StrongBuyPrice,
            calculation.AccumulatePrice,
            calculation.PartialTrimPrice,
            calculation.AggressiveTrimPrice,
            calculation.ObservedPriceZoneCode,
            calculation.PriceZoneCode,
            calculation.PriceZoneConfirmed,
            calculation.RecommendationCode,
            calculation.HeldShares,
            calculation.CoreShares,
            calculation.SatelliteShares,
            calculation.DataAsOfDate,
            calculation.ModelParameterSetId,
            calculation.ComputedAt,
            calculation.Explanation,
            stock.SecurityId);

    private static StockAnalysisResult ToResult(
        Security security,
        AShareReference reference,
        StockRecommendationCalculation calculation)
        => new(
            reference.SecurityCode,
            reference.ExchangeCode,
            GetDisplaySecurityName(security, reference),
            calculation.ModelStatusCode,
            calculation.DividendReliabilityCode,
            calculation.ClosePrice,
            calculation.ModelDividendPerShare,
            calculation.DividendModeCode,
            calculation.DividendYield,
            calculation.StrongBuyPrice,
            calculation.AccumulatePrice,
            calculation.PartialTrimPrice,
            calculation.AggressiveTrimPrice,
            calculation.ObservedPriceZoneCode,
            calculation.PriceZoneCode,
            calculation.PriceZoneConfirmed,
            calculation.RecommendationCode,
            calculation.HeldShares,
            calculation.CoreShares,
            calculation.SatelliteShares,
            calculation.DataAsOfDate,
            calculation.ModelParameterSetId,
            calculation.ComputedAt,
            calculation.Explanation,
            calculation.SecurityId);

    private static string GetDisplaySecurityName(
        Security security,
        AShareReference reference)
        => string.IsNullOrWhiteSpace(security.SecurityName)
            ? string.Empty
            : security.SecurityName;
}
