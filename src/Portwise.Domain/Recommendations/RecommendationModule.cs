using Portwise.Domain.Codes;
using Portwise.Domain.DividendModel;
using Portwise.Domain.Models;
using Portwise.Domain.Portfolio;

namespace Portwise.Domain.Recommendations;

/// <summary>
/// Owns the composition of the dividend recommendation rules.
/// </summary>
public static class RecommendationModule
{
    public static StockRecommendationCalculation CalculateStock(
        StockRecommendationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        EnsureDate(input.CurrentDate, nameof(input.CurrentDate));
        EnsureComputedAt(input.ComputedAt);
        ArgumentNullException.ThrowIfNull(input.PriceObservations);
        ArgumentNullException.ThrowIfNull(input.DividendEvents);
        ArgumentNullException.ThrowIfNull(input.FinancialSnapshots);

        var priceObservation = input.PriceObservations
            .Where(observation =>
                observation.TradingDate <= input.CurrentDate
                && observation.DataQualityCode == DataQualityCodes.Valid)
            .OrderByDescending(observation => observation.TradingDate)
            .ThenByDescending(observation => observation.PriceObservedAt)
            .ThenByDescending(observation => observation.Id)
            .FirstOrDefault();
        var heldShares = input.Position?.HeldShares ?? 0;
        var coreShares = input.Position?.CoreShares ?? 0;
        var satelliteShares = Math.Max(heldShares - coreShares, 0);
        var modelDividendPerShare = priceObservation is null
            ? null
            : TtmDividendCalculator.Calculate(
                input.DividendEvents,
                priceObservation.TradingDate);
        var reliabilityCode = modelDividendPerShare is null
            ? DividendReliabilityCodes.Unavailable
            : DividendReliabilityEvaluator.Evaluate(
                input.DividendEvents,
                input.FinancialSnapshots,
                priceObservation!.TradingDate);

        if (input.Parameters is null
            || priceObservation is null
            || modelDividendPerShare is null)
        {
            return CreateUnavailableResult(
                input.SecurityId,
                priceObservation,
                modelDividendPerShare,
                reliabilityCode,
                heldShares,
                coreShares,
                satelliteShares,
                input.ComputedAt);
        }

        var hasRecentCancellation = DividendReliabilityEvaluator.HasRecentCancellation(
            input.DividendEvents,
            priceObservation.TradingDate);
        var modelStatusCode = hasRecentCancellation
            ? ModelStatusCodes.ReEvaluate
            : reliabilityCode switch
            {
                DividendReliabilityCodes.Passed => ModelStatusCodes.Available,
                DividendReliabilityCodes.Failed => ModelStatusCodes.Failed,
                _ => ModelStatusCodes.Cautious
            };
        var priceZoneValues = DividendPriceZoneCalculator.Calculate(
            input.Parameters,
            modelDividendPerShare.Value,
            priceObservation.ClosePrice);
        var priceZoneConfirmation = PriceZoneConfirmationCalculator.Calculate(
            input.Parameters,
            modelDividendPerShare.Value,
            input.PriceObservations
                .Where(observation => observation.TradingDate <= input.CurrentDate)
                .ToArray());
        var recommendationCode = GetRecommendationCode(
            modelStatusCode,
            priceZoneConfirmation.ConfirmedPriceZoneCode);
        var explanation = BuildExplanation(
            modelStatusCode,
            priceZoneConfirmation.IsConfirmed,
            priceZoneConfirmation.ConfirmedPriceZoneCode);

        return new StockRecommendationCalculation(
            input.SecurityId,
            modelStatusCode,
            reliabilityCode,
            priceObservation.ClosePrice,
            modelDividendPerShare,
            DividendModeCodes.Ttm,
            priceZoneValues.DividendYield,
            priceZoneValues.StrongBuyPrice,
            priceZoneValues.AccumulatePrice,
            priceZoneValues.PartialTrimPrice,
            priceZoneValues.AggressiveTrimPrice,
            priceZoneConfirmation.ObservedPriceZoneCode,
            priceZoneConfirmation.ConfirmedPriceZoneCode,
            priceZoneConfirmation.IsConfirmed,
            recommendationCode,
            heldShares,
            coreShares,
            satelliteShares,
            priceObservation.TradingDate,
            input.Parameters.Id,
            input.ComputedAt,
            explanation);
    }

