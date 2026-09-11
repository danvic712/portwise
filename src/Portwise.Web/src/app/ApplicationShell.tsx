import { lazy, Suspense, useCallback, useEffect, useRef, useState, useSyncExternalStore } from "react"

import { ApplicationErrorPage } from "@/components/feedback/ApplicationErrorPage"
import { NotFoundPage } from "@/components/feedback/NotFoundPage"
import { RouteErrorBoundary } from "@/components/feedback/RouteErrorBoundary"
import { ScrollToTop } from "@/components/layout/ScrollToTop"
import { StatusPageSkeleton } from "@/components/feedback/StatusPageShell"
import { ThemeProvider } from "@/app/providers/ThemeProvider"
import { getApiErrorMessage } from "@/shared/http/api-errors"
import { useLocale } from "@/shared/i18n/i18n"
import { isRequestAborted, useLatestRequest } from "@/shared/hooks/useLatestRequest"
import { getSetupStatus } from "@/setup/setup.api"
import { SetupPageLoading } from "@/setup/SetupPageLoading"
import type { SetupResult, SetupStatus } from "@/shared/http/api-types"
import {
  createBrowserNavigation,
  readPortfolioDirection,
  readPortfolioStockKey,
  readSettingsStockKey,
  resolveSetupPath,
} from "@/app/routing/navigation"

const SetupPage = lazy(async () => ({ default: (await import("@/setup/SetupPage")).SetupPage }))
const RecommendationsPage = lazy(async () => ({ default: (await import("@/recommendations/RecommendationsPage")).RecommendationsPage }))
const StocksPage = lazy(async () => ({ default: (await import("@/stocks/StocksPage")).StocksPage }))
const BudgetPage = lazy(async () => ({ default: (await import("@/budget/BudgetPage")).BudgetPage }))
const PortfolioPage = lazy(async () => ({ default: (await import("@/portfolio/PortfolioPage")).PortfolioPage }))
const SettingsPage = lazy(async () => ({ default: (await import("@/settings/SettingsPage")).SettingsPage }))

