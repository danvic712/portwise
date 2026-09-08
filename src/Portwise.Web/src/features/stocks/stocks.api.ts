import axios from "axios"

import { apiGet, apiPost } from "@/lib/api-client"
import type { SaveStockModelParametersRequest, StockDataSyncRunResult, StockModelParameterSet, StockRecommendationResult, StockWatchlistItem } from "@/lib/api-types"

export async function getStocks() {
  return apiGet<StockWatchlistItem[]>("/api/v1/stocks")
}

export async function getStockAnalysis(securityCode: string, exchangeCode: string, signal?: AbortSignal) {
  return apiGet<StockRecommendationResult>("/api/v1/stocks/{securityCode}/{exchangeCode}/analysis", { securityCode, exchangeCode }, { signal })
}

export async function getModelParameters(securityCode: string, exchangeCode: string, signal?: AbortSignal) {
  try {
    return await apiGet<StockModelParameterSet | null>("/api/v1/stocks/{securityCode}/{exchangeCode}/model-parameters", { securityCode, exchangeCode }, { signal })
  } catch (error) {
    if (axios.isAxiosError(error) && error.response?.status === 404) {
      return null
    }

    throw error
  }
}

export async function syncStocks() {
  return apiPost<StockDataSyncRunResult, "/api/v1/stocks/sync">("/api/v1/stocks/sync")
}

export async function saveModelParameters(request: SaveStockModelParametersRequest) {
  return apiPost<StockModelParameterSet, "/api/v1/stocks/model-parameters", SaveStockModelParametersRequest>("/api/v1/stocks/model-parameters", request)
}
