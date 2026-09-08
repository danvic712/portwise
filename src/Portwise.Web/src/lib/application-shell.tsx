import { lazy, Suspense, useCallback, useEffect, useRef, useState } from "react"

import { ApplicationErrorPage } from "@/components/application-error-page"
import { NotFoundPage } from "@/components/not-found-page"
import { ScrollToTop } from "@/components/scroll-to-top"
import { StatusPageSkeleton } from "@/components/status-page-shell"
import { ThemeProvider } from "@/components/theme-provider"
import { getApiErrorMessage } from "@/lib/api-errors"
import { useLocale } from "@/lib/i18n"
import { getSetupStatus } from "@/features/setup/setup.api"
import type { SetupResult, SetupStatus } from "@/lib/api-types"
import {
  createBrowserNavigation,
  readPortfolioDirection,
  readPortfolioStockKey,
  readSettingsStockKey,
  resolveSetupPath,
  type NavigationLocation,
} from "@/lib/navigation"

const SetupPage = lazy(async () => ({ default: (await import("@/features/setup/SetupPage")).SetupPage }))
const RecommendationsPage = lazy(async () => ({ default: (await import("@/features/recommendations/RecommendationsPage")).RecommendationsPage }))
const StocksPage = lazy(async () => ({ default: (await import("@/features/stocks/StocksPage")).StocksPage }))
const BudgetPage = lazy(async () => ({ default: (await import("@/features/budget/BudgetPage")).BudgetPage }))
const PortfolioPage = lazy(async () => ({ default: (await import("@/features/portfolio/PortfolioPage")).PortfolioPage }))
const SettingsPage = lazy(async () => ({ default: (await import("@/features/settings/SettingsPage")).SettingsPage }))

export function ApplicationShell() {
  const { locale, messages } = useLocale()
  const [navigation] = useState(() => createBrowserNavigation())
  const [location, setLocation] = useState<NavigationLocation>(() => navigation.read())
  const setupErrorRef = useRef(messages.common.application_error_unknown.detail)
  const [portfolioStockKey, setPortfolioStockKey] = useState(() =>
    readPortfolioStockKey(navigation.read(), navigation.readPersistedPortfolioStock()),
  )
  const [setupStatus, setSetupStatus] = useState<SetupStatus | null>(null)
  const [setupNotice, setSetupNotice] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

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
      const nextPath = navigation.read().pathname
      const redirectPath = resolveSetupPath(status.isComplete, nextPath)
      if (redirectPath) navigate(redirectPath, true)
    } catch (statusError) {
      setError(getApiErrorMessage(statusError, setupErrorRef.current, messages.common.ui.errors))
      navigate("/error", true)
    } finally {
      setLoading(false)
    }
  }, [messages.common.ui.errors, navigate, navigation])

  useEffect(() => {
    const timeoutId = window.setTimeout(() => { void checkSetup() }, 0)
    const unsubscribe = navigation.subscribe(setLocation)
    return () => {
      window.clearTimeout(timeoutId)
      unsubscribe()
    }
  }, [checkSetup, navigation])

  useEffect(() => {
    if (setupStatus?.isComplete) {
      const redirectPath = resolveSetupPath(true, location.pathname)
      if (redirectPath) navigate(redirectPath, true)
    }
  }, [location.pathname, navigate, setupStatus?.isComplete])

  function renderPage() {
    if (location.pathname === "/setup") {
      return <SetupPage onComplete={(result: SetupResult) => {
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

  const page = loading
    ? <StatusPageSkeleton label={messages.common.ui.states.connecting} onNavigate={navigate} />
    : error
      ? <ApplicationErrorPage
        message={error}
        onRetry={() => void checkSetup()}
        onNavigate={(nextPath) => { navigate(nextPath); if (nextPath === "/overview") void checkSetup() }}
      />
      : setupStatus?.isComplete || location.pathname === "/setup"
        ? <Suspense fallback={<StatusPageSkeleton label={messages.common.ui.states.preparingSetup} onNavigate={navigate} />}>{renderPage()}</Suspense>
        : <StatusPageSkeleton label={messages.common.ui.states.preparingSetup} onNavigate={navigate} />

  return (
    <ThemeProvider>
      {page}
      <ScrollToTop />
    </ThemeProvider>
  )
}
