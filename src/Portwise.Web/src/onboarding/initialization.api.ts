import { apiGet, apiPost } from "@/shared/http/api-client"
import type { CompleteInitializationRequest } from "@/shared/http/api-types"

export function getInitialization(signal?: AbortSignal) {
  return apiGet("/api/v1/initialization", {}, { signal })
}

export function completeInitialization(request: CompleteInitializationRequest, signal?: AbortSignal) {
  return apiPost("/api/v1/initialization/complete", request, {}, { signal })
}
