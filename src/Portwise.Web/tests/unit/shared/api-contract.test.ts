import assert from "node:assert/strict"
import test from "node:test"

import { apiPath, normalizeWireNumerics } from "../../../src/shared/http/api-contract.ts"

test("apiPath expands and encodes generated route parameters", () => {
  assert.equal(apiPath("/api/v1/stocks/{securityCode}/{exchangeCode}/model-parameters", { securityCode: "000001", exchangeCode: "SZSE" }), "/stocks/000001/SZSE/model-parameters")
  assert.throws(() => apiPath("/api/v1/stocks/{securityCode}/{exchangeCode}/model-parameters", { securityCode: "000001" }), /Missing path parameter/)
})

test("normalizeWireNumerics converts numeric wire strings without changing identifiers", () => {
  const value = normalizeWireNumerics({ tradingLotSize: "100", securityCode: "000001", effectiveFromDate: "2026-09-11", nested: [{ cashAmount: "12.50" }] })
  assert.deepEqual(value, { tradingLotSize: 100, securityCode: "000001", effectiveFromDate: "2026-09-11", nested: [{ cashAmount: 12.5 }] })
})
