import { lazy, Suspense, useCallback, useEffect, useRef, useState, useSyncExternalStore } from "react"

import { ApplicationErrorPage } from "@/components/feedback/ApplicationErrorPage"
import { NotFoundPage } from "@/components/feedback/NotFoundPage"
import { RouteErrorBoundary } from "@/components/feedback/RouteErrorBoundary"
import { ScrollToTop } from "@/components/layout/ScrollToTop"
import { StatusPageSkeleton } from "@/components/feedback/StatusPageShell"
import { useTheme } from "@/app/providers/ThemeContext"
import { getApiErrorMessage } from "@/shared/http/api-errors"
import { useLocale } from "@/shared/i18n/i18n"
import { isRequestAborted, useLatestRequest } from "@/shared/hooks/useLatestRequest"
import { getInitialization } from "@/onboarding/initialization.api"
import type { InitializationStatusResponse } from "@/shared/http/api-types"
import {
  createBrowserNavigation,
  readPortfolioDirection,
  readPortfolioStockKey,
  readSettingsStockKey,
  resolveInitializationPath,
} from "@/app/routing/navigation"

const OnboardingPage = lazy(async () => ({ default: (await import("@/onboarding/OnboardingPage")).OnboardingPage }))
const RecommendationsPage = lazy(async () => ({ default: (await import("@/recommendations/RecommendationsPage")).RecommendationsPage }))
const StocksPage = lazy(async () => ({ default: (await import("@/stocks/StocksPage")).StocksPage }))
const BudgetPage = lazy(async () => ({ default: (await import("@/budget/BudgetPage")).BudgetPage }))
const PortfolioPage = lazy(async () => ({ default: (await import("@/portfolio/PortfolioPage")).PortfolioPage }))
const SettingsOverviewPage = lazy(async () => ({ default: (await import("@/settings/SettingsOverviewPage")).SettingsOverviewPage }))
const PreferencesPage = lazy(async () => ({ default: (await import("@/settings/PreferencesPage")).PreferencesPage }))
const StockDataProvidersPage = lazy(async () => ({ default: (await import("@/settings/StockDataProvidersPage")).StockDataProvidersPage }))
const InferencePage = lazy(async () => ({ default: (await import("@/settings/InferencePage")).InferencePage }))
const StrategyPage = lazy(async () => ({ default: (await import("@/strategy/StrategyPage")).StrategyPage }))

