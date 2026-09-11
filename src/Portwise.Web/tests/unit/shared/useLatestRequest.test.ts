import assert from "node:assert/strict"
import test from "node:test"

import { createLatestRequestController } from "../../../src/shared/hooks/useLatestRequest.ts"

test("latest request controller aborts and invalidates replaced work", () => {
  const controller = createLatestRequestController()
  const first = controller.begin()
  const second = controller.begin()

  assert.equal(first.signal.aborted, true)
  assert.equal(first.isCurrent(), false)
  assert.equal(second.isCurrent(), true)

  controller.cancel()
  assert.equal(second.signal.aborted, true)
  assert.equal(second.isCurrent(), false)
})
