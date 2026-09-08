namespace Portwise.Application.Portfolio.Dtos;

/// <summary>Request to record a cash ledger entry.</summary>
/// <param name="EntryDate">Date of the ledger entry.</param>
/// <param name="EntryTypeCode">Ledger entry type code.</param>
/// <param name="CashDirectionCode">Cash direction code.</param>
/// <param name="CashAmount">Cash amount to record.</param>
/// <param name="SecurityCode">Optional security code associated with the entry.</param>
/// <param name="ExchangeCode">Optional exchange code associated with the entry.</param>
/// <param name="SourceRecordId">Optional source-system record identifier.</param>
public sealed record RecordCashLedgerEntryRequest(
    DateOnly EntryDate,
    string EntryTypeCode,
    string CashDirectionCode,
    decimal CashAmount,
    string? SecurityCode,
    string? ExchangeCode,
    string? SourceRecordId);
