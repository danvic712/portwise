import assert from "node:assert/strict"
import test from "node:test"

import {
  isApplicationPath,
  readNavigationLocation,
  readPortfolioDirection,
  readPortfolioStockKey,
  readSettingsStockKey,
  resolveSetupPath,
} from "../../../src/app/routing/navigation.ts"

test("resolveSetupPath keeps incomplete users on setup and completed users out", () => {
  assert.equal(resolveSetupPath(false, "/stocks"), "/setup")
  assert.equal(resolveSetupPath(false, "/setup"), null)
  assert.equal(resolveSetupPath(true, "/setup"), "/overview")
  assert.equal(resolveSetupPath(true, "/unknown"), "/404")
})

test("readNavigationLocation preserves pathname, query and hash", () => {
  const location = readNavigationLocation("https://portwise.local/portfolio?stock=000001%3ASZSE#trade")
  assert.equal(location.pathname, "/portfolio")
  assert.equal(location.search.get("stock"), "000001:SZSE")
  assert.equal(location.hash, "#trade")
})

test("portfolio and settings selection prefer their supported query shapes", () => {
  const portfolioLocation = readNavigationLocation("/portfolio?code=000001&exchange=SZSE")
  assert.equal(readPortfolioStockKey(portfolioLocation, "600001:SSE"), "000001:SZSE")
  assert.equal(readPortfolioDirection(readNavigationLocation("/portfolio?direction=sell")), "sell")
  assert.equal(readPortfolioDirection(readNavigationLocation("/portfolio?direction=other")), "buy")
  assert.equal(readSettingsStockKey(readNavigationLocation("/settings?stock=600001&exchange=SSE")), "600001:SSE")
})

test("isApplicationPath rejects arbitrary routes", () => {
  assert.equal(isApplicationPath("/overview"), true)
  assert.equal(isApplicationPath("/external"), false)
})
