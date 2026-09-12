import { ArrowUpRight, Bot, Languages, LineChart, Sparkles } from "lucide-react"
import { useEffect, useState } from "react"

import { EmptyState, ErrorState } from "@/components/feedback/AsyncState"
import { PageFrame } from "@/components/layout/PageFrame"
import { PageTitle } from "@/components/layout/PageHeading"
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/Alert"
import { Badge } from "@/components/ui/Badge"
import { Button } from "@/components/ui/Button"
import { Card, CardContent, CardHeader } from "@/components/ui/Card"
import { getApiErrorMessage } from "@/shared/http/api-errors"
import { isRequestAborted, useLatestRequest } from "@/shared/hooks/useLatestRequest"
import { useLocale } from "@/shared/i18n/i18n"
import type { InitializationStatusResponse } from "@/shared/http/api-types"
import { getInitialization } from "@/onboarding/initialization.api"

export function SettingsOverviewPage({ onNavigate }: { onNavigate: (path: string) => void }) {
  const { messages } = useLocale()
  const copy = messages.settings.ui.overview
  const [status, setStatus] = useState<InitializationStatusResponse | null>(null)
  const [error, setError] = useState<string | null>(null)
  const { begin } = useLatestRequest()

  useEffect(() => {
    const request = begin()
    void getInitialization(request.signal)
      .then((result) => { if (request.isCurrent()) setStatus(result) })
      .catch((loadError) => { if (request.isCurrent() && !isRequestAborted(loadError, request.signal)) setError(getApiErrorMessage(loadError, messages.common.ui.errors.requestFailed, messages.common.ui.errors)) })
  }, [begin, messages.common.ui.errors])

  if (error) return <PageFrame currentPath="/settings" onNavigate={onNavigate}><ErrorState message={error} onRetry={() => window.location.reload()} /></PageFrame>
  if (!status) return <PageFrame currentPath="/settings" onNavigate={onNavigate}><EmptyState title={messages.common.ui.states.readingData} description={copy.description} /></PageFrame>

  const statusLabel = (value: string) => value === "currently-unavailable" ? copy.statusUnavailable : value === "unconfigured" ? copy.statusUnconfigured : copy.statusConfigured
  const preferencesLabel = status.preferences
    ? `${status.preferences.languageCode === "zh-CN" ? messages.common.ui.language.zhCN : messages.common.ui.language.enUS} · ${status.preferences.themeCode === "light" ? messages.common.ui.theme.light : status.preferences.themeCode === "dark" ? messages.common.ui.theme.dark : status.preferences.themeCode === "system" ? messages.common.ui.theme.system : status.preferences.themeCode}`
    : copy.statusUnconfigured
  return <PageFrame currentPath="/settings" onNavigate={onNavigate} contentClassName="settings-overview-wrap">
    <PageTitle eyebrow={copy.eyebrow} title={copy.title} description={copy.description} />
    {!status.isComplete && <Alert variant="attention" className="settings-overview-notice"><Sparkles size={17} aria-hidden="true" /><div><AlertTitle>{messages.onboarding.ui.calloutTitle}</AlertTitle><AlertDescription>{messages.onboarding.ui.calloutDescription}</AlertDescription></div></Alert>}
    <div className="settings-overview-grid">
      <OverviewCard icon={<Languages size={19} />} title={copy.preferencesTitle} description={copy.preferencesDescription} badge={preferencesLabel} onOpen={() => onNavigate("/settings/preferences")} label={copy.open} />
      <OverviewCard icon={<LineChart size={19} />} title={copy.stockTitle} description={copy.stockDescription} badge={statusLabel(status.limitations.find((item) => item.capabilityCode === "market")?.statusCode ?? "unconfigured")} onOpen={() => onNavigate("/settings/stock-data-providers")} label={copy.open} />
      <OverviewCard icon={<Bot size={19} />} title={copy.inferenceTitle} description={copy.inferenceDescription} badge={statusLabel(status.limitations.find((item) => item.capabilityCode === "chat")?.statusCode ?? "unconfigured")} onOpen={() => onNavigate("/settings/inference")} label={copy.open} />
    </div>
    <Card className="settings-limitations-card"><CardHeader><h2>{copy.statusUnconfigured}</h2></CardHeader><CardContent className="settings-limitations-list">{status.limitations.map((limitation) => <div className="settings-limitation" key={limitation.capabilityCode}><span>{copy.capabilities[limitation.capabilityCode as keyof typeof copy.capabilities] ?? limitation.capabilityCode}</span><Badge variant={limitation.statusCode === "currently-unavailable" ? "destructive" : limitation.statusCode === "unconfigured" ? "outline" : "accent"}>{statusLabel(limitation.statusCode)}</Badge></div>)}</CardContent></Card>
  </PageFrame>
}

function OverviewCard({ icon, title, description, badge, onOpen, label }: { icon: React.ReactNode; title: string; description: string; badge: string; onOpen: () => void; label: string }) {
  return <Card className="settings-overview-card"><CardHeader><span className="settings-overview-icon">{icon}</span><Badge variant="outline">{badge}</Badge></CardHeader><CardContent><h2>{title}</h2><p>{description}</p><Button variant="ghost" onClick={onOpen}>{label}<ArrowUpRight data-icon="inline-end" /></Button></CardContent></Card>
}
