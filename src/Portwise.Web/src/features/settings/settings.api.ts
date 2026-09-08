import axios from "axios"

import { apiGet, apiPost } from "@/lib/api-client"
import type { SaveStockModelParametersRequest, StockModelParameterSet, StockWatchlistItem } from "@/lib/api-types"

export async function getSettingsStocks() {
  return apiGet<StockWatchlistItem[]>("/api/v1/stocks")
}

export async function getSettingsParameters(securityCode: string, exchangeCode: string, signal?: AbortSignal) {
  try {
    return await apiGet<StockModelParameterSet | null>("/api/v1/stocks/{securityCode}/{exchangeCode}/model-parameters", { securityCode, exchangeCode }, { signal })
  } catch (error) {
    if (axios.isAxiosError(error) && error.response?.status === 404) {
      return null
    }

    throw error
  }
}

export async function updateSettingsParameters(request: SaveStockModelParametersRequest) {
  return apiPost<StockModelParameterSet, "/api/v1/stocks/model-parameters", SaveStockModelParametersRequest>("/api/v1/stocks/model-parameters", request)
}
