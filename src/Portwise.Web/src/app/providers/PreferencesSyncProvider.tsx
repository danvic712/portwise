import * as React from "react"

import { getPreferences, updatePreferences } from "@/settings/preferences.api"
import { PreferencesSyncContext } from "@/app/providers/PreferencesSyncContext"
import type { Locale } from "@/shared/i18n/i18n"
import type { Theme } from "@/app/providers/ThemeContext"

export function PreferencesSyncProvider({ children }: { children: React.ReactNode }) {
  const pendingSave = React.useRef<Promise<void>>(Promise.resolve())

  const persist = React.useCallback((locale: Locale, theme: Theme) => {
    const save = pendingSave.current
      .catch(() => undefined)
      .then(async () => {
        const current = await getPreferences()
        await updatePreferences({
          languageCode: locale,
          themeCode: theme,
          expectedRevision: current.revision,
        })
      })
    pendingSave.current = save
    return save
  }, [])

  const value = React.useMemo(() => ({ persist }), [persist])
  return <PreferencesSyncContext.Provider value={value}>{children}</PreferencesSyncContext.Provider>
}