    public static PortfolioRecommendationCalculation AllocatePortfolio(
        PortfolioRecommendationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        EnsureDate(input.CurrentDate, nameof(input.CurrentDate));
        EnsureComputedAt(input.ComputedAt);
        ArgumentNullException.ThrowIfNull(input.Stocks);
        ArgumentNullException.ThrowIfNull(input.ParameterSets);

        if (input.PortfolioId == Guid.Empty)
        {
            throw new ArgumentException("投资组合标识不能为空。", nameof(input.PortfolioId));
        }

        if (input.CashBalanceAmount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(input.CashBalanceAmount),
                input.CashBalanceAmount,
                "现金余额不能为负数。");
        }

        var parametersById = input.ParameterSets.ToDictionary(parameter => parameter.Id);
        var totalPortfolioValue = input.Stocks
            .Where(stock => stock.ClosePrice is not null)
            .Sum(stock => stock.HeldShares * stock.ClosePrice!.Value);
        var portfolioValuationComplete = input.Stocks
            .All(stock => stock.HeldShares <= 0 || stock.ClosePrice is not null);
        var sectorMarketValues = Enumerable.Range(0, input.Stocks.Count)
            .Where(index => !string.IsNullOrWhiteSpace(input.Stocks[index].SectorCode))
            .GroupBy(index => input.Stocks[index].SectorCode!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(index =>
                    input.Stocks[index].HeldShares
                    * (input.Stocks[index].ClosePrice ?? 0m)),
                StringComparer.OrdinalIgnoreCase);
        var cashReserveRatio = PortfolioBudgetCalculator.CalculateCurrentCashReserveRatio(
            input.ParameterSets,
            input.CurrentDate);
        var startingAvailableBudget = portfolioValuationComplete
            ? PortfolioBudgetCalculator.CalculateAvailableBudget(
                input.CashBalanceAmount,
                totalPortfolioValue,
                cashReserveRatio)
            : 0m;
        var remainingBudget = startingAvailableBudget;
        var totalSuggestedTradeAmount = 0m;
        var totalTransactionFeeAmount = 0m;
        var recommendations = input.Stocks
            .Select(stock => new PortfolioRecommendationStockCalculation(
                stock.SecurityId,
                0,
                0,
                0m,
                0m,
                stock.Explanation))
            .ToArray();

        var orderedIndexes = Enumerable.Range(0, input.Stocks.Count)
            .OrderBy(index => GetPricePriority(input.Stocks[index].PriceZoneCode))
            .ThenBy(index =>
                input.Stocks[index].DividendReliabilityCode
                    == DividendReliabilityCodes.Passed
                    ? 0
                    : 1)
            .ThenByDescending(index =>
                Math.Max(input.Stocks[index].TargetShares - input.Stocks[index].HeldShares, 0))
            .ThenBy(index => index)
            .ToArray();

