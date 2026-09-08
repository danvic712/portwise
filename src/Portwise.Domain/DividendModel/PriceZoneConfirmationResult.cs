namespace Portwise.Domain.DividendModel;

public sealed record PriceZoneConfirmationResult(
    string ObservedPriceZoneCode,
    string? ConfirmedPriceZoneCode,
    bool IsConfirmed);
