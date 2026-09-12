import { ArrowRight, CheckCircle2, KeyRound, Palette, Sparkles } from "lucide-react"
import { useEffect, useMemo, useState } from "react"

import { Alert, AlertDescription, AlertTitle } from "@/components/ui/Alert"
import { Button } from "@/components/ui/Button"
import { Card, CardContent, CardDescription, CardFooter, CardHeader } from "@/components/ui/Card"
import { Field, FieldDescription, FieldGroup, FieldLabel } from "@/components/ui/Field"
import { Input } from "@/components/ui/Input"
import { Select, SelectContent, SelectGroup, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/Select"
import { PageFrame } from "@/components/layout/PageFrame"
import { getApiErrorMessage } from "@/shared/http/api-errors"
import { isRequestAborted, useLatestRequest } from "@/shared/hooks/useLatestRequest"
import { useLocale, type Locale } from "@/shared/i18n/i18n"
import { useTheme, type Theme } from "@/app/providers/ThemeContext"
import type { CompleteInitializationRequest, CompleteInitializationResponse, StockDataProvidersResponse } from "@/shared/http/api-types"
import { completeInitialization } from "@/onboarding/initialization.api"
import { getStockDataProviders } from "@/settings/stock-data-providers.api"
import "./onboarding.css"

type OnboardingPageProps = {
  onNavigate: (path: string) => void
  onComplete?: (response: CompleteInitializationResponse) => void
}

const defaultBaseUrl = "https://api.openai.com/v1"

export function OnboardingPage({ onNavigate, onComplete }: OnboardingPageProps) {
  const { locale, setLocale, messages } = useLocale()
  const { theme, setTheme } = useTheme()
  const copy = messages.onboarding.ui
  const [portfolioName, setPortfolioName] = useState("")
  const [ftShareKey, setFtShareKey] = useState("")
  const [inferenceName, setInferenceName] = useState("")
  const [inferenceBaseUrl, setInferenceBaseUrl] = useState(defaultBaseUrl)
  const [inferenceKey, setInferenceKey] = useState("")
  const [chatModel, setChatModel] = useState("")
  const [embeddingModel, setEmbeddingModel] = useState("")
  const [definitions, setDefinitions] = useState<StockDataProvidersResponse["definitions"]>([])
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const { begin: beginSubmit } = useLatestRequest()
  const { begin: beginDefinitions } = useLatestRequest()

  useEffect(() => {
    const request = beginDefinitions()
    void getStockDataProviders(request.signal)
      .then((response) => {
        if (request.isCurrent()) setDefinitions(response.definitions)
      })
      .catch(() => {
        // FTShare is optional during onboarding; the settings page can load it later.
      })
  }, [beginDefinitions])

  const ftShareDefinition = useMemo(
    () => definitions.find((definition) => definition.providerKindCode === "ftshare"),
    [definitions],
  )

  function buildRequest(): CompleteInitializationRequest {
    const stockDataProviders = ftShareKey.trim() && ftShareDefinition
      ? [{
        providerDefinitionId: ftShareDefinition.id,
        name: "FTShare",
        credentials: { action: "replace", value: ftShareKey.trim() },
      }]
      : null
    const inferenceConfigured = inferenceName.trim() || inferenceKey.trim() || chatModel.trim() || embeddingModel.trim()
    const inferenceProviders = inferenceConfigured
      ? [{
        name: inferenceName.trim(),
        baseUrl: inferenceBaseUrl.trim(),
        apiKey: { action: inferenceKey.trim() ? "replace" : "clear", value: inferenceKey.trim() || null },
      }]
      : null
    const inferenceRoutes = inferenceConfigured
      ? [
        { capabilityCode: "chat", providerName: inferenceName.trim() || null, modelName: chatModel.trim() || null },
        { capabilityCode: "embedding", providerName: inferenceName.trim() || null, modelName: embeddingModel.trim() || null },
      ]
      : null

    return {
      languageCode: locale,
      themeCode: theme,
      portfolioName: portfolioName.trim(),
      stockDataProviders,
      stockDataRoutes: null,
      inferenceProviders,
      inferenceRoutes,
    }
  }

  async function submit() {
    setError(null)
    if (!portfolioName.trim()) {
      setError(copy.validation.portfolio)
      return
    }
    const hasInferenceInput = Boolean(inferenceName.trim() || inferenceKey.trim() || chatModel.trim() || embeddingModel.trim())
    if (hasInferenceInput && (!inferenceName.trim() || !inferenceKey.trim() || !chatModel.trim() || !embeddingModel.trim())) {
      setError(copy.validation.inference)
      return
    }

    const request = beginSubmit()
    setSubmitting(true)
    try {
      const result = await completeInitialization(buildRequest(), request.signal)
      if (request.isCurrent()) {
        onComplete?.(result)
        onNavigate("/overview")
      }
    } catch (submitError) {
      if (request.isCurrent() && !isRequestAborted(submitError, request.signal)) {
        setError(getApiErrorMessage(submitError, copy.states.saveError, messages.common.ui.errors))
      }
    } finally {
      if (request.isCurrent()) setSubmitting(false)
    }
  }

  return (
    <PageFrame currentPath="/onboarding" onNavigate={onNavigate} showBreadcrumb={false} contentClassName="onboarding-page-wrap">
      <div className="onboarding-page">
        <section className="onboarding-intro" aria-labelledby="onboarding-title">
          <span className="onboarding-stamp"><Sparkles size={15} aria-hidden="true" /> {copy.stamp}</span>
          <p className="eyebrow">{copy.eyebrow}</p>
          <h1 id="onboarding-title"><span>{copy.titleLineOne}</span><span>{copy.titleLineTwo}</span></h1>
          <p>{copy.description}</p>
          <Alert className="onboarding-callout" variant="attention">
            <KeyRound size={18} aria-hidden="true" />
            <div><AlertTitle>{copy.calloutTitle}</AlertTitle><AlertDescription>{copy.calloutDescription}</AlertDescription></div>
          </Alert>
        </section>

        <Card className="onboarding-card">
          <CardHeader className="onboarding-card-header">
            <div><p className="eyebrow">{copy.cardEyebrow}</p><h2>{copy.cardTitle}</h2><CardDescription>{copy.cardDescription}</CardDescription></div>
            <span className="onboarding-card-mark" aria-hidden="true"><CheckCircle2 size={22} /></span>
          </CardHeader>
          {error && <Alert variant="destructive" className="onboarding-error"><AlertTitle>{copy.states.errorTitle}</AlertTitle><AlertDescription>{error}</AlertDescription></Alert>}
          <CardContent className="onboarding-card-content">
            <form className="onboarding-form" onSubmit={(event) => { event.preventDefault(); void submit() }}>
              <FieldGroup className="onboarding-field-grid">
                <Field><FieldLabel htmlFor="onboarding-portfolio">{copy.fields.portfolioName}</FieldLabel><Input id="onboarding-portfolio" value={portfolioName} placeholder={copy.fields.portfolioPlaceholder} onChange={(event) => setPortfolioName(event.target.value)} required /><FieldDescription>{copy.fields.portfolioHint}</FieldDescription></Field>
                <Field><FieldLabel htmlFor="onboarding-language">{copy.fields.language}</FieldLabel><Select value={locale} onValueChange={(value) => { if (value === "zh-CN" || value === "en-US") setLocale(value as Locale) }}><SelectTrigger id="onboarding-language"><SelectValue>{locale === "zh-CN" ? messages.common.ui.language.zhCN : messages.common.ui.language.enUS}</SelectValue></SelectTrigger><SelectContent><SelectGroup><SelectItem value="zh-CN">{messages.common.ui.language.zhCN}</SelectItem><SelectItem value="en-US">{messages.common.ui.language.enUS}</SelectItem></SelectGroup></SelectContent></Select></Field>
                <Field><FieldLabel htmlFor="onboarding-theme"><Palette size={14} aria-hidden="true" />{copy.fields.theme}</FieldLabel><Select value={theme} onValueChange={(value) => { if (value === "light" || value === "dark" || value === "system") setTheme(value as Theme) }}><SelectTrigger id="onboarding-theme"><SelectValue>{messages.common.ui.theme[theme]}</SelectValue></SelectTrigger><SelectContent><SelectGroup><SelectItem value="system">{messages.common.ui.theme.system}</SelectItem><SelectItem value="light">{messages.common.ui.theme.light}</SelectItem><SelectItem value="dark">{messages.common.ui.theme.dark}</SelectItem></SelectGroup></SelectContent></Select></Field>
              </FieldGroup>

              <section className="onboarding-section"><div className="onboarding-section-heading"><span>01</span><div><h3>{copy.stock.title}</h3><p>{copy.stock.description}</p></div></div><Field><FieldLabel htmlFor="onboarding-ftshare-key">{copy.stock.keyLabel}</FieldLabel><Input id="onboarding-ftshare-key" type="password" value={ftShareKey} placeholder={copy.stock.keyPlaceholder} onChange={(event) => setFtShareKey(event.target.value)} autoComplete="new-password" /><FieldDescription>{copy.stock.keyHint}</FieldDescription></Field></section>

              <section className="onboarding-section"><div className="onboarding-section-heading"><span>02</span><div><h3>{copy.inference.title}</h3><p>{copy.inference.description}</p></div></div><FieldGroup className="onboarding-field-grid"><Field><FieldLabel htmlFor="onboarding-inference-name">{copy.inference.nameLabel}</FieldLabel><Input id="onboarding-inference-name" value={inferenceName} placeholder={copy.inference.namePlaceholder} onChange={(event) => setInferenceName(event.target.value)} /></Field><Field><FieldLabel htmlFor="onboarding-inference-base-url">{copy.inference.baseUrlLabel}</FieldLabel><Input id="onboarding-inference-base-url" value={inferenceBaseUrl} onChange={(event) => setInferenceBaseUrl(event.target.value)} /></Field><Field><FieldLabel htmlFor="onboarding-inference-key">{copy.inference.keyLabel}</FieldLabel><Input id="onboarding-inference-key" type="password" value={inferenceKey} placeholder={copy.inference.keyPlaceholder} onChange={(event) => setInferenceKey(event.target.value)} autoComplete="new-password" /></Field><Field><FieldLabel htmlFor="onboarding-chat-model">{copy.inference.chatModelLabel}</FieldLabel><Input id="onboarding-chat-model" value={chatModel} placeholder={copy.inference.modelPlaceholder} onChange={(event) => setChatModel(event.target.value)} /></Field><Field><FieldLabel htmlFor="onboarding-embedding-model">{copy.inference.embeddingModelLabel}</FieldLabel><Input id="onboarding-embedding-model" value={embeddingModel} placeholder={copy.inference.modelPlaceholder} onChange={(event) => setEmbeddingModel(event.target.value)} /></Field></FieldGroup><FieldDescription>{copy.inference.hint}</FieldDescription></section>
            </form>
          </CardContent>
          <CardFooter className="onboarding-card-footer"><span>{copy.footer}</span><Button size="lg" onClick={() => void submit()} disabled={submitting}>{submitting ? copy.actions.saving : copy.actions.save}<ArrowRight data-icon="inline-end" /></Button></CardFooter>
        </Card>
      </div>
    </PageFrame>
  )
}