export function ApplicationShell() {
  const { locale, setLocale, messages } = useLocale()
  const { setTheme } = useTheme()
  const [navigation] = useState(() => createBrowserNavigation())
  const location = useSyncExternalStore(navigation.subscribe, navigation.read, navigation.read)
  const initializationErrorRef = useRef(messages.common.application_error_unknown.detail)
  const [portfolioStockKey, setPortfolioStockKey] = useState(() =>
    readPortfolioStockKey(navigation.read(), navigation.readPersistedPortfolioStock()),
  )
  const [initializationStatus, setInitializationStatus] = useState<InitializationStatusResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const { begin: beginInitialization } = useLatestRequest()

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
    document.title = location.pathname === "/onboarding" ? messages.onboarding.ui.pageTitle : messages.common.ui.pageTitle
    document.documentElement.lang = locale
  }, [locale, location.pathname, messages.common.ui.pageTitle, messages.onboarding.ui.pageTitle])

  useEffect(() => {
    initializationErrorRef.current = messages.common.application_error_unknown.detail
  }, [messages.common.application_error_unknown.detail])

  const checkInitialization = useCallback(async () => {
    const request = beginInitialization()
    setLoading(true)
    setError(null)
    try {
      const status = await getInitialization(request.signal)
      if (!request.isCurrent()) return
      setInitializationStatus(status)
      if (status.preferences?.languageCode === "zh-CN" || status.preferences?.languageCode === "en-US") setLocale(status.preferences.languageCode)
      if (status.preferences?.themeCode === "light" || status.preferences?.themeCode === "dark" || status.preferences?.themeCode === "system") setTheme(status.preferences.themeCode)
      const nextPath = navigation.read().pathname
      const redirectPath = resolveInitializationPath(status.isComplete, nextPath)
      if (redirectPath) navigate(redirectPath, true)
    } catch (statusError) {
      if (request.isCurrent() && !isRequestAborted(statusError, request.signal)) {
        setError(getApiErrorMessage(statusError, initializationErrorRef.current, messages.common.ui.errors))
        navigate("/error", true)
      }
    } finally {
      if (request.isCurrent()) setLoading(false)
    }
  }, [beginInitialization, messages.common.ui.errors, navigate, navigation, setLocale, setTheme])

  useEffect(() => {
    const timeoutId = window.setTimeout(() => { void checkInitialization() }, 0)
    return () => window.clearTimeout(timeoutId)
  }, [checkInitialization])

  useEffect(() => {
    if (initializationStatus?.isComplete) {
      const redirectPath = resolveInitializationPath(true, location.pathname)
      if (redirectPath) navigate(redirectPath, true)
    }
  }, [initializationStatus?.isComplete, location.pathname, navigate])

  function renderPage() {
    if (location.pathname === "/onboarding") return <OnboardingPage onNavigate={navigate} onComplete={(result) => setInitializationStatus(result.status)} />
    if (location.pathname === "/" || location.pathname === "/overview") return <RecommendationsPage onNavigate={navigate} />
    if (location.pathname === "/404") return <NotFoundPage onNavigate={navigate} />
    if (location.pathname === "/error") return <ApplicationErrorPage message={messages.common.application_error_unknown.detail} onRetry={() => void checkInitialization()} onNavigate={navigate} />
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
    if (location.pathname === "/settings") return <SettingsOverviewPage onNavigate={navigate} />
    if (location.pathname === "/settings/preferences") return <PreferencesPage onNavigate={navigate} />
    if (location.pathname === "/settings/stock-data-providers") return <StockDataProvidersPage onNavigate={navigate} />
    if (location.pathname === "/settings/inference") return <InferencePage onNavigate={navigate} />
    if (location.pathname === "/strategy") return <StrategyPage onNavigate={navigate} onReplaceQuery={replaceQuery} initialStockKey={readSettingsStockKey(location)} />
    return <NotFoundPage onNavigate={navigate} />
  }

  const isOnboardingRoute = location.pathname === "/onboarding"
  const onboardingLoadingPage = <StatusPageSkeleton label={messages.common.ui.states.preparingSetup} onNavigate={navigate} />
  const page = loading
    ? isOnboardingRoute
      ? onboardingLoadingPage
      : <StatusPageSkeleton label={messages.common.ui.states.connecting} onNavigate={navigate} />
    : error
      ? <ApplicationErrorPage
        message={error}
        onRetry={() => void checkInitialization()}
        onNavigate={(nextPath) => { navigate(nextPath); if (nextPath === "/overview") void checkInitialization() }}
      />
      : initializationStatus?.isComplete || location.pathname === "/onboarding"
        ? <RouteErrorBoundary
          resetKey={`${location.pathname}${location.search.toString()}${location.hash}`}
          fallback={<ApplicationErrorPage message={messages.common.application_error_unknown.detail} onRetry={() => window.location.reload()} onNavigate={navigate} />}
        >
          <Suspense fallback={isOnboardingRoute ? onboardingLoadingPage : <StatusPageSkeleton label={messages.common.ui.states.preparingSetup} onNavigate={navigate} />}>{renderPage()}</Suspense>
        </RouteErrorBoundary>
        : <StatusPageSkeleton label={messages.common.ui.states.preparingSetup} onNavigate={navigate} />

  return (
    <>
      {page}
      <ScrollToTop />
    </>
  )
}
