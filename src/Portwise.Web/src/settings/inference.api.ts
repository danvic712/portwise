import { apiDelete, apiGet, apiPost, apiPut } from "@/shared/http/api-client"
import type { CreateInferenceProviderRequest, UpdateInferenceProviderRequest, UpdateInferenceRoutesRequest, VerifyInferenceProviderRequest } from "@/shared/http/api-types"

export function getInferenceProviders(signal?: AbortSignal) {
  return apiGet("/api/v1/inference/providers", {}, { signal })
}

export function getInferenceRoutes(signal?: AbortSignal) {
  return apiGet("/api/v1/inference/routes", {}, { signal })
}

export function createInferenceProvider(request: CreateInferenceProviderRequest, signal?: AbortSignal) {
  return apiPost("/api/v1/inference/providers", request, {}, { signal })
}

export function updateInferenceProvider(providerId: string, request: UpdateInferenceProviderRequest, signal?: AbortSignal) {
  return apiPut("/api/v1/inference/providers/{providerId}", request, { providerId }, { signal })
}

export function deleteInferenceProvider(providerId: string, expectedRevision: number, signal?: AbortSignal) {
  return apiDelete("/api/v1/inference/providers/{providerId}", { providerId }, { params: { expectedRevision }, signal })
}

export function verifyInferenceProvider(providerId: string, request: VerifyInferenceProviderRequest, signal?: AbortSignal) {
  return apiPost("/api/v1/inference/providers/{providerId}/verify", request, { providerId }, { signal })
}

export function updateInferenceRoutes(request: UpdateInferenceRoutesRequest, signal?: AbortSignal) {
  return apiPut("/api/v1/inference/routes", request, {}, { signal })
}
