import { apiGet, apiPost } from "@/lib/api-client"
import type { SetupRequest, SetupResult, SetupStatus } from "@/lib/api-types"

export async function getSetupStatus() {
  return apiGet<SetupStatus>("/api/v1/setup/status")
}

export async function initializeSetup(request: SetupRequest) {
  return apiPost<SetupResult, "/api/v1/setup", SetupRequest>("/api/v1/setup", request)
}
