namespace Portwise.Application.Portfolio.Dtos;

/// <summary>Result returned after a cash ledger entry is recorded.</summary>
/// <param name="CashLedgerEntryId">Identifier of the created cash ledger entry.</param>
/// <param name="PortfolioId">Identifier of the portfolio.</param>
/// <param name="EntryDate">Date of the ledger entry.</param>
/// <param name="EntryTypeCode">Normalized ledger entry type code.</param>
/// <param name="CashDirectionCode">Normalized cash direction code.</param>
/// <param name="CashAmount">Cash amount recorded.</param>
/// <param name="SecurityCode">Optional security code associated with the entry.</param>
/// <param name="ExchangeCode">Optional exchange code associated with the entry.</param>
/// <param name="SourceRecordId">Optional source-system record identifier.</param>
public sealed record CashLedgerEntryResult(
    Guid CashLedgerEntryId,
    Guid PortfolioId,
    DateOnly EntryDate,
    string EntryTypeCode,
    string CashDirectionCode,
    decimal CashAmount,
    string? SecurityCode,
    string? ExchangeCode,
    string? SourceRecordId);
