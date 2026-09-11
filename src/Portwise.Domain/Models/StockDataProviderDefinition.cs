using Portwise.Domain.Enums;

namespace Portwise.Domain.Models;

/// <summary>
/// Describes a migration-managed stock data provider adapter.
/// </summary>
public sealed class StockDataProviderDefinition
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public StockDataProviderKind ProviderKind { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