        foreach (var index in orderedIndexes)
        {
            var stock = input.Stocks[index];
            if (stock.ModelParameterSetId is not { } parameterId
                || !parametersById.TryGetValue(parameterId, out var parameter)
                || stock.ClosePrice is not { } closePrice
                || stock.PriceZoneCode is not { } priceZoneCode)
            {
                continue;
            }

            var trade = TradeQuantityCalculator.Calculate(
                parameter,
                stock.ModelStatusCode,
                stock.DividendReliabilityCode,
                priceZoneCode,
                closePrice,
                stock.HeldShares,
                stock.CoreShares,
                stock.TargetShares,
                remainingBudget,
                totalPortfolioValue > 0 ? totalPortfolioValue : null,
                stock.HeldShares * closePrice,
                stock.SectorCode is { } sectorCode
                    && sectorMarketValues.TryGetValue(sectorCode, out var sectorMarketValue)
                    ? sectorMarketValue
                    : null);
            var explanation = stock.Explanation;
            if (trade.SuggestedBuyShares == 0
                && IsBuyZone(priceZoneCode)
                && stock.ModelStatusCode == ModelStatusCodes.Available
                && stock.DividendReliabilityCode == DividendReliabilityCodes.Passed)
            {
                explanation = !portfolioValuationComplete
                    ? $"{explanation} 组合中存在缺少有效收盘价的持仓，本期不生成买入建议。"
                    : $"{explanation} 本期组合预算或仓位额度不足，建议股数为 0。";
            }

            recommendations[index] = new PortfolioRecommendationStockCalculation(
                stock.SecurityId,
                trade.SuggestedBuyShares,
                trade.SuggestedSellShares,
                trade.SuggestedTradeAmount,
                trade.EstimatedTransactionFeeAmount,
                explanation);
            if (trade.SuggestedBuyShares > 0)
            {
                remainingBudget = Math.Max(
                    remainingBudget
                        - trade.SuggestedTradeAmount
                        - trade.EstimatedTransactionFeeAmount,
                    0m);
            }

            totalSuggestedTradeAmount += trade.SuggestedTradeAmount;
            totalTransactionFeeAmount += trade.EstimatedTransactionFeeAmount;
        }

        return new PortfolioRecommendationCalculation(
            input.PortfolioId,
            startingAvailableBudget,
            remainingBudget,
            totalSuggestedTradeAmount,
            totalTransactionFeeAmount,
            recommendations,
            input.ComputedAt);
    }

    private static StockRecommendationCalculation CreateUnavailableResult(
        Guid securityId,
        PriceObservation? priceObservation,
        decimal? modelDividendPerShare,
        string reliabilityCode,
        int heldShares,
        int coreShares,
        int satelliteShares,
        DateTimeOffset computedAt)
        => new(
            securityId,
            ModelStatusCodes.Unavailable,
            reliabilityCode,
            priceObservation?.ClosePrice,
            modelDividendPerShare,
            modelDividendPerShare is null ? null : DividendModeCodes.Ttm,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            false,
            RecommendationCodes.NoAction,
            heldShares,
            coreShares,
            satelliteShares,
            priceObservation?.TradingDate,
            null,
            computedAt,
            "缺少有效模型参数、行情或 TTM 实际股息，暂不生成价格区域和交易建议。");

    private static string GetRecommendationCode(
        string modelStatusCode,
        string? confirmedPriceZoneCode)
        => modelStatusCode switch
        {
            ModelStatusCodes.ReEvaluate => RecommendationCodes.ReEvaluate,
            ModelStatusCodes.Failed or ModelStatusCodes.Unavailable => RecommendationCodes.NoAction,
            ModelStatusCodes.Cautious => RecommendationCodes.Hold,
            _ => confirmedPriceZoneCode ?? RecommendationCodes.Hold
        };

    private static string BuildExplanation(
        string modelStatusCode,
        bool priceZoneConfirmed,
        string? confirmedPriceZoneCode)
        => modelStatusCode switch
        {
            ModelStatusCodes.Unavailable =>
                "缺少有效模型参数、行情或 TTM 实际股息，暂不生成价格区域和交易建议。",
            ModelStatusCodes.ReEvaluate =>
                "最近存在已确认取消分红的事件，需要重新评估核心仓和后续操作，当前不生成交易建议。",
            ModelStatusCodes.Failed =>
                "股息可靠性检查未通过，当前只展示行情和持仓信息，不生成交易建议。",
            ModelStatusCodes.Cautious =>
                "股息率和价格区域可以计算，但可靠性资料不足或存在风险提醒，当前仅谨慎持有。",
            _ when !priceZoneConfirmed =>
                "模型资料完整，但新的价格区域尚未连续两个有效交易日确认，当前仅观察。",
            _ => $"股息可靠性检查通过，已确认当前价格区域为 {confirmedPriceZoneCode}。"
        };

    private static int GetPricePriority(string? priceZoneCode)
        => priceZoneCode switch
        {
            PriceZoneCodes.StrongBuy => 0,
            PriceZoneCodes.Accumulate => 1,
            _ => 2
        };

    private static bool IsBuyZone(string priceZoneCode)
        => priceZoneCode is PriceZoneCodes.StrongBuy or PriceZoneCodes.Accumulate;

    private static void EnsureDate(DateOnly date, string parameterName)
    {
        if (date == DateOnly.MinValue)
        {
            throw new ArgumentException("参数日期不能为空。", parameterName);
        }
    }

    private static void EnsureComputedAt(DateTimeOffset computedAt)
    {
        if (computedAt == default)
        {
            throw new ArgumentException("计算时间不能为空。", nameof(computedAt));
        }
    }
}

