import { apiDelete, apiGet, apiPost, apiPut } from "@/shared/http/api-client"
import type { CreateStockDataProviderRequest, UpdateStockDataProviderRequest, UpdateStockDataRoutesRequest, VerifyStockDataProviderRequest } from "@/shared/http/api-types"

export function getStockDataProviders(signal?: AbortSignal) {
  return apiGet("/api/v1/stock-data-providers", {}, { signal })
}

export function getStockDataRoutes(signal?: AbortSignal) {
  return apiGet("/api/v1/stock-data-providers/routes", {}, { signal })
}

export function createStockDataProvider(request: CreateStockDataProviderRequest, signal?: AbortSignal) {
  return apiPost("/api/v1/stock-data-providers", request, {}, { signal })
}

export function updateStockDataProvider(providerId: string, request: UpdateStockDataProviderRequest, signal?: AbortSignal) {
  return apiPut("/api/v1/stock-data-providers/{providerId}", request, { providerId }, { signal })
}

export function deleteStockDataProvider(providerId: string, expectedRevision: number, signal?: AbortSignal) {
  return apiDelete("/api/v1/stock-data-providers/{providerId}", { providerId }, { params: { expectedRevision }, signal })
}

export function verifyStockDataProvider(providerId: string, request: VerifyStockDataProviderRequest, signal?: AbortSignal) {
  return apiPost("/api/v1/stock-data-providers/{providerId}/verify", request, { providerId }, { signal })
}

export function updateStockDataRoutes(request: UpdateStockDataRoutesRequest, signal?: AbortSignal) {
  return apiPut("/api/v1/stock-data-providers/routes", request, {}, { signal })
}
