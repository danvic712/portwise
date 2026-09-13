import type { InferenceProvidersResponse } from "@/shared/http/api-types"

type InferenceProviderSummary = InferenceProvidersResponse["providers"][number]

const exampleBaseUrls = new Set([
  "https://api.example.com/v1",
  "https://your-resource.openai.azure.com/openai/v1",
])

const legacyOpenAiCompatibleBaseUrl = "https://api.openai.com/v1"

/**
 * Returns whether a user-editable provider still contains one of the seeded
 * example addresses rather than a service address supplied by the user.
 */
export function isInferenceProviderBaseUrlPlaceholder(provider: InferenceProviderSummary) {
  if (!provider.isBaseUrlEditable) return false

  const baseUrl = provider.baseUrl.trim().replace(/\/+$/, "").toLowerCase()
  return exampleBaseUrls.has(baseUrl)
    || (provider.name.trim().toLowerCase() === "openai compatible" && baseUrl === legacyOpenAiCompatibleBaseUrl)
}

/**
 * Returns the value that should be placed in an editable address field.
 * Seed examples are kept in the database for schema compatibility, but are
 * shown as an empty draft so users must provide their own service address.
 */
export function getInferenceProviderBaseUrlDraft(provider: InferenceProviderSummary) {
  return isInferenceProviderBaseUrlPlaceholder(provider) ? "" : provider.baseUrl
}
