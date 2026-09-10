import { apiPost } from "@/lib/api-client"
import type { RecordPortfolioTradeRequest } from "@/lib/api-types"

export async function recordPortfolioTrade(request: RecordPortfolioTradeRequest, signal?: AbortSignal) {
  return apiPost("/api/v1/portfolio/trades", request, {}, { signal })
}
