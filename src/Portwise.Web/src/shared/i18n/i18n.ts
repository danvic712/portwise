import { createContext, createElement, useCallback, useContext, useMemo, useState, type ReactNode } from "react"

import enCommon from "../../../../../locales/en-US/common.json"
import enBudget from "../../../../../locales/en-US/budget.json"
import enDividendStrategy from "../../../../../locales/en-US/dividend-strategy.json"
import enPortfolio from "../../../../../locales/en-US/portfolio.json"
import enOnboarding from "../../../../../locales/en-US/initialization.json"
import enStocks from "../../../../../locales/en-US/stocks.json"
import enSettings from "../../../../../locales/en-US/settings.json"
import enStockDataProviders from "../../../../../locales/en-US/stock-data-providers.json"
import enInference from "../../../../../locales/en-US/inference.json"
import zhCommon from "../../../../../locales/zh-CN/common.json"
import zhBudget from "../../../../../locales/zh-CN/budget.json"
import zhDividendStrategy from "../../../../../locales/zh-CN/dividend-strategy.json"
import zhPortfolio from "../../../../../locales/zh-CN/portfolio.json"
import zhOnboarding from "../../../../../locales/zh-CN/initialization.json"
import zhStocks from "../../../../../locales/zh-CN/stocks.json"
import zhSettings from "../../../../../locales/zh-CN/settings.json"
import zhStockDataProviders from "../../../../../locales/zh-CN/stock-data-providers.json"
import zhInference from "../../../../../locales/zh-CN/inference.json"

export type Locale = "zh-CN" | "en-US"

type LocaleMessages = {
  common: typeof zhCommon
  budget: {
    ui: typeof zhBudget.ui
  }
  dividendStrategy: {
    ui: typeof zhDividendStrategy.ui
  }
  portfolio: {
    ui: typeof zhPortfolio.ui
  }
  onboarding: {
    ui: typeof zhOnboarding.ui
  }
  stocks: {
    ui: typeof zhStocks.ui
  }
  settings: {
    ui: typeof zhSettings.ui
  }
  stockDataProviders: {
    ui: typeof zhStockDataProviders.ui
  }
  inference: {
    ui: typeof zhInference.ui
  }
}

const catalogs: Record<Locale, LocaleMessages> = {
  "zh-CN": {
    common: zhCommon,
    budget: { ui: zhBudget.ui },
    dividendStrategy: { ui: zhDividendStrategy.ui },
    portfolio: { ui: zhPortfolio.ui },
    onboarding: { ui: zhOnboarding.ui },
    stocks: { ui: zhStocks.ui },
    settings: { ui: zhSettings.ui },
    stockDataProviders: { ui: zhStockDataProviders.ui },
    inference: { ui: zhInference.ui },
  },
  "en-US": {
    common: enCommon,
    budget: { ui: enBudget.ui },
    dividendStrategy: { ui: enDividendStrategy.ui },
    portfolio: { ui: enPortfolio.ui },
    onboarding: { ui: enOnboarding.ui },
    stocks: { ui: enStocks.ui },
    settings: { ui: enSettings.ui },
    stockDataProviders: { ui: enStockDataProviders.ui },
    inference: { ui: enInference.ui },
  },
}

export type OverviewCopy = LocaleMessages["dividendStrategy"]["ui"]["overview"]

type LocaleContextValue = {
  locale: Locale
  messages: LocaleMessages
  setLocale: (locale: Locale) => void
}

const LocaleContext = createContext<LocaleContextValue | undefined>(undefined)

function isLocale(value: string | null): value is Locale {
  return value === "zh-CN" || value === "en-US"
}

function getInitialLocale(): Locale {
  const storedLocale = typeof window !== "undefined" ? window.localStorage.getItem("portwise-locale") : null
  return isLocale(storedLocale) ? storedLocale : "zh-CN"
}

export function LocaleProvider({ children }: { children: ReactNode }) {
  const [locale, setLocaleState] = useState<Locale>(getInitialLocale)
  const messages = catalogs[locale]

  const setLocale = useCallback((nextLocale: Locale) => {
    window.localStorage.setItem("portwise-locale", nextLocale)
    setLocaleState(nextLocale)
  }, [])

  const value = useMemo(() => ({ locale, messages, setLocale }), [locale, messages, setLocale])

  return createElement(LocaleContext.Provider, { value }, children)
}

export function useLocale() {
  const context = useContext(LocaleContext)
  if (!context) {
    throw new Error("useLocale must be used within a LocaleProvider")
  }

  return context
}

export function interpolate(template: string, values: Record<string, string | number>) {
  return template.replace(/\{(\w+)\}/g, (_, key: string) => String(values[key] ?? `{${key}}`))
}
