using Portwise.Application.Portfolio.Dtos;

namespace Portwise.Application.Portfolio.Contracts;

public interface IBudgetAppService
{
    Task<CashLedgerEntryResult> RecordAsync(
        RecordCashLedgerEntryRequest request,
        CancellationToken cancellationToken);

    Task<BudgetSummary> GetSummaryAsync(CancellationToken cancellationToken);
}
