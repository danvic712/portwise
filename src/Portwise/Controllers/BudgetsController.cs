using Portwise.Application.Contracts;
using Portwise.Application.Portfolio.Contracts;
using Portwise.Application.Portfolio.Dtos;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Portwise.Controllers;

/// <summary>Provides cash budget summaries and ledger entry endpoints.</summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/budgets")]
public sealed class BudgetsController(IBudgetAppService budgetAppService)
    : ControllerBase
{
    /// <summary>Returns the current portfolio cash budget summary.</summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpGet("summary")]
    public async Task<ActionResult<BudgetSummary>> GetSummary(
        CancellationToken cancellationToken)
    {
        var summary = await budgetAppService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    /// <summary>Records a cash ledger entry and returns the created entry.</summary>
    /// <param name="request">Cash ledger entry details.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    [HttpPost("entries")]
    public async Task<ActionResult<CashLedgerEntryResult>> RecordEntry(
        [FromBody] RecordCashLedgerEntryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await budgetAppService.RecordAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetSummary),
            new { version = "1" },
            result);
    }
}
