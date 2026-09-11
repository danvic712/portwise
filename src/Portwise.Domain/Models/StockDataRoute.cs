using Portwise.Domain.Enums;

namespace Portwise.Domain.Models;

/// <summary>
/// Maps one stock data capability to its configured provider.
/// </summary>
public sealed class StockDataRoute
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public StockDataCapability Capability { get; set; }

    public Guid? ProviderId { get; set; }

    public long Revision { get; set; } = 1;

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
