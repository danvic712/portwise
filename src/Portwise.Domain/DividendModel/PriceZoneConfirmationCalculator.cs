using Portwise.Domain.Codes;
using Portwise.Domain.Models;

namespace Portwise.Domain.DividendModel;

public static class PriceZoneConfirmationCalculator
{
    public static PriceZoneConfirmationResult Calculate(
        ModelParameterSet parameters,
        decimal modelDividendPerShare,
        IReadOnlyCollection<PriceObservation> observations)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(observations);

        var latestObservations = observations
            .Where(observation => observation.DataQualityCode == DataQualityCodes.Valid)
            .GroupBy(observation => observation.TradingDate)
            .Select(group => group
                .OrderByDescending(observation => observation.PriceObservedAt)
                .ThenByDescending(observation => observation.Id)
                .First())
            .OrderByDescending(observation => observation.TradingDate)
            .Take(2)
            .ToArray();
        if (latestObservations.Length == 0)
        {
            throw new ArgumentException("At least one valid price observation is required.", nameof(observations));
        }

        var observedZone = DividendPriceZoneCalculator.Calculate(
            parameters,
            modelDividendPerShare,
            latestObservations[0].ClosePrice).PriceZoneCode;
        if (latestObservations.Length < 2)
        {
            return new PriceZoneConfirmationResult(observedZone, null, false);
        }

        var previousZone = DividendPriceZoneCalculator.Calculate(
            parameters,
            modelDividendPerShare,
            latestObservations[1].ClosePrice).PriceZoneCode;
        return string.Equals(observedZone, previousZone, StringComparison.Ordinal)
            ? new PriceZoneConfirmationResult(observedZone, observedZone, true)
            : new PriceZoneConfirmationResult(observedZone, null, false);
    }
}
