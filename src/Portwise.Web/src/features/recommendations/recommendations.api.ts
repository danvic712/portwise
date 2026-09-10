import { apiGet, apiPost } from "@/lib/api-client"

export async function getRecommendations(signal?: AbortSignal) {
  return apiGet("/api/v1/recommendations", {}, { signal })
}

export async function createRecommendationSnapshot(signal?: AbortSignal) {
  return apiPost("/api/v1/recommendations/snapshots", undefined, {}, { signal })
}
