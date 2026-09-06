import { useCallback, useEffect, useRef, useState } from "react"

import { ApplicationErrorPage } from "@/components/application-error-page"
import { NotFoundPage } from "@/components/not-found-page"
import { ScrollToTop } from "@/components/scroll-to-top"
import { StatusPageSkeleton } from "@/components/status-page-shell"
import { ThemeProvider } from "@/components/theme-provider"
import { getApiErrorMessage } from "@/lib/api-errors"
import { LocaleProvider, useLocale } from "@/lib/i18n"
import { getSetupStatus } from "@/features/setup/setup.api"
import type { SetupResult, SetupStatus } from "@/lib/api-types"
import { SetupPage } from "@/features/setup/SetupPage"
import { RecommendationsPage } from "@/features/recommendations/RecommendationsPage"
import { StocksPage } from "@/features/stocks/StocksPage"
import { BudgetPage } from "@/features/budget/BudgetPage"
import { PortfolioPage } from "@/features/portfolio/PortfolioPage"
import { SettingsPage } from "@/features/settings/SettingsPage"

function currentPath() {
  return window.location.pathname
}

const applicationPaths = new Set(["/", "/overview", "/setup", "/stocks", "/budget", "/portfolio", "/settings", "/404", "/error"])

export default function App() {
  return <LocaleProvider><AppContent /></LocaleProvider>
}

function AppContent() {
  const { locale, messages } = useLocale()
  const setupErrorRef = useRef(messages.common.application_error_unknown.detail)
  const [path, setPath] = useState(currentPath)
  const [portfolioStockKey, setPortfolioStockKey] = useState(() => {
    const query = new URLSearchParams(window.location.search)
    const queryStockKey = query.get("stock") ?? (query.get("code") && query.get("exchange") ? `${query.get("code")}:${query.get("exchange")}` : null)
    return queryStockKey ?? window.sessionStorage.getItem("dividend-harvest-portfolio-stock") ?? ""
  })
  const [setupStatus, setSetupStatus] = useState<SetupStatus | null>(null)
  const [setupNotice, setSetupNotice] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const navigate = useCallback((nextPath: string, replace = false) => {
    const [pathname] = nextPath.split("?")
    const updateHistory = replace ? window.history.replaceState.bind(window.history) : window.history.pushState.bind(window.history)
    updateHistory({}, "", nextPath)
    setPath(pathname || "/overview")
  }, [])

  useEffect(() => {
    document.title = messages.common.ui.pageTitle
    document.documentElement.lang = locale
  }, [locale, messages.common.ui.pageTitle])

  useEffect(() => {
    setupErrorRef.current = messages.common.application_error_unknown.detail
  }, [messages.common.application_error_unknown.detail])

  const checkSetup = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const status = await getSetupStatus()
      setSetupStatus(status)
      const nextPath = currentPath()
      if (!status.isComplete && nextPath !== "/setup") {
        navigate("/setup", true)
      } else if (status.isComplete && (nextPath === "/setup" || nextPath === "/error")) {
        navigate("/overview", true)
      } else if (status.isComplete && !applicationPaths.has(nextPath)) {
        navigate("/404", true)
      }
    } catch (statusError) {
      setError(getApiErrorMessage(statusError, setupErrorRef.current))
      navigate("/error", true)
    } finally {
      setLoading(false)
    }
  }, [navigate])

  useEffect(() => {
    const timeoutId = window.setTimeout(() => { void checkSetup() }, 0)
    const handlePopState = () => {
      const nextPath = currentPath()
      if (setupStatus?.isComplete && !applicationPaths.has(nextPath)) {
        navigate("/404", true)
        return
      }
      setPath(nextPath)
    }
    window.addEventListener("popstate", handlePopState)
    return () => {
      window.clearTimeout(timeoutId)
      window.removeEventListener("popstate", handlePopState)
    }
  }, [checkSetup, navigate, setupStatus?.isComplete])

  function renderPage() {
    if (path === "/setup") return <SetupPage onComplete={(result: SetupResult) => { setSetupStatus({ isComplete: true, missingRequirements: [] }); setSetupNotice(result.stockDataSyncScheduled ? messages.common.ui.states.setupCompleteSync : messages.common.ui.states.setupCompleteDeferred); navigate("/overview") }} />
    if (path === "/" || path === "/overview") return <RecommendationsPage onNavigate={navigate} notice={setupNotice} />
    if (path === "/404") return <NotFoundPage onNavigate={navigate} />
    if (path === "/error") return <ApplicationErrorPage message={messages.common.application_error_unknown.detail} onRetry={() => void checkSetup()} onNavigate={navigate} />
    if (path === "/stocks") return <StocksPage onNavigate={navigate} />
    if (path === "/budget") return <BudgetPage onNavigate={navigate} />
    if (path === "/portfolio") return <PortfolioPage onNavigate={navigate} selectedStockKey={portfolioStockKey} onSelectedStockKeyChange={setPortfolioStockKey} />
    if (path === "/settings") return <SettingsPage onNavigate={navigate} />
    return <NotFoundPage onNavigate={navigate} />
  }

  return (
    <ThemeProvider>
      {loading ? <StatusPageSkeleton label={messages.common.ui.states.connecting} onNavigate={navigate} /> : error ? <ApplicationErrorPage message={error} onRetry={() => void checkSetup()} onNavigate={(nextPath) => { navigate(nextPath); if (nextPath === "/overview") void checkSetup() }} /> : setupStatus?.isComplete || path === "/setup" ? renderPage() : <StatusPageSkeleton label={messages.common.ui.states.preparingSetup} onNavigate={navigate} />}
      <ScrollToTop />
    </ThemeProvider>
  )
}
