import { Check, Save } from "lucide-react"
import { useEffect, useState } from "react"

import { Alert, AlertDescription, AlertTitle } from "@/components/ui/Alert"
import { Button } from "@/components/ui/Button"
import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/Card"
import { Field, FieldGroup, FieldLabel } from "@/components/ui/Field"
import { Select, SelectContent, SelectGroup, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/Select"
import { PageFrame } from "@/components/layout/PageFrame"
import { PageTitle } from "@/components/layout/PageHeading"
import { getApiErrorMessage } from "@/shared/http/api-errors"
import { isRequestAborted, useLatestRequest } from "@/shared/hooks/useLatestRequest"
import { useLocale, type Locale } from "@/shared/i18n/i18n"
import { useTheme, type Theme } from "@/app/providers/ThemeContext"
import { getPreferences, updatePreferences } from "@/settings/preferences.api"

export function PreferencesPage({ onNavigate }: { onNavigate: (path: string) => void }) {
  const { locale, setLocale, messages } = useLocale()
  const { theme, setTheme } = useTheme()
  const [message, setMessage] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [revision, setRevision] = useState(1)
  const { begin } = useLatestRequest()
  const copy = messages.settings.ui.overview
  useEffect(() => {
    const request = begin()
    void getPreferences(request.signal)
      .then((result) => { if (request.isCurrent()) setRevision(result.revision) })
      .catch((loadError) => {
        if (request.isCurrent() && !isRequestAborted(loadError, request.signal)) {
          setError(getApiErrorMessage(loadError, messages.common.ui.errors.requestFailed, messages.common.ui.errors))
        }
      })
  }, [begin, messages.common.ui.errors])
  async function save() {
    setError(null); setMessage(null)
    try {
      const result = await updatePreferences({ languageCode: locale, themeCode: theme, expectedRevision: revision })
      setRevision(result.revision)
      setMessage(copy.saved)
    }
    catch (saveError) { setError(getApiErrorMessage(saveError, messages.common.ui.errors.requestFailed, messages.common.ui.errors)) }
  }
  return <PageFrame currentPath="/settings" onNavigate={onNavigate} contentClassName="settings-subpage-wrap"><PageTitle eyebrow={copy.eyebrow} title={copy.preferencesTitle} description={copy.preferencesDescription} />{error && <Alert variant="destructive"><AlertTitle>{messages.common.ui.states.applicationError.title}</AlertTitle><AlertDescription>{error}</AlertDescription></Alert>}{message && <Alert><Check size={16} aria-hidden="true" /><div><AlertTitle>{copy.saved}</AlertTitle><AlertDescription>{message}</AlertDescription></div></Alert>}<Card className="settings-subpage-card"><CardHeader><h2>{copy.preferencesTitle}</h2></CardHeader><CardContent><FieldGroup className="settings-preferences-fields"><Field><FieldLabel>{messages.common.ui.language.label}</FieldLabel><Select value={locale} onValueChange={(value) => { if (value === "zh-CN" || value === "en-US") setLocale(value as Locale) }}><SelectTrigger><SelectValue>{locale === "zh-CN" ? messages.common.ui.language.zhCN : messages.common.ui.language.enUS}</SelectValue></SelectTrigger><SelectContent><SelectGroup><SelectItem value="zh-CN">{messages.common.ui.language.zhCN}</SelectItem><SelectItem value="en-US">{messages.common.ui.language.enUS}</SelectItem></SelectGroup></SelectContent></Select></Field><Field><FieldLabel>{messages.common.ui.theme.label}</FieldLabel><Select value={theme} onValueChange={(value) => { if (value === "light" || value === "dark" || value === "system") setTheme(value as Theme) }}><SelectTrigger><SelectValue>{messages.common.ui.theme[theme]}</SelectValue></SelectTrigger><SelectContent><SelectGroup><SelectItem value="system">{messages.common.ui.theme.system}</SelectItem><SelectItem value="light">{messages.common.ui.theme.light}</SelectItem><SelectItem value="dark">{messages.common.ui.theme.dark}</SelectItem></SelectGroup></SelectContent></Select></Field></FieldGroup></CardContent><CardFooter><Button onClick={() => void save()}><Save data-icon="inline-start" />{copy.open}</Button></CardFooter></Card></PageFrame>
}
