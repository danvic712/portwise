using Portwise.Domain.Enums;

namespace Portwise.Domain.Codes;

/// <summary>
/// Defines stable persistence and transport codes for application languages.
/// </summary>
public static class ApplicationLanguageCodes
{
    public const string ZhCn = "zh-CN";

    public const string EnUs = "en-US";

    public static string From(ApplicationLanguage language) => language switch
    {
        ApplicationLanguage.ZhCn => ZhCn,
        ApplicationLanguage.EnUs => EnUs,
        _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
    };

    public static bool TryParse(string? code, out ApplicationLanguage language)
    {
        switch (code)
        {
            case ZhCn:
                language = ApplicationLanguage.ZhCn;
                return true;
            case EnUs:
                language = ApplicationLanguage.EnUs;
                return true;
            default:
                language = default;
                return false;
        }
    }
}
