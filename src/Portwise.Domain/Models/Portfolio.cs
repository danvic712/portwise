namespace Portwise.Domain.Models;

using Portwise.Domain.Codes;

public sealed class Portfolio
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public string Name { get; set; } = string.Empty;

    public string CurrencyCode { get; set; } = CurrencyCodes.Cny;
}
