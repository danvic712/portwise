import { apiGet, apiPost } from "@/lib/api-client"
import type { BudgetSummary, CashLedgerEntryResult, RecordCashLedgerEntryRequest } from "@/lib/api-types"

export async function getBudget() {
  return apiGet<BudgetSummary>("/api/v1/budgets/summary")
}

export async function recordBudgetEntry(request: RecordCashLedgerEntryRequest) {
  return apiPost<CashLedgerEntryResult, "/api/v1/budgets/entries", RecordCashLedgerEntryRequest>("/api/v1/budgets/entries", request)
}
