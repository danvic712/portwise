import { apiGet, apiPut } from "@/shared/http/api-client"
import type { UpdatePreferencesRequest } from "@/shared/http/api-types"

export function getPreferences(signal?: AbortSignal) {
  return apiGet("/api/v1/preferences", {}, { signal })
}

export function updatePreferences(request: UpdatePreferencesRequest, signal?: AbortSignal) {
  return apiPut("/api/v1/preferences", request, {}, { signal })
}
