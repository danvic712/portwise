using Portwise.Domain.Portfolio;

namespace Portwise.Domain.Models;

public sealed class CashLedgerEntry
{
    private CashLedgerEntry()
    {
    }

    public Guid Id { get; private set; }

    public Guid PortfolioId { get; private set; }

    public Guid? SecurityId { get; private set; }

    public DateOnly EntryDate { get; private set; }

    public string EntryTypeCode { get; private set; } = string.Empty;

    public string CashDirectionCode { get; private set; } = string.Empty;

    public decimal CashAmount { get; private set; }

    public string? SourceRecordId { get; private set; }

    public static CashLedgerEntry Create(
        Guid portfolioId,
        Guid? securityId,
        DateOnly entryDate,
        string entryTypeCode,
        string cashDirectionCode,
        decimal cashAmount,
        string? sourceRecordId)
    {
        if (portfolioId == Guid.Empty)
        {
            throw new ArgumentException("Portfolio identifier is required.", nameof(portfolioId));
        }

        if (securityId == Guid.Empty)
        {
            throw new ArgumentException("Security identifier is required.", nameof(securityId));
        }

        if (entryDate == DateOnly.MinValue)
        {
            throw new ArgumentException("Cash ledger date is required.", nameof(entryDate));
        }

        var normalizedEntryType = entryTypeCode?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!CashLedgerCodes.IsSupportedEntryType(normalizedEntryType))
        {
            throw new ArgumentException("Cash ledger type is not supported.", nameof(entryTypeCode));
        }

        var normalizedDirection = cashDirectionCode?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!CashLedgerCodes.IsSupportedDirection(normalizedDirection))
        {
            throw new ArgumentException("Cash ledger direction is not supported.", nameof(cashDirectionCode));
        }

        if (!CashLedgerCodes.IsCompatible(normalizedEntryType, normalizedDirection))
        {
            throw new ArgumentException("Cash ledger type and direction do not match.", nameof(cashDirectionCode));
        }

        if (cashAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cashAmount),
                cashAmount,
                "Cash ledger amount must be greater than zero.");
        }

        return new CashLedgerEntry
        {
            Id = Guid.CreateVersion7(),
            PortfolioId = portfolioId,
            SecurityId = securityId,
            EntryDate = entryDate,
            EntryTypeCode = normalizedEntryType,
            CashDirectionCode = normalizedDirection,
            CashAmount = cashAmount,
            SourceRecordId = string.IsNullOrWhiteSpace(sourceRecordId)
                ? null
                : sourceRecordId.Trim()
        };
    }
}
