import * as React from "react"

import type { Locale } from "@/shared/i18n/i18n"
import type { Theme } from "@/app/providers/ThemeContext"

export type PreferencesSyncContextValue = {
  persist: (locale: Locale, theme: Theme) => Promise<void>
}

export const PreferencesSyncContext = React.createContext<PreferencesSyncContextValue | undefined>(undefined)

export function usePreferencesSync() {
  const context = React.useContext(PreferencesSyncContext)
  if (!context) {
    throw new Error("usePreferencesSync must be used within a PreferencesSyncProvider")
  }

  return context
}
