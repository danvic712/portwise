export const applicationPaths = [
  "/",
  "/overview",
  "/setup",
  "/stocks",
  "/budget",
  "/portfolio",
  "/settings",
  "/404",
  "/error",
] as const

export type ApplicationPath = (typeof applicationPaths)[number]
export type QueryPatch = Record<string, string | null | undefined>

export type NavigationLocation = {
  pathname: string
  search: URLSearchParams
  hash: string
}

export type BrowserNavigation = {
  read: () => NavigationLocation
  navigate: (path: string, replace?: boolean) => void
  replaceQuery: (patch: QueryPatch) => void
  readPersistedPortfolioStock: () => string
  persistPortfolioStock: (stockKey: string) => void
  subscribe: (listener: (location: NavigationLocation) => void) => () => void
}

const portfolioStockStorageKey = "portwise-portfolio-stock"

export function isApplicationPath(pathname: string): pathname is ApplicationPath {
  return applicationPaths.includes(pathname as ApplicationPath)
}

export function resolveSetupPath(isComplete: boolean, pathname: string): ApplicationPath | null {
  if (!isComplete) return pathname === "/setup" ? null : "/setup"
  if (pathname === "/setup" || pathname === "/error") return "/overview"
  return isApplicationPath(pathname) ? null : "/404"
}

export function readNavigationLocation(href: string): NavigationLocation {
  const url = new URL(href, "http://portwise.local")
  return { pathname: url.pathname || "/", search: url.searchParams, hash: url.hash }
}

export function readPortfolioStockKey(location: NavigationLocation, fallback = "") {
  const queryStockKey = location.search.get("stock")
    ?? (location.search.get("code") && location.search.get("exchange")
      ? `${location.search.get("code")}:${location.search.get("exchange")}`
      : null)
  return queryStockKey ?? fallback
}

export function readPortfolioDirection(location: NavigationLocation): "buy" | "sell" {
  return location.search.get("direction") === "sell" ? "sell" : "buy"
}

export function readSettingsStockKey(location: NavigationLocation) {
  const securityCode = location.search.get("stock")
  const exchangeCode = location.search.get("exchange")
  return securityCode && exchangeCode ? `${securityCode}:${exchangeCode}` : ""
}

export function createBrowserNavigation(browserWindow: Window = window): BrowserNavigation {
  const listeners = new Set<(location: NavigationLocation) => void>()

  function read() {
    return readNavigationLocation(browserWindow.location.href)
  }

  function notify() {
    const location = read()
    listeners.forEach((listener) => listener(location))
  }

  function updateHistory(path: string, replace = false) {
    const url = new URL(path || "/overview", browserWindow.location.href)
    const nextPath = `${url.pathname || "/"}${url.search}${url.hash}`
    if (replace) browserWindow.history.replaceState({}, "", nextPath)
    else browserWindow.history.pushState({}, "", nextPath)
    notify()
  }

  function replaceQuery(patch: QueryPatch) {
    const url = new URL(browserWindow.location.href)
    Object.entries(patch).forEach(([key, value]) => {
      if (value === null || value === undefined || value === "") url.searchParams.delete(key)
      else url.searchParams.set(key, value)
    })
    updateHistory(`${url.pathname}${url.search}${url.hash}`, true)
  }

  function persistPortfolioStock(stockKey: string) {
    browserWindow.sessionStorage.setItem(portfolioStockStorageKey, stockKey)
    replaceQuery({ stock: stockKey, code: null, exchange: null })
  }

  function readPersistedPortfolioStock() {
    return browserWindow.sessionStorage.getItem(portfolioStockStorageKey) ?? ""
  }

  function subscribe(listener: (location: NavigationLocation) => void) {
    if (listeners.size === 0) browserWindow.addEventListener("popstate", notify)
    listeners.add(listener)
    return () => {
      listeners.delete(listener)
      if (listeners.size === 0) browserWindow.removeEventListener("popstate", notify)
    }
  }

  return { read, navigate: updateHistory, replaceQuery, readPersistedPortfolioStock, persistPortfolioStock, subscribe }
}
