import { apiGet, apiPut } from "@/shared/http/api-client"
import type { StockDataSyncSettingsResponse, UpdateStockDataSyncSettingsRequest } from "@/shared/http/api-types"

export function getStockDataSyncSettings(signal?: AbortSignal) {
  return apiGet("/api/v1/stock-data-sync/settings", {}, {
    signal,
    headers: { "Cache-Control": "no-cache" },
  })
}

export function updateStockDataSyncSettings(
  request: UpdateStockDataSyncSettingsRequest,
  signal?: AbortSignal,
): Promise<StockDataSyncSettingsResponse> {
  return apiPut("/api/v1/stock-data-sync/settings", request, {}, { signal })
}
