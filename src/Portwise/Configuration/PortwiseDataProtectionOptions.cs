namespace Portwise.Configuration;

/// <summary>
/// Configures persistent ASP.NET Core Data Protection storage for Portwise.
/// </summary>
public sealed class PortwiseDataProtectionOptions
{
    public const string SectionName = "DataProtection";

    /// <summary>
    /// Gets or sets the key ring directory. Relative paths are resolved from
    /// the application's content root; absolute paths are used as provided.
    /// </summary>
    public string KeysPath { get; set; } = "/app/keys";
}
