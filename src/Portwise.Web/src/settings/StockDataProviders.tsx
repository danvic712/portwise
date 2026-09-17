import { Database, KeyRound, Route, Save, ShieldCheck } from "lucide-react"
import { useCallback, useEffect, useMemo, useRef, useState } from "react"

import { useActionFeedback } from "@/app/providers/ActionFeedbackContext"
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/Alert"
import { Badge } from "@/components/ui/Badge"
import { Button } from "@/components/ui/Button"
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/Card"
import { Field, FieldContent, FieldDescription, FieldGroup, FieldLabel, FieldLegend, FieldSet } from "@/components/ui/Field"
import { Input } from "@/components/ui/Input"
import { Select, SelectContent, SelectGroup, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/Select"
import { getApiErrorMessage } from "@/shared/http/api-errors"
import { isRequestAborted, useLatestRequest } from "@/shared/hooks/useLatestRequest"
import { useLocale } from "@/shared/i18n/i18n"
import type { StockDataProvidersResponse, StockDataRoutesResponse } from "@/shared/http/api-types"
import { SettingsFormSkeleton } from "@/settings/SettingsSkeleton"
import { createStockDataProvider, getStockDataProviders, getStockDataRoutes, updateStockDataProvider, updateStockDataRoutes, verifyStockDataProvider } from "@/settings/stock-data-providers.api"

export function StockDataProviders({ onUpdated }: { onUpdated: () => void }) {
  const { messages } = useLocale()
  const copy = messages.stockDataProviders.ui
  const [data, setData] = useState<StockDataProvidersResponse | null>(null)
  const [routes, setRoutes] = useState<StockDataRoutesResponse | null>(null)
  const [selectedDefinitionId, setSelectedDefinitionId] = useState<string | null>(null)
  const [key, setKey] = useState("")
  const [selected, setSelected] = useState<Record<string, string | null>>({})
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const feedbackRef = useRef<HTMLDivElement>(null)
  const { begin } = useLatestRequest()
  const { showFeedback } = useActionFeedback()

  useEffect(() => {
    if (error) feedbackRef.current?.scrollIntoView({ behavior: "smooth", block: "start" })
  }, [error])

  const load = useCallback(() => {
    const request = begin()
    setError(null)
    setLoading(true)
    void Promise.all([getStockDataProviders(request.signal), getStockDataRoutes(request.signal)])
      .then(([providers, routeResponse]) => {
        if (!request.isCurrent()) return
        setData(providers)
        setRoutes(routeResponse)
        setSelectedDefinitionId(providers.providers[0]?.providerDefinitionId ?? providers.definitions.find((item) => item.isEnabled)?.id ?? null)
        setSelected(Object.fromEntries(routeResponse.routes.map((route) => [route.capabilityCode, route.providerId])))
        setLoading(false)
      })
      .catch((loadError) => {
        if (request.isCurrent() && !isRequestAborted(loadError, request.signal)) {
          setError(getApiErrorMessage(loadError, copy.error, messages.common.ui.errors))
          setLoading(false)
        }
      })
  }, [begin, copy.error, messages.common.ui.errors])

  useEffect(() => {
    const timeoutId = window.setTimeout(load, 0)
    return () => window.clearTimeout(timeoutId)
  }, [load])

  const definition = useMemo(() => data?.definitions.find((item) => item.id === selectedDefinitionId), [data, selectedDefinitionId])
  const provider = data?.providers.find((item) => item.providerDefinitionId === selectedDefinitionId)
  const statusLabel = (code: string) => code === "configured-unverified" ? copy.configuredUnverified : code === "recently-verified" ? copy.recentlyVerified : code === "currently-unavailable" ? copy.currentlyUnavailable : copy.unconfigured
  const statusVariant = (code: string) => code === "currently-unavailable" ? "destructive" as const : code === "unconfigured" ? "outline" as const : code === "configured-unverified" ? "attention" as const : "accent" as const

  async function saveProvider() {
    if (!definition || !definition.isEnabled) return
    setBusy(true); setError(null)
    try {
      const result = provider
        ? await updateStockDataProvider(provider.id, { name: provider.name, credentials: { action: key.trim() ? "replace" : "keep", value: key.trim() || null }, expectedRevision: provider.revision })
        : await createStockDataProvider({ providerDefinitionId: definition.id, name: definition.displayName, credentials: { action: key.trim() ? "replace" : "clear", value: key.trim() || null } })
      setData((current) => current ? { ...current, providers: current.providers.some((item) => item.id === result.id) ? current.providers.map((item) => item.id === result.id ? result : item) : [...current.providers, result] } : current)
      setKey("")
      showFeedback({ message: copy.providerSaved })
      onUpdated()
    } catch (saveError) {
      setError(getApiErrorMessage(saveError, copy.error, messages.common.ui.errors))
    } finally { setBusy(false) }
  }

  async function saveRoutes() {
    if (!routes) return
    setBusy(true); setError(null)
    try {
      const result = await updateStockDataRoutes({ routes: routes.routes.map((route) => ({ capabilityCode: route.capabilityCode, providerId: selected[route.capabilityCode] ?? null, expectedRevision: route.revision })) })
      setRoutes(result)
      showFeedback({ message: copy.routesSaved })
      onUpdated()
    } catch (saveError) {
      setError(getApiErrorMessage(saveError, copy.error, messages.common.ui.errors))
    } finally { setBusy(false) }
  }

  async function verify() {
    if (!provider) return
    setBusy(true); setError(null)
    try {
      const result = await verifyStockDataProvider(provider.id, { expectedRevision: provider.revision })
      setData((current) => current ? { ...current, providers: current.providers.map((item) => item.id === result.provider.id ? result.provider : item) } : current)
      const statusCode = result.provider.runtimeStatusCode
      showFeedback({ message: statusCode === "recently-verified" ? copy.verified : statusCode === "unconfigured" ? copy.verificationNeedsConfiguration : statusCode === "currently-unavailable" ? copy.verificationUnavailable : copy.verificationNeedsRoute, tone: statusCode === "recently-verified" ? "success" : "attention" })
      onUpdated()
    } catch (verifyError) {
      setError(getApiErrorMessage(verifyError, copy.error, messages.common.ui.errors))
    } finally { setBusy(false) }
  }

  return <div className="settings-stock-tab-content">
    {error && <Alert ref={feedbackRef} variant="destructive" className="settings-subpage-feedback settings-subpage-feedback-error"><AlertTitle>{copy.feedbackErrorTitle}</AlertTitle><AlertDescription>{error}</AlertDescription></Alert>}

    {loading && !data && !routes ? <SettingsFormSkeleton kind="stock" label={copy.loading} /> : !data || !routes ? <div className="settings-load-error"><p>{copy.loadFailed}</p><Button variant="outline" onClick={load}>{copy.retry}</Button></div> : <div className="settings-subpage-sections">
      <div className="settings-stock-tab-intro">
        <span className="settings-stock-tab-intro-mark"><Database size={18} aria-hidden="true" /></span>
        <div className="settings-stock-tab-intro-copy">
          <p className="settings-stock-tab-kicker"><span aria-hidden="true" />{copy.eyebrow}<span aria-hidden="true" /></p>
          <h2>{copy.title}</h2>
          <span>{copy.description}</span>
        </div>
      </div>

      <div className="settings-stock-capability-rail" aria-label={copy.routesTitle}>
        {routes.routes.map((route) => {
          const label = messages.settings.ui.overview.capabilities[route.capabilityCode as keyof typeof messages.settings.ui.overview.capabilities] ?? route.capabilityCode
          const routeProvider = data.providers.find((item) => item.id === selected[route.capabilityCode])
          return <div className="settings-stock-capability-item" key={route.capabilityCode}>
            <span className="settings-stock-capability-mark"><span className="settings-stock-capability-dot" /></span>
            <div>
              <strong>{label}</strong>
              <span>{routeProvider?.name ?? copy.unconfigured}</span>
            </div>
            <Badge variant={statusVariant(route.runtimeStatusCode)}>{statusLabel(route.runtimeStatusCode)}</Badge>
          </div>
        })}
      </div>

      <div className="settings-stock-config-grid">
        <Card className="settings-stock-source-card">
          <CardHeader>
            <div className="settings-stock-card-heading">
              <span className="settings-stock-card-icon"><Database size={17} aria-hidden="true" /></span>
              <div><p className="settings-form-kicker">{copy.connectionEyebrow}</p><CardTitle>{copy.providerTitle}</CardTitle><CardDescription>{copy.providerHint}</CardDescription></div>
            </div>
            <Badge variant={statusVariant(provider?.runtimeStatusCode ?? "unconfigured")}>{statusLabel(provider?.runtimeStatusCode ?? "unconfigured")}</Badge>
          </CardHeader>
          <CardContent>
            <FieldGroup className="settings-stock-fields">
              <Field>
                <FieldLabel htmlFor="stock-provider-source">{copy.providerLabel}</FieldLabel>
                <Select value={selectedDefinitionId ?? undefined} onValueChange={(value) => { setSelectedDefinitionId(value); setKey(""); setError(null) }}>
                  <SelectTrigger id="stock-provider-source" aria-label={copy.providerLabel}><SelectValue>{definition?.displayName ?? copy.providerPlaceholder}</SelectValue></SelectTrigger>
                  <SelectContent><SelectGroup>{data.definitions.map((item) => <SelectItem value={item.id} key={item.id} disabled={!item.isEnabled && !data.providers.some((providerItem) => providerItem.providerDefinitionId === item.id)}>{item.displayName}</SelectItem>)}</SelectGroup></SelectContent>
                </Select>
              </Field>
              <Field>
                <FieldLabel htmlFor="stock-provider-key"><KeyRound size={14} aria-hidden="true" />{copy.keyLabel}</FieldLabel>
                <Input id="stock-provider-key" type="password" value={key} placeholder={provider?.secretState.stateCode === "configured" ? copy.keyConfigured : copy.keyPlaceholder} onChange={(event) => setKey(event.target.value)} autoComplete="new-password" />
              </Field>
            </FieldGroup>
          </CardContent>
          <CardFooter>
            <Button onClick={() => void saveProvider()} disabled={busy || !definition?.isEnabled}><Save data-icon="inline-start" />{copy.save}</Button>
            {provider && <Button variant="outline" onClick={() => void verify()} disabled={busy}><ShieldCheck data-icon="inline-start" />{copy.verify}</Button>}
          </CardFooter>
        </Card>

        <Card className="settings-stock-routes-card">
          <CardHeader>
            <div className="settings-stock-card-heading">
              <span className="settings-stock-card-icon settings-stock-card-icon-route"><Route size={17} aria-hidden="true" /></span>
              <div><p className="settings-form-kicker">{copy.routesEyebrow}</p><CardTitle>{copy.routesTitle}</CardTitle><CardDescription>{copy.routeHint}</CardDescription></div>
            </div>
            <span className="settings-stock-route-count">{routes.routes.length}</span>
          </CardHeader>
          <CardContent>
            <FieldSet className="settings-stock-route-fields">
              <FieldLegend className="sr-only">{copy.routesTitle}</FieldLegend>
              {routes.routes.map((route) => {
                const label = messages.settings.ui.overview.capabilities[route.capabilityCode as keyof typeof messages.settings.ui.overview.capabilities] ?? route.capabilityCode
                const routeProvider = data.providers.find((item) => item.id === selected[route.capabilityCode])
                return <Field orientation="vertical" className="settings-stock-route-field" key={route.capabilityCode}>
                  <FieldContent>
                    <FieldLabel htmlFor={`stock-route-${route.capabilityCode}`}>{label}</FieldLabel>
                    <FieldDescription><Badge variant={statusVariant(route.runtimeStatusCode)}>{statusLabel(route.runtimeStatusCode)}</Badge></FieldDescription>
                  </FieldContent>
                  <Select value={selected[route.capabilityCode] ?? "none"} onValueChange={(value) => setSelected((current) => ({ ...current, [route.capabilityCode]: value === "none" || value === null ? null : value }))}>
                    <SelectTrigger id={`stock-route-${route.capabilityCode}`} aria-label={`${label} · ${copy.routesTitle}`}><SelectValue>{routeProvider?.name ?? copy.unconfigured}</SelectValue></SelectTrigger>
                    <SelectContent><SelectGroup><SelectItem value="none">{copy.unconfigured}</SelectItem>{data.providers.map((item) => <SelectItem value={item.id} key={item.id}>{item.name}</SelectItem>)}</SelectGroup></SelectContent>
                  </Select>
                </Field>
              })}
            </FieldSet>
          </CardContent>
          <CardFooter><Button onClick={() => void saveRoutes()} disabled={busy || !routes}><Save data-icon="inline-start" />{copy.saveRoutes}</Button></CardFooter>
        </Card>
      </div>
    </div>}
  </div>
}
