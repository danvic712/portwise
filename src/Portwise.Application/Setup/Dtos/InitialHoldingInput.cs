namespace Portwise.Application.Setup.Dtos;

/// <summary>Optional initial holding values supplied during setup.</summary>
/// <param name="HeldShares">Initial total held shares.</param>
/// <param name="CoreShares">Initial core shares.</param>
/// <param name="TargetShares">Initial target share quantity.</param>
/// <param name="AverageCostPerShare">Initial average cost per share.</param>
public sealed record InitialHoldingInput(
    int HeldShares,
    int CoreShares,
    int TargetShares,
    decimal AverageCostPerShare);
