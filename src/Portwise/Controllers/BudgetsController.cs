using Portwise.Application.Contracts;
using Portwise.Application.Portfolio.Contracts;
using Portwise.Application.Portfolio.Dtos;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Portwise.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/budgets")]
public sealed class BudgetsController(IBudgetAppService budgetAppService)
    : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<BudgetSummary>> GetSummary(
        CancellationToken cancellationToken)
    {
        var summary = await budgetAppService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }

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
