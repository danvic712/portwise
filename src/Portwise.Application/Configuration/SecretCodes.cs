namespace Portwise.Application.Configuration;

/// <summary>
/// Defines stable secret update actions and read states.
/// </summary>
public static class SecretCodes
{
    public const string Keep = "keep";
    public const string Replace = "replace";
    public const string Clear = "clear";

    public const string Missing = "missing";
    public const string Configured = "configured";
    public const string Unreadable = "unreadable";
}
