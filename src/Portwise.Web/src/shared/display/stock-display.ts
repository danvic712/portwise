import type { StockAnalysisResult, StockWatchlistItem } from "@/shared/http/api-types"

type StockLike = Pick<StockWatchlistItem, "securityCode"> & { securityName?: string | null }

export function displayStockName(stock: StockLike, pendingName = stock.securityCode) {
  const name = stock.securityName?.trim()
  return name || pendingName.replace("{securityCode}", stock.securityCode)
}

export function exchangeLabel(exchangeCode: string, labels: Record<string, string> = {}) {
  return labels[exchangeCode] ?? exchangeCode
}

export function analysisDisplayName(analysis: Pick<StockAnalysisResult, "securityName" | "securityCode">, pendingName = analysis.securityCode) {
  return displayStockName(analysis, pendingName)
}
