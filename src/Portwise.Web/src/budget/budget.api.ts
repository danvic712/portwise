import { apiGet, apiPost } from "@/shared/http/api-client"
import type { RecordCashLedgerEntryRequest } from "@/shared/http/api-types"

export async function getBudgetSummary(signal?: AbortSignal) {
  return apiGet("/api/v1/budgets/summary", {}, { signal })
}

export async function recordBudgetEntry(request: RecordCashLedgerEntryRequest, signal?: AbortSignal) {
  return apiPost("/api/v1/budgets/entries", request, {}, { signal })
}
