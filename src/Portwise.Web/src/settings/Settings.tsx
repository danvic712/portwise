import { Bot, LineChart, Sparkles } from "lucide-react"
import { useState } from "react"

import { PageFrame } from "@/components/layout/PageFrame"
import { Alert, AlertDescription } from "@/components/ui/Alert"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { getInitialization } from "@/onboarding/initialization.api"
import { isRequestAborted, useLatestRequest } from "@/shared/hooks/useLatestRequest"
import { useLocale } from "@/shared/i18n/i18n"
import type { InitializationStatusResponse } from "@/shared/http/api-types"
import { Inference } from "@/settings/Inference"
import { StockDataProviders } from "@/settings/StockDataProviders"
import { StockDataSyncSchedule } from "@/settings/StockDataSyncSchedule"

import "./settings.css"

export type SettingsTab = "stock" | "ai"

type SettingsProps = {
  onNavigate: (path: string) => void
  activeTab: SettingsTab
  onTabChange: (tab: SettingsTab) => void
  initialStatus: InitializationStatusResponse | null
}

function capabilityStatus(status: InitializationStatusResponse | null, codes: string[]) {
  const items = codes.map((code) => status?.limitations.find((item) => item.capabilityCode === code)?.statusCode ?? "unconfigured")
  if (items.some((code) => code === "currently-unavailable")) return "currently-unavailable"
  if (items.some((code) => code === "unconfigured")) return "unconfigured"
  if (items.every((code) => code === "recently-verified")) return "recently-verified"
  return "configured-unverified"
}

export function Settings({ onNavigate, activeTab, onTabChange, initialStatus }: SettingsProps) {
  const { messages } = useLocale()
  const copy = messages.settings.ui.overview
  const [status, setStatus] = useState(initialStatus)
  const [refreshFailed, setRefreshFailed] = useState(false)
  const [visited, setVisited] = useState({ stock: activeTab === "stock", ai: activeTab === "ai" })
  const { begin } = useLatestRequest()

  function refreshStatus() {
    const request = begin()
    void getInitialization(request.signal)
      .then((result) => {
        if (!request.isCurrent()) return
        setStatus(result)
        setRefreshFailed(false)
      })
      .catch((error) => {
        if (request.isCurrent() && !isRequestAborted(error, request.signal)) setRefreshFailed(true)
      })
  }

  function selectTab(tab: SettingsTab) {
    setVisited((current) => ({ ...current, [tab]: true }))
    onTabChange(tab)
  }

  function handleTabChange(value: string) {
    if (value === "stock" || value === "ai") selectTab(value)
  }

  const stockStatus = capabilityStatus(status, ["profile", "market", "dividend", "financial"])
  const aiStatus = capabilityStatus(status, ["chat", "embedding"])
  const statusLabel = (code: string) => code === "currently-unavailable" ? copy.statusUnavailable : code === "unconfigured" ? copy.statusUnconfigured : code === "recently-verified" ? copy.statusRecentlyVerified : copy.statusConfiguredUnverified
  const hasPending = stockStatus !== "recently-verified" || aiStatus !== "recently-verified"

  return <PageFrame currentPath="/settings" onNavigate={onNavigate} contentClassName="settings-workspace-wrap">
    <header className="settings-workspace-hero">
      <span className="settings-workspace-sticker" aria-hidden="true"><strong>P</strong><small>{copy.sticker}</small></span>
      <div className="settings-workspace-hero-copy">
        <p className="settings-workspace-kicker"><span />{copy.heroEyebrow}<span /></p>
        <h1>{copy.heroTitle}</h1>
        <p>{copy.heroDescription}</p>
      </div>
      <div className="settings-workspace-hero-status"><span className={hasPending ? "settings-status-dot settings-status-dot-pending" : "settings-status-dot"} />{hasPending ? copy.heroPending : copy.heroReady}</div>
    </header>

    {refreshFailed && <Alert variant="attention" className="settings-workspace-notice"><Sparkles size={16} aria-hidden="true" /><AlertDescription>{copy.statusRefreshFailed}</AlertDescription></Alert>}

    <section className="settings-workspace-panel" aria-label={copy.tabsLabel}>
      <Tabs value={activeTab} onValueChange={handleTabChange} className="settings-workspace-tabs-root">
        <TabsList className="settings-workspace-tabs" aria-label={copy.tabsLabel}>
          <TabsTrigger value="stock" className="settings-workspace-tab">
            <span className="settings-workspace-tab-icon"><LineChart size={17} aria-hidden="true" /></span>
            <span className="settings-workspace-tab-label">{copy.stockTab}</span>
            <small><span className="settings-workspace-tab-status-mark" aria-hidden="true" />{statusLabel(stockStatus)}</small>
          </TabsTrigger>
          <TabsTrigger value="ai" className="settings-workspace-tab">
            <span className="settings-workspace-tab-icon"><Bot size={17} aria-hidden="true" /></span>
            <span className="settings-workspace-tab-label">{copy.aiTab}</span>
            <small><span className="settings-workspace-tab-status-mark" aria-hidden="true" />{statusLabel(aiStatus)}</small>
          </TabsTrigger>
        </TabsList>
        <TabsContent value="stock" className="settings-workspace-tab-panel">{(visited.stock || activeTab === "stock") && <><StockDataProviders onUpdated={refreshStatus} /><StockDataSyncSchedule onUpdated={refreshStatus} /></>}</TabsContent>
        <TabsContent value="ai" className="settings-workspace-tab-panel">{(visited.ai || activeTab === "ai") && <Inference onUpdated={refreshStatus} />}</TabsContent>
      </Tabs>
    </section>
  </PageFrame>
}
