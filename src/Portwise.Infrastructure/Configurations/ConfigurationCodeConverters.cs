using Portwise.Domain.Codes;
using Portwise.Domain.Enums;

namespace Portwise.Infrastructure.Configurations;

internal static class ConfigurationCodeConverters
{
    public static string ToCode(ApplicationLanguage value) => ApplicationLanguageCodes.From(value);

    public static ApplicationLanguage ToApplicationLanguage(string value) =>
        ApplicationLanguageCodes.TryParse(value, out var language)
            ? language
            : throw new ArgumentOutOfRangeException(nameof(value), value, null);

    public static string ToCode(ApplicationTheme value) => ApplicationThemeCodes.From(value);

    public static ApplicationTheme ToApplicationTheme(string value) =>
        ApplicationThemeCodes.TryParse(value, out var theme)
            ? theme
            : throw new ArgumentOutOfRangeException(nameof(value), value, null);

    public static string ToCode(ProviderVerificationState value) => value switch
    {
        ProviderVerificationState.Unverified => "unverified",
        ProviderVerificationState.Succeeded => "succeeded",
        ProviderVerificationState.Failed => "failed",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    public static ProviderVerificationState ToProviderVerificationState(string value) => value switch
    {
        "unverified" => ProviderVerificationState.Unverified,
        "succeeded" => ProviderVerificationState.Succeeded,
        "failed" => ProviderVerificationState.Failed,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    public static string ToCode(StockDataProviderKind value) => value switch
    {
        StockDataProviderKind.FtShare => "ftshare",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    public static StockDataProviderKind ToStockDataProviderKind(string value) => value switch
    {
        "ftshare" => StockDataProviderKind.FtShare,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    public static string ToCode(StockDataCapability value) => value switch
    {
        StockDataCapability.Profile => "profile",
        StockDataCapability.Market => "market",
        StockDataCapability.Dividend => "dividend",
        StockDataCapability.Financial => "financial",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    public static StockDataCapability ToStockDataCapability(string value) => value switch
    {
        "profile" => StockDataCapability.Profile,
        "market" => StockDataCapability.Market,
        "dividend" => StockDataCapability.Dividend,
        "financial" => StockDataCapability.Financial,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    public static string ToCode(InferenceProviderType value) => value switch
    {
        InferenceProviderType.OpenAiCompatible => "openai-compatible",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    public static InferenceProviderType ToInferenceProviderType(string value) => value switch
    {
        "openai-compatible" => InferenceProviderType.OpenAiCompatible,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    public static string ToCode(InferenceCapability value) => value switch
    {
        InferenceCapability.Chat => "chat",
        InferenceCapability.Embedding => "embedding",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    public static InferenceCapability ToInferenceCapability(string value) => value switch
    {
        "chat" => InferenceCapability.Chat,
        "embedding" => InferenceCapability.Embedding,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };
}
