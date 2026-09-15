import { Bot, LineChart, Sparkles } from "lucide-react"
import { useState } from "react"

import { PageFrame } from "@/components/layout/PageFrame"
import { Alert, AlertDescription } from "@/components/ui/Alert"
import { getInitialization } from "@/onboarding/initialization.api"
import { isRequestAborted, useLatestRequest } from "@/shared/hooks/useLatestRequest"
import { useLocale } from "@/shared/i18n/i18n"
import type { InitializationStatusResponse } from "@/shared/http/api-types"
import { Inference } from "@/settings/Inference"
import { StockDataProviders } from "@/settings/StockDataProviders"

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

  function onTabKeyDown(event: React.KeyboardEvent<HTMLDivElement>) {
    const tab = event.key === "ArrowRight" || event.key === "End" ? "ai" : event.key === "ArrowLeft" || event.key === "Home" ? "stock" : null
    if (!tab) return
    event.preventDefault()
    selectTab(tab)
    const index = tab === "stock" ? 0 : 1
    event.currentTarget.querySelectorAll<HTMLButtonElement>('[role="tab"]')[index]?.focus()
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
      <div className="settings-workspace-tabs" role="tablist" aria-label={copy.tabsLabel} onKeyDown={onTabKeyDown}>
        <button type="button" role="tab" id="settings-stock-tab" aria-controls="settings-stock-panel" aria-selected={activeTab === "stock"} tabIndex={activeTab === "stock" ? 0 : -1} className={activeTab === "stock" ? "settings-workspace-tab is-active" : "settings-workspace-tab"} onClick={() => selectTab("stock")}><LineChart size={18} aria-hidden="true" /><span>{copy.stockTab}</span><small>{statusLabel(stockStatus)}</small></button>
        <button type="button" role="tab" id="settings-ai-tab" aria-controls="settings-ai-panel" aria-selected={activeTab === "ai"} tabIndex={activeTab === "ai" ? 0 : -1} className={activeTab === "ai" ? "settings-workspace-tab is-active" : "settings-workspace-tab"} onClick={() => selectTab("ai")}><Bot size={18} aria-hidden="true" /><span>{copy.aiTab}</span><small>{statusLabel(aiStatus)}</small></button>
      </div>
      {(visited.stock || activeTab === "stock") && <div id="settings-stock-panel" role="tabpanel" aria-labelledby="settings-stock-tab" hidden={activeTab !== "stock"}><StockDataProviders onUpdated={refreshStatus} /></div>}
      {(visited.ai || activeTab === "ai") && <div id="settings-ai-panel" role="tabpanel" aria-labelledby="settings-ai-tab" hidden={activeTab !== "ai"}><Inference onUpdated={refreshStatus} /></div>}
    </section>
  </PageFrame>
}
