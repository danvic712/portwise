using Portwise.Application.Portfolio.Dtos;
using Portwise.Domain.Models;
using Riok.Mapperly.Abstractions;

namespace Portwise.Application.Portfolio;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class PortfolioMapper
{
    [MapProperty(nameof(CashLedgerEntry.Id), nameof(CashLedgerEntryResult.CashLedgerEntryId))]
    public static partial CashLedgerEntryResult ToCashLedgerEntryResult(
        CashLedgerEntry entry,
        string? securityCode,
        string? exchangeCode);
}
