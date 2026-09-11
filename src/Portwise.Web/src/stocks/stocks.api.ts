import axios from "axios"

import { apiGet, apiPost } from "@/shared/http/api-client"
import type { SaveStockModelParametersRequest } from "@/shared/http/api-types"

export async function getWatchedStocks(signal?: AbortSignal) {
  return apiGet("/api/v1/stocks", {}, { signal })
}

export async function getStockAnalysis(securityCode: string, exchangeCode: string, signal?: AbortSignal) {
  return apiGet("/api/v1/stocks/{securityCode}/{exchangeCode}/analysis", { securityCode, exchangeCode }, { signal })
}

export async function getStockModelParameters(securityCode: string, exchangeCode: string, signal?: AbortSignal) {
  try {
    return await apiGet("/api/v1/stocks/{securityCode}/{exchangeCode}/model-parameters", { securityCode, exchangeCode }, { signal })
  } catch (error) {
    if (axios.isAxiosError(error) && error.response?.status === 404) {
      return null
    }

    throw error
  }
}

export async function syncStocks(signal?: AbortSignal) {
  return apiPost("/api/v1/stocks/sync", undefined, {}, { signal })
}

export async function saveStockModelParameters(request: SaveStockModelParametersRequest, signal?: AbortSignal) {
  return apiPost("/api/v1/stocks/model-parameters", request, {}, { signal })
}
