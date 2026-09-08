namespace Portwise.Domain.Securities;

public sealed record AShareReference
{
    private static readonly string[] SupportedExchanges = ["SSE", "SZSE", "BSE"];

    private AShareReference(string securityCode, string exchangeCode)
    {
        SecurityCode = securityCode;
        ExchangeCode = exchangeCode;
    }

    public string SecurityCode { get; }

    public string ExchangeCode { get; }

    public static AShareReference Create(string securityCode, string exchangeCode)
    {
        var normalizedCode = securityCode?.Trim() ?? string.Empty;
        var normalizedExchange = exchangeCode?.Trim().ToUpperInvariant() ?? string.Empty;

        if (normalizedCode.Length != 6 || normalizedCode.Any(character => character is < '0' or > '9'))
        {
            throw new ArgumentException("Security code must contain exactly 6 digits.", nameof(securityCode));
        }

        if (!SupportedExchanges.Contains(normalizedExchange, StringComparer.Ordinal))
        {
            throw new ArgumentException("Exchange code must be SSE, SZSE or BSE.", nameof(exchangeCode));
        }

        return new AShareReference(normalizedCode, normalizedExchange);
    }
}
