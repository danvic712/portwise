import { Check, KeyRound, Save, ShieldCheck } from "lucide-react"
import { useEffect, useState } from "react"

import { Alert, AlertDescription, AlertTitle } from "@/components/ui/Alert"
import { Badge } from "@/components/ui/Badge"
import { Button } from "@/components/ui/Button"
import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/Card"
import { Field, FieldDescription, FieldGroup, FieldLabel } from "@/components/ui/Field"
import { Input } from "@/components/ui/Input"
import { Select, SelectContent, SelectGroup, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/Select"
import { PageFrame } from "@/components/layout/PageFrame"
import { PageTitle } from "@/components/layout/PageHeading"
import { getApiErrorMessage } from "@/shared/http/api-errors"
import { isRequestAborted, useLatestRequest } from "@/shared/hooks/useLatestRequest"
import { useLocale } from "@/shared/i18n/i18n"
import type { InferenceProvidersResponse, InferenceRoutesResponse } from "@/shared/http/api-types"
import { getInferenceProviders, getInferenceRoutes, updateInferenceProvider, updateInferenceRoutes, verifyInferenceProvider } from "@/settings/inference.api"
import { getInferenceProviderBaseUrlDraft } from "@/shared/inference/provider-base-url"

type Notice = {
  message: string
  variant: "default" | "attention"
}

export function InferencePage({ onNavigate }: { onNavigate: (path: string) => void }) {
  const { messages } = useLocale()
  const copy = messages.inference.ui
  const [data, setData] = useState<InferenceProvidersResponse | null>(null)
  const [routes, setRoutes] = useState<InferenceRoutesResponse | null>(null)
  const [selectedProviderId, setSelectedProviderId] = useState<string | null>(null)
  const [name, setName] = useState("")
  const [baseUrl, setBaseUrl] = useState("")
  const [key, setKey] = useState("")
  const [models, setModels] = useState<Record<string, string>>({})
  const [routeProviders, setRouteProviders] = useState<Record<string, string>>({})
  const [busy, setBusy] = useState(false)
  const [notice, setNotice] = useState<Notice | null>(null)
  const [error, setError] = useState<string | null>(null)
  const { begin } = useLatestRequest()

  const provider = data?.providers.find((item) => item.id === selectedProviderId) ?? null

  useEffect(() => {
    const request = begin()
    void Promise.all([getInferenceProviders(request.signal), getInferenceRoutes(request.signal)])
      .then(([providers, routeResponse]) => {
        if (!request.isCurrent()) return
        setData(providers)
        setRoutes(routeResponse)
        const firstProvider = providers.providers[0]
        setSelectedProviderId(firstProvider?.id ?? null)
        setName(firstProvider?.name ?? "")
        setBaseUrl(firstProvider ? getInferenceProviderBaseUrlDraft(firstProvider) : "")
        setKey("")
        setModels(Object.fromEntries(routeResponse.routes.map((route) => [route.capabilityCode, route.modelName ?? ""])))
        setRouteProviders(Object.fromEntries(routeResponse.routes.map((route) => [route.capabilityCode, route.providerId ?? ""])))
      })
      .catch((loadError) => {
        if (request.isCurrent() && !isRequestAborted(loadError, request.signal)) {
          setError(getApiErrorMessage(loadError, copy.error, messages.common.ui.errors))
        }
      })
  }, [begin, copy.error, messages.common.ui.errors])

  async function saveProvider() {
    if (!provider) return
    setBusy(true)
    setError(null)
    setNotice(null)
    try {
      const result = await updateInferenceProvider(provider.id, {
        name: name.trim(),
        baseUrl: baseUrl.trim(),
        apiKey: { action: key.trim() ? "replace" : "keep", value: key.trim() || null },
        expectedRevision: provider.revision,
      })
      setData((current) => current ? { ...current, providers: current.providers.map((item) => item.id === result.id ? result : item) } : current)
      setKey("")
      setNotice({ message: copy.saved, variant: "default" })
    } catch (saveError) {
      setError(getApiErrorMessage(saveError, copy.error, messages.common.ui.errors))
    } finally {
      setBusy(false)
    }
  }

  async function saveModels() {
    if (!routes) return
    setBusy(true)
    setError(null)
    setNotice(null)
    try {
      const result = await updateInferenceRoutes({
        routes: routes.routes.map((route) => ({
          capabilityCode: route.capabilityCode,
          providerId: routeProviders[route.capabilityCode]?.trim() || null,
          modelName: models[route.capabilityCode]?.trim() || null,
          expectedRevision: route.revision,
        })),
      })
      setRoutes(result)
      setNotice({ message: copy.saved, variant: "default" })
    } catch (saveError) {
      setError(getApiErrorMessage(saveError, copy.error, messages.common.ui.errors))
    } finally {
      setBusy(false)
    }
  }

  async function verify() {
    if (!provider) return
    setBusy(true)
    setError(null)
    setNotice(null)
    try {
      const result = await verifyInferenceProvider(provider.id, { expectedRevision: provider.revision })
      setData((current) => current ? { ...current, providers: current.providers.map((item) => item.id === result.provider.id ? result.provider : item) } : current)
      const statusCode = result.provider.runtimeStatusCode
      setNotice({
        message: statusCode === "recently-verified"
          ? copy.verified
          : statusCode === "unconfigured"
            ? copy.verificationNeedsConfiguration
            : statusCode === "currently-unavailable"
              ? copy.verificationUnavailable
              : copy.verificationNeedsRoute,
        variant: statusCode === "recently-verified" ? "default" : "attention",
      })
    } catch (verifyError) {
      setError(getApiErrorMessage(verifyError, copy.error, messages.common.ui.errors))
    } finally {
      setBusy(false)
    }
  }

  const statusLabel = (code: string) => code === "configured-unverified"
    ? copy.configuredUnverified
    : code === "recently-verified"
      ? copy.recentlyVerified
      : code === "currently-unavailable"
        ? copy.currentlyUnavailable
        : copy.unconfigured

  return <PageFrame currentPath="/settings" onNavigate={onNavigate} contentClassName="settings-subpage-wrap">
    <PageTitle eyebrow={copy.eyebrow} title={copy.title} description={copy.description} />
    {error && <Alert variant="destructive"><AlertTitle>{messages.common.ui.states.applicationError.title}</AlertTitle><AlertDescription>{error}</AlertDescription></Alert>}
    {notice && <Alert variant={notice.variant}>{notice.variant === "default" && <Check size={16} aria-hidden="true" />}<AlertDescription>{notice.message}</AlertDescription></Alert>}

    <Card className="settings-subpage-card settings-inference-provider-card">
      <CardHeader>
        <h2>{copy.providerTitle}</h2>
        <p className="settings-inference-card-description">{copy.providerListDescription}</p>
      </CardHeader>
      <CardContent>
        <div className="settings-inference-provider-list" role="list" aria-label={copy.providerTitle}>
          {data?.providers.map((item) => <button
            type="button"
            className={`settings-inference-provider-item${item.id === selectedProviderId ? " settings-inference-provider-item-active" : ""}`}
            key={item.id}
            onClick={() => { setSelectedProviderId(item.id); setName(item.name); setBaseUrl(getInferenceProviderBaseUrlDraft(item)); setKey("") }}
          >
            <span className="settings-inference-provider-item-copy"><strong>{item.name}</strong><small>{getInferenceProviderBaseUrlDraft(item) || copy.baseUrlRequired}</small></span>
            <Badge variant={item.runtimeStatusCode === "unconfigured" ? "outline" : "accent"}>{statusLabel(item.runtimeStatusCode)}</Badge>
          </button>)}
        </div>
        {provider && <FieldGroup className="settings-provider-fields">
          <Field><FieldLabel>{copy.nameLabel}</FieldLabel><Input value={name} placeholder={copy.namePlaceholder} onChange={(event) => setName(event.target.value)} /></Field>
          {provider.isBaseUrlEditable && <Field><FieldLabel>{copy.baseUrlLabel}</FieldLabel><Input type="url" value={baseUrl} placeholder={copy.baseUrlPlaceholder} onChange={(event) => setBaseUrl(event.target.value)} /><FieldDescription>{copy.baseUrlHint}</FieldDescription></Field>}
          <Field><FieldLabel><KeyRound size={14} aria-hidden="true" />{copy.keyLabel}</FieldLabel><Input type="password" value={key} placeholder={copy.keyPlaceholder} onChange={(event) => setKey(event.target.value)} autoComplete="new-password" /></Field>
        </FieldGroup>}
      </CardContent>
      <CardFooter>
        <Button onClick={() => void saveProvider()} disabled={busy || !provider}><Save data-icon="inline-start" />{copy.save}</Button>
        {provider && <Button variant="outline" onClick={() => void verify()} disabled={busy}><ShieldCheck data-icon="inline-start" />{copy.verify}</Button>}
      </CardFooter>
    </Card>

    <Card className="settings-subpage-card settings-inference-routes-card">
      <CardHeader><h2>{copy.routesTitle}</h2></CardHeader>
      <CardContent className="settings-route-list">
        {routes?.routes.map((route) => {
          const selectedRouteProvider = data?.providers.find((item) => item.id === routeProviders[route.capabilityCode])
          const isChat = route.capabilityCode === "chat"
          return <div className="settings-route-row" key={route.capabilityCode}>
            <div><strong>{isChat ? copy.chat : copy.embedding}</strong><Badge variant={route.runtimeStatusCode === "unconfigured" ? "outline" : "accent"}>{statusLabel(route.runtimeStatusCode)}</Badge></div>
            <Select value={routeProviders[route.capabilityCode] || "none"} onValueChange={(value) => setRouteProviders((current) => ({ ...current, [route.capabilityCode]: value === "none" || value === null ? "" : value }))}>
              <SelectTrigger><SelectValue>{selectedRouteProvider?.name ?? copy.providerPlaceholder}</SelectValue></SelectTrigger>
              <SelectContent><SelectGroup><SelectItem value="none">{copy.providerNone}</SelectItem>{data?.providers.map((item) => <SelectItem value={item.id} key={item.id}>{item.name}</SelectItem>)}</SelectGroup></SelectContent>
            </Select>
            <Input value={models[route.capabilityCode] ?? ""} placeholder={copy.modelPlaceholder} onChange={(event) => setModels((current) => ({ ...current, [route.capabilityCode]: event.target.value }))} />
          </div>
        })}
      </CardContent>
      <CardFooter><Button onClick={() => void saveModels()} disabled={busy || !routes}><Save data-icon="inline-start" />{copy.saveRoutes}</Button></CardFooter>
    </Card>
  </PageFrame>
}
