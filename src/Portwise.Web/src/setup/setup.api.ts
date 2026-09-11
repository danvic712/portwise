import { apiGet, apiPost } from "@/shared/http/api-client"
import type { SetupRequest } from "@/shared/http/api-types"

export async function getSetupStatus(signal?: AbortSignal) {
  return apiGet("/api/v1/setup/status", {}, { signal })
}

export async function initializeSetup(request: SetupRequest, signal?: AbortSignal) {
  return apiPost("/api/v1/setup", request, {}, { signal })
}
