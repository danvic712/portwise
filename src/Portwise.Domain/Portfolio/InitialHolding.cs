namespace Portwise.Domain.Portfolio;

public sealed record InitialHolding
{
    private InitialHolding(int heldShares, int coreShares, int targetShares, decimal averageCostPerShare)
    {
        HeldShares = heldShares;
        CoreShares = coreShares;
        TargetShares = targetShares;
        AverageCostPerShare = averageCostPerShare;
    }

    public int HeldShares { get; }

    public int CoreShares { get; }

    public int TargetShares { get; }

    public decimal AverageCostPerShare { get; }

    public static InitialHolding Create(
        int heldShares,
        int coreShares,
        int targetShares,
        decimal averageCostPerShare)
    {
        if (heldShares < 0)
        {
            throw new ArgumentException("Held shares cannot be negative.", nameof(heldShares));
        }

        if (coreShares < 0 || coreShares > heldShares)
        {
            throw new ArgumentException("Core shares cannot be negative or exceed held shares.", nameof(coreShares));
        }

        if (targetShares < 0)
        {
            throw new ArgumentException("Target shares cannot be negative.", nameof(targetShares));
        }

        if (averageCostPerShare < 0)
        {
            throw new ArgumentException("Average cost per share cannot be negative.", nameof(averageCostPerShare));
        }

        return new InitialHolding(heldShares, coreShares, targetShares, averageCostPerShare);
    }
}
