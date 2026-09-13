import assert from "node:assert/strict"
import test from "node:test"

import { getInferenceProviderBaseUrlDraft, isInferenceProviderBaseUrlPlaceholder } from "../../../src/shared/inference/provider-base-url.ts"
import type { InferenceProvidersResponse } from "../../../src/shared/http/api-types.ts"

type InferenceProvider = InferenceProvidersResponse["providers"][number]

function provider(overrides: Partial<{
  name: string
  baseUrl: string
  isBaseUrlEditable: boolean
}> = {}) {
  return {
    name: "OpenAI Compatible",
    baseUrl: "https://api.example.com/v1",
    isBaseUrlEditable: true,
    ...overrides,
  } as InferenceProvider
}

test("editable seeded inference addresses become empty drafts", () => {
  const seededProvider = provider()
  assert.equal(isInferenceProviderBaseUrlPlaceholder(seededProvider), true)
  assert.equal(getInferenceProviderBaseUrlDraft(seededProvider), "")
})

test("configured editable and fixed inference addresses remain unchanged", () => {
  const customProvider = provider({ baseUrl: "https://inference.example.net/v1" })
  const fixedProvider = provider({ baseUrl: "https://api.openai.com/v1", isBaseUrlEditable: false })
  assert.equal(isInferenceProviderBaseUrlPlaceholder(customProvider), false)
  assert.equal(getInferenceProviderBaseUrlDraft(customProvider), customProvider.baseUrl)
  assert.equal(getInferenceProviderBaseUrlDraft(fixedProvider), fixedProvider.baseUrl)
})

test("legacy OpenAI Compatible seed is treated as a placeholder", () => {
  const legacyProvider = provider({ baseUrl: "https://api.openai.com/v1" })
  assert.equal(isInferenceProviderBaseUrlPlaceholder(legacyProvider), true)
  assert.equal(getInferenceProviderBaseUrlDraft(legacyProvider), "")
})
