namespace Portwise.Domain.Codes;

/// <summary>
/// Provides stable UUID v7 identifiers for singleton and migration-managed records.
/// </summary>
public static class KnownConfigurationIds
{
    public static readonly Guid ApplicationPreferences = Guid.Parse("01a0929e-0a1a-7589-a243-8f1744fd3324");

    public static readonly Guid InitializationState = Guid.Parse("01a0929e-0a1b-7c5e-8a7d-07c611bcdf94");

    public static readonly Guid FtShareProviderDefinition = Guid.Parse("01a0929e-0a1c-7ffa-86f5-e6626219e9d2");

    public static readonly Guid FtShareProviderSettings = Guid.Parse("01a0929e-0a1d-721b-9e74-ce8d31633069");

    public static readonly Guid StockProfileRoute = Guid.Parse("01a0929e-0a1e-79eb-b895-61b60ce05be8");

    public static readonly Guid StockMarketRoute = Guid.Parse("01a0929e-0a1f-7fa1-9e4c-bc69e95fa39e");

    public static readonly Guid StockDividendRoute = Guid.Parse("01a0929e-0a20-7e10-a15e-f1b39354fb0b");

    public static readonly Guid StockFinancialRoute = Guid.Parse("01a0929e-0a21-74d9-a33f-e04567185f75");

    public static readonly Guid InferenceChatRoute = Guid.Parse("01a0929e-0a22-7eec-bdf8-94c2db32b317");

    public static readonly Guid InferenceEmbeddingRoute = Guid.Parse("01a0929e-0a23-7c2e-9948-e2028030f99a");
}
