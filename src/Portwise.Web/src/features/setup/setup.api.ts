import { apiGet, apiPost } from "@/lib/api-client"
import type { SetupRequest } from "@/lib/api-types"

export async function getSetupStatus(signal?: AbortSignal) {
  return apiGet("/api/v1/setup/status", {}, { signal })
}

export async function initializeSetup(request: SetupRequest, signal?: AbortSignal) {
  return apiPost("/api/v1/setup", request, {}, { signal })
}
