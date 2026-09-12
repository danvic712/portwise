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

    public static string ToCode(ProviderVerificationState value) =>
        ProviderVerificationStateCodes.From(value);

    public static ProviderVerificationState ToProviderVerificationState(string value) =>
        ProviderVerificationStateCodes.TryParse(value, out var state)
            ? state
            : throw new ArgumentOutOfRangeException(nameof(value), value, null);

    public static string ToCode(StockDataProviderKind value) =>
        StockDataProviderKindCodes.From(value);

    public static StockDataProviderKind ToStockDataProviderKind(string value) =>
        StockDataProviderKindCodes.TryParse(value, out var kind)
            ? kind
            : throw new ArgumentOutOfRangeException(nameof(value), value, null);

    public static string ToCode(StockDataCapability value) =>
        StockDataCapabilityCodes.From(value);

    public static StockDataCapability ToStockDataCapability(string value) =>
        StockDataCapabilityCodes.TryParse(value, out var capability)
            ? capability
            : throw new ArgumentOutOfRangeException(nameof(value), value, null);

    public static string ToCode(InferenceProviderType value) =>
        InferenceProviderTypeCodes.From(value);

    public static InferenceProviderType ToInferenceProviderType(string value) =>
        InferenceProviderTypeCodes.TryParse(value, out var providerType)
            ? providerType
            : throw new ArgumentOutOfRangeException(nameof(value), value, null);

    public static string ToCode(InferenceCapability value) =>
        InferenceCapabilityCodes.From(value);

    public static InferenceCapability ToInferenceCapability(string value) =>
        InferenceCapabilityCodes.TryParse(value, out var capability)
            ? capability
            : throw new ArgumentOutOfRangeException(nameof(value), value, null);
}
