using Portwise.Domain.Enums;

namespace Portwise.Domain.Codes;

/// <summary>
/// Defines stable persistence and transport codes for stock data capabilities.
/// </summary>
public static class StockDataCapabilityCodes
{
    public const string Profile = "profile";

    public const string Market = "market";

    public const string Dividend = "dividend";

    public const string Financial = "financial";

    public static string From(StockDataCapability capability) => capability switch
    {
        StockDataCapability.Profile => Profile,
        StockDataCapability.Market => Market,
        StockDataCapability.Dividend => Dividend,
        StockDataCapability.Financial => Financial,
        _ => throw new ArgumentOutOfRangeException(nameof(capability), capability, null)
    };

    public static bool TryParse(string? code, out StockDataCapability capability)
    {
        switch (code)
        {
            case Profile:
                capability = StockDataCapability.Profile;
                return true;
            case Market:
                capability = StockDataCapability.Market;
                return true;
            case Dividend:
                capability = StockDataCapability.Dividend;
                return true;
            case Financial:
                capability = StockDataCapability.Financial;
                return true;
            default:
                capability = default;
                return false;
        }
    }
}
