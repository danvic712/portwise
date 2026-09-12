import { ArrowLeft, ArrowRight, Check, Info, Plus, Trash2 } from "lucide-react"
import { useEffect, useMemo, useRef, useState } from "react"

import { Alert, AlertDescription, AlertTitle } from "@/components/ui/Alert"
import { Button } from "@/components/ui/Button"
import { Card, CardContent, CardHeader } from "@/components/ui/Card"
import { Field, FieldDescription, FieldGroup, FieldLabel } from "@/components/ui/Field"
import { Input } from "@/components/ui/Input"
import { NumberInput } from "@/components/ui/NumberInput"
import { Select, SelectContent, SelectGroup, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/Select"
import { PageFrame } from "@/components/layout/PageFrame"
import { getApiErrorMessage } from "@/shared/http/api-errors"
import { isRequestAborted, useLatestRequest } from "@/shared/hooks/useLatestRequest"
import { interpolate, useLocale, type Locale } from "@/shared/i18n/i18n"
import { useTheme, type Theme } from "@/app/providers/ThemeContext"
import type { CompleteInitializationRequest, CompleteInitializationResponse, StockDataProvidersResponse } from "@/shared/http/api-types"
import { completeInitialization } from "@/onboarding/initialization.api"
import { getStockDataProviders } from "@/settings/stock-data-providers.api"
import "./onboarding.css"

type OnboardingProps = {
  onNavigate: (path: string) => void
  onComplete?: (response: CompleteInitializationResponse) => void
}

type OnboardingStep = 1 | 2 | 3 | 4

type StepDefinition = {
  number: OnboardingStep
  key: "preferences" | "portfolio" | "stockData" | "inference"
  optional?: boolean
}

type InitialStockDraft = {
  id: number
  securityCode: string
  exchangeCode: string
  heldShares: string
}

const defaultBaseUrl = "https://api.openai.com/v1"
const aShareExchangeCodes = {
  sse: "SSE",
  szse: "SZSE",
  bse: "BSE",
} as const

function exchangeDisplayName(code: string, options: Record<string, string>) {
  return options[code] ?? code
}
const stepDefinitions: StepDefinition[] = [
  { number: 1, key: "preferences" },
  { number: 2, key: "portfolio" },
  { number: 3, key: "stockData", optional: true },
  { number: 4, key: "inference", optional: true },
]

export function Onboarding({ onNavigate, onComplete }: OnboardingProps) {
  const { locale, setLocale, messages } = useLocale()
  const { theme, setTheme } = useTheme()
  const copy = messages.onboarding.ui
  const [step, setStep] = useState<OnboardingStep>(1)
  const [portfolioName, setPortfolioName] = useState("")
  const stockDraftSequence = useRef(0)
  const [initialStocks, setInitialStocks] = useState<InitialStockDraft[]>([])
  const [ftShareKey, setFtShareKey] = useState("")
  const [inferenceName, setInferenceName] = useState("")
  const [inferenceBaseUrl, setInferenceBaseUrl] = useState(defaultBaseUrl)
  const [inferenceKey, setInferenceKey] = useState("")
  const [chatModel, setChatModel] = useState("")
  const [embeddingModel, setEmbeddingModel] = useState("")
  const [definitions, setDefinitions] = useState<StockDataProvidersResponse["definitions"]>([])
  const [definitionsError, setDefinitionsError] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const { begin: beginSubmit } = useLatestRequest()
  const { begin: beginDefinitions } = useLatestRequest()

  useEffect(() => {
    const request = beginDefinitions()
    void getStockDataProviders(request.signal)
      .then((response) => {
        if (request.isCurrent()) {
          setDefinitions(response.definitions)
          setDefinitionsError(false)
        }
      })
      .catch(() => {
        if (request.isCurrent()) setDefinitionsError(true)
      })
  }, [beginDefinitions])

  const ftShareDefinition = useMemo(
    () => definitions.find((definition) => definition.providerKindCode === "ftshare"),
    [definitions],
  )

  const hasStockDataInput = Boolean(ftShareKey.trim())
  const hasInferenceInput = Boolean(inferenceName.trim() || inferenceKey.trim() || chatModel.trim() || embeddingModel.trim() || (inferenceBaseUrl.trim() && inferenceBaseUrl.trim() !== defaultBaseUrl))
  const hasPortfolioInput = Boolean(portfolioName.trim())

  function createStockDraft(): InitialStockDraft {
    stockDraftSequence.current += 1
    return {
      id: stockDraftSequence.current,
      securityCode: "",
      exchangeCode: aShareExchangeCodes.sse,
      heldShares: "0",
    }
  }

  function addInitialStock() {
    const draft = createStockDraft()
    setInitialStocks((current) => [...current, draft])
    setError(null)
  }

  function updateInitialStock(id: number, patch: Partial<Omit<InitialStockDraft, "id">>) {
    setInitialStocks((current) => current.map((stock) => stock.id === id ? { ...stock, ...patch } : stock))
  }

  function removeInitialStock(id: number) {
    setInitialStocks((current) => current.filter((stock) => stock.id !== id))
    setError(null)
  }

  function buildRequest(): CompleteInitializationRequest {
    const stocks = initialStocks
      .filter((stock) => stock.securityCode.trim())
      .map((stock) => ({
        securityCode: stock.securityCode.trim(),
        exchangeCode: stock.exchangeCode,
        heldShares: Number(stock.heldShares),
      }))
    const stockDataProviders = hasStockDataInput && ftShareDefinition
      ? [{ providerDefinitionId: ftShareDefinition.id, name: copy.stock.providerName, credentials: { action: "replace" as const, value: ftShareKey.trim() } }]
      : null
    const inferenceProviders = hasInferenceInput
      ? [{ name: inferenceName.trim(), baseUrl: inferenceBaseUrl.trim(), apiKey: { action: inferenceKey.trim() ? "replace" as const : "clear" as const, value: inferenceKey.trim() || null } }]
      : null
    const inferenceRoutes = hasInferenceInput
      ? [
        { capabilityCode: "chat", providerName: inferenceName.trim() || null, modelName: chatModel.trim() || null },
        { capabilityCode: "embedding", providerName: inferenceName.trim() || null, modelName: embeddingModel.trim() || null },
      ]
      : null

    return { languageCode: locale, themeCode: theme, portfolioName: portfolioName.trim(), initialStocks: stocks.length ? stocks : null, stockDataProviders, stockDataRoutes: null, inferenceProviders, inferenceRoutes }
  }

  function validatePortfolioStep() {
    for (const stock of initialStocks) {
      const stockCode = stock.securityCode.trim()
      const hasStockInput = Boolean(stockCode || stock.heldShares.trim())
      if (!hasStockInput) continue
      if (!stockCode) return copy.validation.stockCodeRequired
      if (!/^\d{6}$/.test(stockCode)) return copy.validation.stockCode

      const parsedHeldShares = Number(stock.heldShares)
      if (stock.heldShares.trim() === "" || !Number.isInteger(parsedHeldShares) || parsedHeldShares < 0) return copy.validation.heldShares
    }
    return null
  }

  async function submit() {
    setError(null)
    if (!portfolioName.trim()) {
      setStep(2)
      setError(copy.validation.portfolio)
      return
    }
    const portfolioError = validatePortfolioStep()
    if (portfolioError) {
      setStep(2)
      setError(portfolioError)
      return
    }
    if (hasInferenceInput && (!inferenceName.trim() || !inferenceKey.trim() || !chatModel.trim() || !embeddingModel.trim())) {
      setStep(4)
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

  function nextStep() {
    setError(null)
    if (step === 2 && !portfolioName.trim()) {
      setError(copy.validation.portfolio)
      return
    }
    if (step === 2) {
      const portfolioError = validatePortfolioStep()
      if (portfolioError) {
        setError(portfolioError)
        return
      }
    }
    if (step === 4) {
      void submit()
      return
    }
    setStep((current) => (current + 1) as OnboardingStep)
  }

  function previousStep() {
    setError(null)
    setStep((current) => (Math.max(1, current - 1)) as OnboardingStep)
  }

  function useDefaults() {
    setLocale("zh-CN")
    setTheme("system")
    setError(null)
  }

  const activeStep = stepDefinitions[step - 1]
  const activeCopy = copy.activeStep
  const stepTitle = activeStep.key === "preferences"
    ? activeCopy.preferencesTitle
    : activeStep.key === "portfolio"
      ? activeCopy.portfolioTitle
      : activeStep.key === "stockData"
        ? activeCopy.stockTitle
        : activeCopy.inferenceTitle
  const stepDescription = activeStep.key === "preferences"
    ? activeCopy.preferencesDescription
    : activeStep.key === "portfolio"
      ? activeCopy.portfolioDescription
      : activeStep.key === "stockData"
        ? activeCopy.stockDescription
        : activeCopy.inferenceDescription
  const stepEyebrow = activeStep.key === "preferences"
    ? activeCopy.preferencesEyebrow
    : activeStep.key === "portfolio"
      ? activeCopy.portfolioEyebrow
      : activeStep.key === "stockData"
        ? activeCopy.stockEyebrow
        : activeCopy.inferenceEyebrow

  function isStepComplete(item: StepDefinition) {
    return item.key === "preferences"
      ? step > item.number
      : item.key === "portfolio"
        ? hasPortfolioInput && step > item.number
        : item.key === "stockData"
          ? hasStockDataInput && step > item.number
          : hasInferenceInput && step > item.number
  }

  function checklistStatus(item: StepDefinition) {
    if (isStepComplete(item)) return copy.checklist.complete
    if (item.number === step) return copy.checklist.active
    return item.optional ? copy.checklist.optional : copy.checklist.pending
  }

  const nextLabel = step === 1
    ? copy.actions.nextPortfolio
    : step === 2
      ? copy.actions.nextStock
      : step === 3
        ? copy.actions.nextInference
        : copy.actions.save

  return (
    <PageFrame currentPath="/onboarding" onNavigate={onNavigate} showBreadcrumb={false} showNavigation={false} showSettings={false} contentClassName="onboarding-page-wrap">
      <div className="onboarding-page">
        <section className="onboarding-intro" aria-labelledby="onboarding-title">
          <p className="eyebrow">{copy.eyebrow}</p>
          <h1 id="onboarding-title"><span>{copy.titleLineOne}</span><span>{copy.titleLineTwo}</span></h1>
          <p>{copy.description}</p>
          <Alert className="onboarding-callout" variant="default">
            <Info size={19} aria-hidden="true" />
            <div><AlertTitle>{copy.calloutTitle}</AlertTitle><AlertDescription>{copy.calloutDescription}</AlertDescription></div>
          </Alert>
        </section>

        <section className="onboarding-workspace" aria-labelledby="onboarding-workspace-title">
          <span className="onboarding-step-sticker" aria-hidden="true"><strong>{String(step).padStart(2, "0")}</strong><small>{copy.activeStep.stickerSuffix}</small></span>
          <Card className="onboarding-card">
            <CardHeader className="onboarding-card-header">
              <p className="eyebrow">{copy.cardEyebrow}<span className="onboarding-header-rule" aria-hidden="true" /></p>
              <h2 id="onboarding-workspace-title">{copy.cardTitle}</h2>
              <div className="onboarding-progress" aria-label={copy.cardEyebrow}>
                {stepDefinitions.map((item, index) => {
                  const isActive = item.number === step
                  const isComplete = isStepComplete(item)
                  const status = checklistStatus(item)
                  const statusTone = isActive ? "active" : isComplete ? "complete" : "pending"
                  return (
                    <div className="onboarding-progress-item" key={item.key}>
                      <Button type="button" variant="ghost" size="icon" className={`onboarding-progress-marker${isActive ? " onboarding-progress-marker-active" : ""}${isComplete ? " onboarding-progress-marker-complete" : ""}`} onClick={() => item.number <= step && setStep(item.number)} aria-current={isActive ? "step" : undefined} aria-label={`${copy.steps[item.key]}，${status}`}>
                        {isComplete ? <Check size={14} aria-hidden="true" /> : <span>{String(item.number).padStart(2, "0")}</span>}
                      </Button>
                      <span className={`onboarding-progress-label${isActive ? " onboarding-progress-label-active" : ""}`}>{copy.steps[item.key]}</span>
                      <span className={`onboarding-progress-status onboarding-progress-status-${statusTone}`}>{status}</span>
                      {index < stepDefinitions.length - 1 && <span className={`onboarding-progress-line${isComplete ? " onboarding-progress-line-complete" : ""}`} aria-hidden="true" />}
                    </div>
                  )
                })}
              </div>
            </CardHeader>

            <CardContent className="onboarding-card-content">
                <form className="onboarding-form" onSubmit={(event) => { event.preventDefault(); nextStep() }}>
                  <div className="onboarding-active-step">
                    <div className="onboarding-active-heading">
                      <span>{interpolate(activeCopy.format, { current: String(step).padStart(2, "0"), total: "04" })}</span>
                      <span className="onboarding-active-rule" aria-hidden="true" />
                      <p>{stepEyebrow}</p>
                    </div>
                    <h3>{stepTitle}</h3>
                    <p className="onboarding-active-description">{stepDescription}</p>
                    <div className="onboarding-form-divider" aria-hidden="true" />

                    {activeStep.key === "preferences" && <>
                      <FieldGroup className="onboarding-field-grid onboarding-preference-fields">
                        <Field>
                          <FieldLabel htmlFor="onboarding-language">{copy.fields.language}</FieldLabel>
                          <Select value={locale} onValueChange={(value) => { if (value === "zh-CN" || value === "en-US") setLocale(value as Locale) }}>
                            <SelectTrigger id="onboarding-language"><SelectValue>{locale === "zh-CN" ? messages.common.ui.language.zhCN : messages.common.ui.language.enUS}</SelectValue></SelectTrigger>
                            <SelectContent><SelectGroup><SelectItem value="zh-CN">{messages.common.ui.language.zhCN}</SelectItem><SelectItem value="en-US">{messages.common.ui.language.enUS}</SelectItem></SelectGroup></SelectContent>
                          </Select>
                        </Field>
                        <Field>
                          <FieldLabel htmlFor="onboarding-theme">{copy.fields.theme}</FieldLabel>
                          <Select value={theme} onValueChange={(value) => { if (value === "light" || value === "dark" || value === "system") setTheme(value as Theme) }}>
                            <SelectTrigger id="onboarding-theme"><SelectValue>{messages.common.ui.theme[theme]}</SelectValue></SelectTrigger>
                            <SelectContent><SelectGroup><SelectItem value="system">{messages.common.ui.theme.system}</SelectItem><SelectItem value="light">{messages.common.ui.theme.light}</SelectItem><SelectItem value="dark">{messages.common.ui.theme.dark}</SelectItem></SelectGroup></SelectContent>
                          </Select>
                        </Field>
                      </FieldGroup>
                      <FieldDescription>{activeCopy.preferencesHint}</FieldDescription>
                    </>}

                    {activeStep.key === "portfolio" && <FieldGroup className="onboarding-portfolio-fields">
                      <Field>
                        <FieldLabel htmlFor="onboarding-portfolio">{copy.fields.portfolioName}</FieldLabel>
                        <Input id="onboarding-portfolio" value={portfolioName} placeholder={copy.fields.portfolioPlaceholder} onChange={(event) => setPortfolioName(event.target.value)} autoComplete="off" required />
                        <FieldDescription>{copy.fields.portfolioHint}</FieldDescription>
                      </Field>

                      <section className="onboarding-stock-editor" aria-labelledby="onboarding-stock-editor-title">
                        <div className="onboarding-stock-editor-heading">
                          <div>
                            <FieldLabel id="onboarding-stock-editor-title">{copy.portfolio.stockSectionTitle}</FieldLabel>
                            <FieldDescription>{copy.portfolio.stockSectionDescription}</FieldDescription>
                          </div>
                          <Button type="button" variant="outline" size="sm" onClick={addInitialStock}>
                            <Plus data-icon="inline-start" />
                            {copy.portfolio.addStock}
                          </Button>
                        </div>

                        {initialStocks.length === 0 ? (
                          <div className="onboarding-stock-editor-empty">
                            <span>{copy.portfolio.stockEmpty}</span>
                            <Button type="button" variant="link" onClick={addInitialStock}>{copy.portfolio.addFirstStock}</Button>
                          </div>
                        ) : (
                          <div className="onboarding-stock-list">
                            {initialStocks.map((stock, index) => (
                              <div className="onboarding-stock-row" key={stock.id}>
                                <div className="onboarding-stock-row-heading">
                                  <span className="onboarding-stock-row-number">{String(index + 1).padStart(2, "0")}</span>
                                  <span>{interpolate(copy.portfolio.stockItem, { number: String(index + 1) })}</span>
                                  <Button type="button" variant="ghost" size="icon" className="onboarding-stock-remove" onClick={() => removeInitialStock(stock.id)} aria-label={copy.portfolio.removeStock}>
                                    <Trash2 size={15} aria-hidden="true" />
                                  </Button>
                                </div>
                                <FieldGroup className="onboarding-stock-row-fields">
                                  <Field>
                                    <FieldLabel htmlFor={`onboarding-stock-code-${stock.id}`}>{copy.portfolio.stockCode}</FieldLabel>
                                    <Input id={`onboarding-stock-code-${stock.id}`} value={stock.securityCode} placeholder={copy.portfolio.stockCodePlaceholder} onChange={(event) => updateInitialStock(stock.id, { securityCode: event.target.value })} inputMode="numeric" maxLength={6} autoComplete="off" />
                                  </Field>
                                  <Field>
                                    <FieldLabel htmlFor={`onboarding-stock-exchange-${stock.id}`}>{copy.portfolio.exchangeCode}</FieldLabel>
                                    <Select value={stock.exchangeCode} onValueChange={(value) => { if (value) updateInitialStock(stock.id, { exchangeCode: value }) }}>
                                      <SelectTrigger id={`onboarding-stock-exchange-${stock.id}`}>
                                        <SelectValue>{exchangeDisplayName(stock.exchangeCode, copy.portfolio.exchangeOptions)}</SelectValue>
                                      </SelectTrigger>
                                      <SelectContent>
                                        <SelectGroup>
                                          <SelectItem value={aShareExchangeCodes.sse}>{copy.portfolio.exchangeOptions.SSE}</SelectItem>
                                          <SelectItem value={aShareExchangeCodes.szse}>{copy.portfolio.exchangeOptions.SZSE}</SelectItem>
                                          <SelectItem value={aShareExchangeCodes.bse}>{copy.portfolio.exchangeOptions.BSE}</SelectItem>
                                        </SelectGroup>
                                      </SelectContent>
                                    </Select>
                                  </Field>
                                  <Field>
                                    <FieldLabel htmlFor={`onboarding-stock-held-${stock.id}`}>{copy.portfolio.heldShares}</FieldLabel>
                                    <NumberInput id={`onboarding-stock-held-${stock.id}`} min="0" step="1" value={stock.heldShares} onChange={(event) => updateInitialStock(stock.id, { heldShares: event.target.value })} placeholder={copy.portfolio.sharesPlaceholder} />
                                  </Field>
                                </FieldGroup>
                              </div>
                            ))}
                          </div>
                        )}
                      </section>
                    </FieldGroup>}

                    {activeStep.key === "stockData" && <Field>
                      <FieldLabel htmlFor="onboarding-ftshare-key">{copy.stock.keyLabel}</FieldLabel>
                      <Input id="onboarding-ftshare-key" type="password" value={ftShareKey} placeholder={copy.stock.keyPlaceholder} onChange={(event) => setFtShareKey(event.target.value)} autoComplete="new-password" />
                      <FieldDescription>{definitionsError ? copy.stock.loadError : copy.stock.keyHint}</FieldDescription>
                    </Field>}

                    {activeStep.key === "inference" && <FieldGroup className="onboarding-field-grid onboarding-inference-fields">
                      <Field><FieldLabel htmlFor="onboarding-inference-name">{copy.inference.nameLabel}</FieldLabel><Input id="onboarding-inference-name" value={inferenceName} placeholder={copy.inference.namePlaceholder} onChange={(event) => setInferenceName(event.target.value)} /></Field>
                      <Field><FieldLabel htmlFor="onboarding-inference-base-url">{copy.inference.baseUrlLabel}</FieldLabel><Input id="onboarding-inference-base-url" value={inferenceBaseUrl} onChange={(event) => setInferenceBaseUrl(event.target.value)} /></Field>
                      <Field><FieldLabel htmlFor="onboarding-inference-key">{copy.inference.keyLabel}</FieldLabel><Input id="onboarding-inference-key" type="password" value={inferenceKey} placeholder={copy.inference.keyPlaceholder} onChange={(event) => setInferenceKey(event.target.value)} autoComplete="new-password" /></Field>
                      <Field><FieldLabel htmlFor="onboarding-chat-model">{copy.inference.chatModelLabel}</FieldLabel><Input id="onboarding-chat-model" value={chatModel} placeholder={copy.inference.modelPlaceholder} onChange={(event) => setChatModel(event.target.value)} /></Field>
                      <Field><FieldLabel htmlFor="onboarding-embedding-model">{copy.inference.embeddingModelLabel}</FieldLabel><Input id="onboarding-embedding-model" value={embeddingModel} placeholder={copy.inference.modelPlaceholder} onChange={(event) => setEmbeddingModel(event.target.value)} /></Field>
                      <FieldDescription>{copy.inference.hint}</FieldDescription>
                    </FieldGroup>}
                    {error && <Alert variant="destructive" className="onboarding-error"><AlertTitle>{copy.states.errorTitle}</AlertTitle><AlertDescription>{error}</AlertDescription></Alert>}
                    <div className="onboarding-step-actions">
                      {step > 1 ? <Button type="button" variant="ghost" onClick={previousStep}><ArrowLeft data-icon="inline-start" />{copy.actions.back}</Button> : <span />}
                      <div className="onboarding-step-actions-primary">
                        {step === 1 && <Button type="button" variant="link" onClick={useDefaults}>{copy.actions.useDefaults}</Button>}
                        <Button size="lg" type="submit" disabled={submitting}>{submitting ? copy.actions.saving : nextLabel}<ArrowRight data-icon="inline-end" /></Button>
                      </div>
                    </div>
                  </div>
                </form>
            </CardContent>

          </Card>
        </section>
      </div>
    </PageFrame>
  )
}
