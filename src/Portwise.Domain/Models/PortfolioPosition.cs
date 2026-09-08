namespace Portwise.Domain.Models;

public sealed class PortfolioPosition
{
    public Guid PortfolioId { get; set; }

    public Guid SecurityId { get; set; }

    public int HeldShares { get; set; }

    public int CoreShares { get; set; }

    public int TargetShares { get; set; }

    public decimal AverageCostPerShare { get; set; }

    public void ApplyBuy(
        int shareQuantity,
        decimal pricePerShare,
        decimal transactionFeeAmount)
    {
        if (shareQuantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(shareQuantity),
                shareQuantity,
                "Buy share quantity must be greater than zero.");
        }

        if (pricePerShare <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pricePerShare),
                pricePerShare,
                "Execution price must be greater than zero.");
        }

        if (transactionFeeAmount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(transactionFeeAmount),
                transactionFeeAmount,
                "Transaction fee cannot be negative.");
        }

        var existingCost = HeldShares * AverageCostPerShare;
        HeldShares += shareQuantity;
        AverageCostPerShare =
            (existingCost + shareQuantity * pricePerShare + transactionFeeAmount)
            / HeldShares;
    }

    public void ApplySell(int shareQuantity)
    {
        if (shareQuantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(shareQuantity),
                shareQuantity,
                "Sell share quantity must be greater than zero.");
        }

        if (shareQuantity > HeldShares)
        {
            throw new InvalidOperationException("Sell share quantity cannot exceed held shares.");
        }

        HeldShares -= shareQuantity;
        if (HeldShares == 0)
        {
            AverageCostPerShare = 0m;
        }
    }
}
