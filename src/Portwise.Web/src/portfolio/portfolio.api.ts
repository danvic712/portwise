import { apiPost } from "@/shared/http/api-client"
import type { RecordPortfolioTradeRequest } from "@/shared/http/api-types"

export async function recordPortfolioTrade(request: RecordPortfolioTradeRequest, signal?: AbortSignal) {
  return apiPost("/api/v1/portfolio/trades", request, {}, { signal })
}
