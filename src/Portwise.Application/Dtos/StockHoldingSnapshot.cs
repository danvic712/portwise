namespace Portwise.Application.Dtos;

/// <summary>Snapshot of the shares held for one configured stock.</summary>
/// <param name="HeldShares">Total shares currently held.</param>
/// <param name="CoreShares">Shares assigned to the core position.</param>
/// <param name="TargetShares">Desired target share quantity.</param>
/// <param name="AverageCostPerShare">Average acquisition cost per share.</param>
public sealed record StockHoldingSnapshot(
    int HeldShares,
    int CoreShares,
    int TargetShares,
    decimal AverageCostPerShare);
