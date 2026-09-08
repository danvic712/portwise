using Portwise.Domain.Models;
using Portwise.Domain.Codes;

namespace Portwise.Domain.DividendModel;

public static class DividendPriceZoneCalculator
{
    public static PriceZoneResult Calculate(
        ModelParameterSet parameters,
        decimal modelDividendPerShare,
        decimal closePrice)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        if (modelDividendPerShare <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(modelDividendPerShare),
                modelDividendPerShare,
                "Model dividend per share must be greater than zero.");
        }

        if (closePrice <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(closePrice),
                closePrice,
                "Current price must be greater than zero.");
        }

        var strongBuyPrice =
            modelDividendPerShare / parameters.StrongBuyYieldThreshold;
        var accumulatePrice =
            modelDividendPerShare / parameters.AccumulationYieldThreshold;
        var partialTrimPrice =
            modelDividendPerShare / parameters.PartialTrimYieldThreshold;
        var aggressiveTrimPrice =
            modelDividendPerShare / parameters.AggressiveTrimYieldThreshold;
        var priceZoneCode = closePrice <= strongBuyPrice
            ? PriceZoneCodes.StrongBuy
            : closePrice <= accumulatePrice
                ? PriceZoneCodes.Accumulate
                : closePrice < partialTrimPrice
                    ? PriceZoneCodes.Hold
                    : closePrice < aggressiveTrimPrice
                        ? PriceZoneCodes.PartialTrim
                        : PriceZoneCodes.AggressiveTrim;

        return new PriceZoneResult(
            strongBuyPrice,
            accumulatePrice,
            partialTrimPrice,
            aggressiveTrimPrice,
            modelDividendPerShare / closePrice,
            priceZoneCode);
    }
}
