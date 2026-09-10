import { useEffect, useState } from "react"

export type LatestRequest = {
  signal: AbortSignal
  isCurrent: () => boolean
}

export type LatestRequestController = {
  begin: () => LatestRequest
  cancel: () => void
}

export function createLatestRequestController(): LatestRequestController {
  let sequence = 0
  let controller: AbortController | null = null

  const cancel = () => {
    sequence += 1
    controller?.abort()
    controller = null
  }

  const begin = (): LatestRequest => {
    controller?.abort()

    const nextController = new AbortController()
    const currentSequence = sequence + 1
    sequence = currentSequence
    controller = nextController

    return {
      signal: nextController.signal,
      isCurrent: () => sequence === currentSequence && !nextController.signal.aborted,
    }
  }

  return { begin, cancel }
}

/**
 * Coordinates one replaceable async operation for a React module.
 * Starting a request cancels the previous one; cleanup invalidates all
 * outstanding results so an unmounted module cannot update its state.
 */
export function useLatestRequest() {
  const [controller] = useState(createLatestRequestController)

  useEffect(() => controller.cancel, [controller])

  return controller
}

export function isRequestAborted(error: unknown, signal?: AbortSignal) {
  return signal?.aborted === true
    || (error instanceof DOMException && error.name === "AbortError")
    || (typeof error === "object" && error !== null && "code" in error && error.code === "ERR_CANCELED")
}
