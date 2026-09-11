using Portwise.Domain.Enums;

namespace Portwise.Domain.Codes;

/// <summary>
/// Defines stable persistence and transport codes for application themes.
/// </summary>
public static class ApplicationThemeCodes
{
    public const string System = "system";

    public const string Light = "light";

    public const string Dark = "dark";

    public static string From(ApplicationTheme theme) => theme switch
    {
        ApplicationTheme.System => System,
        ApplicationTheme.Light => Light,
        ApplicationTheme.Dark => Dark,
        _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null)
    };

    public static bool TryParse(string? code, out ApplicationTheme theme)
    {
        switch (code)
        {
            case System:
                theme = ApplicationTheme.System;
                return true;
            case Light:
                theme = ApplicationTheme.Light;
                return true;
            case Dark:
                theme = ApplicationTheme.Dark;
                return true;
            default:
                theme = default;
                return false;
        }
    }
}