export function ApplicationShell() {
  const { locale, messages } = useLocale()
  const [navigation] = useState(() => createBrowserNavigation())
  const location = useSyncExternalStore(navigation.subscribe, navigation.read, navigation.read)
  const setupErrorRef = useRef(messages.common.application_error_unknown.detail)
  const [portfolioStockKey, setPortfolioStockKey] = useState(() =>
    readPortfolioStockKey(navigation.read(), navigation.readPersistedPortfolioStock()),
  )
  const [setupStatus, setSetupStatus] = useState<SetupStatus | null>(null)
  const [setupNotice, setSetupNotice] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const { begin: beginSetup } = useLatestRequest()

  const navigate = useCallback((nextPath: string, replace = false) => {
    navigation.navigate(nextPath, replace)
  }, [navigation])

  const replaceQuery = useCallback((patch: Record<string, string | null | undefined>) => {
    navigation.replaceQuery(patch)
  }, [navigation])

  const handlePortfolioStockChange = useCallback((nextKey: string) => {
    setPortfolioStockKey(nextKey)
    navigation.persistPortfolioStock(nextKey)
  }, [navigation])

  useEffect(() => {
    document.title = location.pathname === "/setup" ? messages.setup.ui.pageTitle : messages.common.ui.pageTitle
    document.documentElement.lang = locale
  }, [locale, location.pathname, messages.common.ui.pageTitle, messages.setup.ui.pageTitle])

  useEffect(() => {
    setupErrorRef.current = messages.common.application_error_unknown.detail
  }, [messages.common.application_error_unknown.detail])

  const checkSetup = useCallback(async () => {
    const request = beginSetup()
    setLoading(true)
    setError(null)
    try {
      const status = await getSetupStatus(request.signal)
      if (!request.isCurrent()) return
      setSetupStatus(status)
      const nextPath = navigation.read().pathname
      const redirectPath = resolveSetupPath(status.isComplete, nextPath)
      if (redirectPath) navigate(redirectPath, true)
    } catch (statusError) {
      if (request.isCurrent() && !isRequestAborted(statusError, request.signal)) {
        setError(getApiErrorMessage(statusError, setupErrorRef.current, messages.common.ui.errors))
        navigate("/error", true)
      }
    } finally {
      if (request.isCurrent()) setLoading(false)
    }
  }, [beginSetup, messages.common.ui.errors, navigate, navigation])

  useEffect(() => {
    const timeoutId = window.setTimeout(() => { void checkSetup() }, 0)
    return () => window.clearTimeout(timeoutId)
  }, [checkSetup])

  useEffect(() => {
    if (setupStatus?.isComplete) {
      const redirectPath = resolveSetupPath(true, location.pathname)
      if (redirectPath) navigate(redirectPath, true)
    }
  }, [location.pathname, navigate, setupStatus?.isComplete])

  function renderPage() {
    if (location.pathname === "/setup") {
      return <SetupPage disableEntryAnimation onComplete={(result: SetupResult) => {
        setSetupStatus({ isComplete: true, missingRequirements: [] })
        setSetupNotice(result.stockDataSyncScheduled ? messages.common.ui.states.setupCompleteSync : messages.common.ui.states.setupCompleteDeferred)
        navigate("/overview")
      }} />
    }
    if (location.pathname === "/" || location.pathname === "/overview") return <RecommendationsPage onNavigate={navigate} notice={setupNotice} />
    if (location.pathname === "/404") return <NotFoundPage onNavigate={navigate} />
    if (location.pathname === "/error") return <ApplicationErrorPage message={messages.common.application_error_unknown.detail} onRetry={() => void checkSetup()} onNavigate={navigate} />
    if (location.pathname === "/stocks") return <StocksPage onNavigate={navigate} />
    if (location.pathname === "/budget") return <BudgetPage onNavigate={navigate} />
    if (location.pathname === "/portfolio") {
      return <PortfolioPage
        onNavigate={navigate}
        onReplaceQuery={replaceQuery}
        selectedStockKey={readPortfolioStockKey(location, portfolioStockKey)}
        initialDirection={readPortfolioDirection(location)}
        onSelectedStockKeyChange={handlePortfolioStockChange}
      />
    }
    if (location.pathname === "/settings") return <SettingsPage onNavigate={navigate} onReplaceQuery={replaceQuery} initialStockKey={readSettingsStockKey(location)} />
    return <NotFoundPage onNavigate={navigate} />
  }

  const isSetupRoute = location.pathname === "/setup"
  const setupLoadingPage = <SetupPageLoading label={messages.common.ui.states.preparingSetup} />
  const page = loading
    ? isSetupRoute
      ? setupLoadingPage
      : <StatusPageSkeleton label={messages.common.ui.states.connecting} onNavigate={navigate} />
    : error
      ? <ApplicationErrorPage
        message={error}
        onRetry={() => void checkSetup()}
        onNavigate={(nextPath) => { navigate(nextPath); if (nextPath === "/overview") void checkSetup() }}
      />
      : setupStatus?.isComplete || location.pathname === "/setup"
        ? <RouteErrorBoundary
          resetKey={`${location.pathname}${location.search.toString()}${location.hash}`}
          fallback={<ApplicationErrorPage message={messages.common.application_error_unknown.detail} onRetry={() => window.location.reload()} onNavigate={navigate} />}
        >
          <Suspense fallback={isSetupRoute ? setupLoadingPage : <StatusPageSkeleton label={messages.common.ui.states.preparingSetup} onNavigate={navigate} />}>{renderPage()}</Suspense>
        </RouteErrorBoundary>
        : <StatusPageSkeleton label={messages.common.ui.states.preparingSetup} onNavigate={navigate} />

  return (
    <ThemeProvider>
      {page}
      <ScrollToTop />
    </ThemeProvider>
  )
}
