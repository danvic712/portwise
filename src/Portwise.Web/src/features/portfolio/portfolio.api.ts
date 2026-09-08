import { apiGet, apiPost } from "@/lib/api-client"
import type { PortfolioTradeResult, RecordPortfolioTradeRequest, StockWatchlistItem } from "@/lib/api-types"

export async function getPortfolioStocks() {
  return apiGet<StockWatchlistItem[]>("/api/v1/stocks")
}

export async function recordPortfolioTrade(request: RecordPortfolioTradeRequest) {
  return apiPost<PortfolioTradeResult, "/api/v1/portfolio/trades", RecordPortfolioTradeRequest>("/api/v1/portfolio/trades", request)
}
