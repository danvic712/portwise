namespace Portwise.Application.Setup.Dtos;

public sealed record InitialHoldingInput(
    int HeldShares,
    int CoreShares,
    int TargetShares,
    decimal AverageCostPerShare);