public sealed record StockRecommendationInput(
    Guid SecurityId,
    ModelParameterSet? Parameters,
    IReadOnlyCollection<PriceObservation> PriceObservations,
    IReadOnlyCollection<DividendEvent> DividendEvents,
    IReadOnlyCollection<FinancialSnapshot> FinancialSnapshots,
    PortfolioPosition? Position,
    DateOnly CurrentDate,
    DateTimeOffset ComputedAt);

public sealed record StockRecommendationCalculation(
    Guid SecurityId,
    string ModelStatusCode,
    string DividendReliabilityCode,
    decimal? ClosePrice,
    decimal? ModelDividendPerShare,
    string? DividendModeCode,
    decimal? DividendYield,
    decimal? StrongBuyPrice,
    decimal? AccumulatePrice,
    decimal? PartialTrimPrice,
    decimal? AggressiveTrimPrice,
    string? ObservedPriceZoneCode,
    string? PriceZoneCode,
    bool PriceZoneConfirmed,
    string RecommendationCode,
    int HeldShares,
    int CoreShares,
    int SatelliteShares,
    DateOnly? DataAsOfDate,
    Guid? ModelParameterSetId,
    DateTimeOffset ComputedAt,
    string Explanation);

public sealed record PortfolioRecommendationInput(
    Guid PortfolioId,
    decimal CashBalanceAmount,
    IReadOnlyList<PortfolioRecommendationStockInput> Stocks,
    IReadOnlyList<ModelParameterSet> ParameterSets,
    DateOnly CurrentDate,
    DateTimeOffset ComputedAt);

public sealed record PortfolioRecommendationStockInput(
    Guid SecurityId,
    string? SectorCode,
    string ModelStatusCode,
    string DividendReliabilityCode,
    decimal? ClosePrice,
    string? PriceZoneCode,
    int HeldShares,
    int CoreShares,
    int TargetShares,
    Guid? ModelParameterSetId,
    string Explanation);

public sealed record PortfolioRecommendationCalculation(
    Guid PortfolioId,
    decimal StartingAvailableBudgetAmount,
    decimal RemainingAvailableBudgetAmount,
    decimal TotalSuggestedTradeAmount,
    decimal EstimatedTransactionFeeAmount,
    IReadOnlyList<PortfolioRecommendationStockCalculation> Stocks,
    DateTimeOffset ComputedAt);

public sealed record PortfolioRecommendationStockCalculation(
    Guid SecurityId,
    int SuggestedBuyShares,
    int SuggestedSellShares,
    decimal SuggestedTradeAmount,
    decimal EstimatedTransactionFeeAmount,
    string Explanation);
