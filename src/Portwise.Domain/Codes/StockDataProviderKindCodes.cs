using Portwise.Domain.Enums;

namespace Portwise.Domain.Codes;

/// <summary>
/// Defines stable persistence and transport codes for stock data provider kinds.
/// </summary>
public static class StockDataProviderKindCodes
{
    public const string FtShare = "ftshare";

    public static string From(StockDataProviderKind kind) => kind switch
    {
        StockDataProviderKind.FtShare => FtShare,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };

    public static bool TryParse(string? code, out StockDataProviderKind kind)
    {
        if (code == FtShare)
        {
            kind = StockDataProviderKind.FtShare;
            return true;
        }

        kind = default;
        return false;
    }
}
