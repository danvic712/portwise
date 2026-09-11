namespace Portwise.Configuration;

/// <summary>
/// Configures persistent ASP.NET Core Data Protection storage for Portwise.
/// </summary>
public sealed class PortwiseDataProtectionOptions
{
    public const string SectionName = "DataProtection";

    public string KeysPath { get; set; } = "/app/keys";
}
