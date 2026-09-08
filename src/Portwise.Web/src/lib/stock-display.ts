import type { StockAnalysisResult, StockWatchlistItem } from "@/lib/api-types"

type StockLike = Pick<StockWatchlistItem, "securityCode"> & { securityName?: string | null }

export function displayStockName(stock: StockLike) {
  const name = stock.securityName?.trim()
  return name || `待同步 ${stock.securityCode}`
}

export function exchangeLabel(exchangeCode: string) {
  return ({ SSE: "沪市", SZSE: "深市", BSE: "北市" } as Record<string, string>)[exchangeCode] ?? exchangeCode
}

export function analysisDisplayName(analysis: Pick<StockAnalysisResult, "securityName" | "securityCode">) {
  return displayStockName(analysis)
}
