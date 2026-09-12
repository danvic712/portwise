import { CalendarDays, Check, CircleGauge, Info, Save, ShieldCheck, SlidersHorizontal } from "lucide-react"
import { useCallback, useEffect, useRef, useState } from "react"

import { EmptyState, ErrorState } from "@/components/feedback/AsyncState"
import { PageFrame } from "@/components/layout/PageFrame"
import { PageTitle, SectionHeading } from "@/components/layout/PageHeading"
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/Alert"
import { Badge } from "@/components/ui/Badge"
import { Button } from "@/components/ui/Button"
import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/Card"
import { Field, FieldDescription, FieldLabel, FieldSet } from "@/components/ui/Field"
import { Input } from "@/components/ui/Input"
import { getApiErrorMessage } from "@/shared/http/api-errors"
import { interpolate, useLocale } from "@/shared/i18n/i18n"
import { displayStockName } from "@/shared/display/stock-display"
import { formatMoney, formatNumber, stockKey, today } from "@/shared/utils/utils"
import type { SaveStockModelParametersRequest, StockWatchlistItem } from "@/shared/http/api-types"
import type { QueryPatch } from "@/app/routing/navigation"
import { isRequestAborted, useLatestRequest } from "@/shared/hooks/useLatestRequest"
import { getStockModelParameters, getWatchedStocks, saveStockModelParameters } from "@/stocks/stocks.api"
import { SettingsPageSkeleton, SettingsParameterSkeleton } from "@/settings/SettingsSkeleton"
import "./settings.css"

type NumericKey = Exclude<keyof SaveStockModelParametersRequest, "securityCode" | "exchangeCode" | "modelVersion" | "effectiveFromDate">
type FieldDefinition = { key: NumericKey; step?: string }
type ParameterGroupKey = "thresholds" | "ratios" | "limits"

const thresholdFields: FieldDefinition[] = [
  { key: "strongBuyYieldThreshold" },
  { key: "accumulationYieldThreshold" },
  { key: "partialTrimYieldThreshold" },
  { key: "aggressiveTrimYieldThreshold" },
]
const ratioFields: FieldDefinition[] = [
  { key: "strongBuyBudgetRatio" },
  { key: "accumulateBudgetRatio" },
  { key: "partialTrimRatio" },
  { key: "aggressiveTrimRatio" },
  { key: "maxSecurityWeight" },
  { key: "maxSectorWeight" },
  { key: "cashReserveRatio" },
  { key: "transactionFeeRatio" },
]
const limitFields: FieldDefinition[] = [
  { key: "maxSingleTradeAmount", step: "0.01" },
  { key: "maxPeriodBudgetAmount", step: "0.01" },
  { key: "minimumTransactionFeeAmount", step: "0.01" },
  { key: "tradingLotSize", step: "1" },
]
const parameterGroups: Array<{ key: ParameterGroupKey; fields: FieldDefinition[]; percent: boolean }> = [
  { key: "thresholds", fields: thresholdFields, percent: true },
  { key: "ratios", fields: ratioFields, percent: true },
  { key: "limits", fields: limitFields, percent: false },
]
const numericKeys = new Set<NumericKey>(parameterGroups.flatMap(({ fields }) => fields.map(({ key }) => key)))
const ratioKeys = new Set<NumericKey>(ratioFields.map(({ key }) => key))
const skeletonGroups = parameterGroups.map(({ key, fields }) => ({ key, fieldCount: fields.length }))

export function SettingsPage({ onNavigate, onReplaceQuery, initialStockKey, currentPath = "/strategy" }: { onNavigate: (path: string) => void; onReplaceQuery: (patch: QueryPatch) => void; initialStockKey: string; currentPath?: string }) {
  const { messages } = useLocale()
  const copy = messages.settings.ui
  const readErrorRef = useRef(copy.states.readError)
  const parameterErrorRef = useRef(copy.states.parametersReadError)
  const [stocks, setStocks] = useState<StockWatchlistItem[]>([])
  const [parameters, setParameters] = useState<SaveStockModelParametersRequest | null>(null)
  const [parametersStockKey, setParametersStockKey] = useState("")
  const [loading, setLoading] = useState(true)
  const [parametersLoading, setParametersLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [message, setMessage] = useState<string | null>(null)
  const { begin: beginStocks } = useLatestRequest()
  const { begin: beginParameters } = useLatestRequest()
  const { begin: beginSave, cancel: cancelSave } = useLatestRequest()
  const selectedKeyRef = useRef("")

  useEffect(() => {
    readErrorRef.current = copy.states.readError
    parameterErrorRef.current = copy.states.parametersReadError
  }, [copy.states.parametersReadError, copy.states.readError])

  const loadStocks = useCallback(async () => {
    const request = beginStocks()
    setLoading(true)
    setError(null)
    try {
      const list = await getWatchedStocks(request.signal)
      if (!request.isCurrent()) return
      setStocks(list)
    } catch (loadError) {
      if (request.isCurrent() && !isRequestAborted(loadError, request.signal)) setError(getApiErrorMessage(loadError, readErrorRef.current, messages.common.ui.errors))
    } finally {
      if (request.isCurrent()) setLoading(false)
    }
  }, [beginStocks, messages.common.ui.errors])

  const loadParameters = useCallback(async () => {
    const request = beginParameters()
    cancelSave()
    setSaving(false)
    const selectedKey = stocks.some((item) => stockKey(item) === initialStockKey)
      ? initialStockKey
      : stocks[0]
        ? stockKey(stocks[0])
        : ""
    const stock = stocks.find((item) => stockKey(item) === selectedKey)
    if (!stock) {
      if (request.isCurrent()) {
        setParameters(null)
        setParametersStockKey("")
        setParametersLoading(false)
      }
      return
    }

    setParametersLoading(true)
    setParameters(null)
    setParametersStockKey("")
    setError(null)
    setMessage(null)
    try {
      const result = await getStockModelParameters(stock.securityCode, stock.exchangeCode, request.signal)
      if (request.isCurrent()) {
        setParameters(result ? { ...result, effectiveFromDate: result.effectiveFromDate || today() } : null)
        setParametersStockKey(selectedKey)
      }
    } catch (loadError) {
      if (request.isCurrent() && !isRequestAborted(loadError, request.signal)) setError(getApiErrorMessage(loadError, parameterErrorRef.current, messages.common.ui.errors))
    } finally {
      if (request.isCurrent()) setParametersLoading(false)
    }
  }, [beginParameters, cancelSave, initialStockKey, messages.common.ui.errors, stocks])

  useEffect(() => {
    const timeoutId = window.setTimeout(() => { void loadStocks() }, 0)
    return () => window.clearTimeout(timeoutId)
  }, [loadStocks])

  useEffect(() => {
    const timeoutId = window.setTimeout(() => { void loadParameters() }, 0)
    return () => window.clearTimeout(timeoutId)
  }, [loadParameters])

  const selectedKey = stocks.some((item) => stockKey(item) === initialStockKey)
    ? initialStockKey
    : stocks[0]
      ? stockKey(stocks[0])
      : ""

  useEffect(() => {
    selectedKeyRef.current = selectedKey
  }, [selectedKey])
  const selectedStock = stocks.find((stock) => stockKey(stock) === selectedKey)
  const activeParameters = parametersStockKey === selectedKey ? parameters : null
  const invalidParameters = activeParameters ? hasInvalidParameters(activeParameters) : false

  function changeStock(stock: StockWatchlistItem) {
    const nextKey = stockKey(stock)
    if (nextKey === selectedKey) return
    setParametersLoading(true)
    setParameters(null)
    setParametersStockKey("")
    setError(null)
    setMessage(null)
    cancelSave()
    setSaving(false)
    onReplaceQuery({ stock: stock.securityCode, exchange: stock.exchangeCode })
  }

  function updateValue(key: keyof SaveStockModelParametersRequest, value: string) {
    setParameters((current) => current ? {
      ...current,
      [key]: key === "modelVersion" || key === "securityCode" || key === "exchangeCode" || key === "effectiveFromDate" ? value : value === "" ? 0 : Number(value),
    } : current)
  }

  async function save() {
    if (!activeParameters) return
    if (hasInvalidParameters(activeParameters)) {
      setError(copy.validation.invalid)
      setMessage(null)
      return
    }

    const savedStockKey = selectedKey
    const request = beginSave()
    setSaving(true)
    setError(null)
    setMessage(null)
    try {
      const saved = await saveStockModelParameters(activeParameters, request.signal)
      if (request.isCurrent() && selectedKeyRef.current === savedStockKey) {
        setParameters({ ...saved, effectiveFromDate: saved.effectiveFromDate || today() })
        setMessage(copy.states.successMessage)
      }
    } catch (saveError) {
      if (request.isCurrent() && !isRequestAborted(saveError, request.signal)) setError(getApiErrorMessage(saveError, copy.states.recordError, messages.common.ui.errors))
    } finally {
      if (request.isCurrent()) setSaving(false)
    }
  }

  if (loading) {
    return <PageFrame currentPath={currentPath} onNavigate={onNavigate} dataState="pending" contentClassName="settings-page-wrap"><SettingsPageSkeleton label={copy.states.loading} groups={skeletonGroups} /></PageFrame>
  }

  if (error && !stocks.length) {
    return <PageFrame currentPath={currentPath} onNavigate={onNavigate} dataState="unknown" contentClassName="settings-page-wrap"><ErrorState message={error} onRetry={() => void loadStocks()} /></PageFrame>
  }

  if (!stocks.length) {
    return <PageFrame currentPath={currentPath} onNavigate={onNavigate} dataState="unknown" contentClassName="settings-page-wrap"><EmptyState title={copy.states.emptyTitle} description={copy.states.emptyDescription} action={<Button onClick={() => onNavigate("/onboarding")}>{copy.actions.goSetup}</Button>} /></PageFrame>
  }

  const stockLabel = selectedStock ? `${displayStockName(selectedStock, messages.stocks.ui.identity.pendingName)} · ${selectedStock.securityCode}` : copy.form.stockFallback

  return (
    <PageFrame currentPath={currentPath} onNavigate={onNavigate} dataState="unknown" contentClassName="settings-page-wrap">
      <PageTitle
        eyebrow={copy.page.eyebrow}
        title={copy.page.title}
        description={copy.page.description}
      />

      {error && <Alert variant="destructive" className="settings-feedback"><AlertTitle>{copy.states.noticeTitle}</AlertTitle><AlertDescription>{error}</AlertDescription></Alert>}
      {message && <Alert className="settings-feedback"><Check size={17} aria-hidden="true" /><div><AlertTitle>{copy.states.successTitle}</AlertTitle><AlertDescription>{message}</AlertDescription></div></Alert>}

      <Card className="settings-workflow-card">
        <CardHeader className="settings-workflow-header">
          <SectionHeading label={copy.workflow.eyebrow} title={copy.workflow.title} description={copy.workflow.description} />
          <span className="settings-workflow-sticker" aria-hidden="true"><SlidersHorizontal size={19} /></span>
        </CardHeader>
        <CardContent className="settings-workflow-steps">
          {copy.workflow.steps.map((step) => <div className="settings-workflow-step" key={step.number}><span>{step.number}</span><div><strong>{step.title}</strong><p>{step.description}</p></div></div>)}
        </CardContent>
      </Card>

      <div className="settings-layout">
        <Card className="settings-stocks-card">
          <CardHeader className="settings-card-header">
            <SectionHeading label={copy.stocks.eyebrow} title={copy.stocks.title} description={copy.stocks.description} />
            <Badge variant="accent">{interpolate(copy.stocks.stockCount, { count: stocks.length })}</Badge>
          </CardHeader>
          <CardContent className="settings-stock-list" role="group" aria-label={copy.stocks.title}>
            {stocks.map((stock) => {
              const key = stockKey(stock)
              const holding = stock.holding
              const isSelected = selectedKey === key
              return <Button key={key} variant="ghost" className={`settings-stock-item ${isSelected ? "settings-stock-item-active" : ""}`} type="button" onClick={() => changeStock(stock)} aria-pressed={isSelected}>
                <span className="settings-stock-avatar" aria-hidden="true">{displayStockName(stock, messages.stocks.ui.identity.pendingName).slice(0, 2)}</span>
                <span className="settings-stock-copy"><strong>{displayStockName(stock, messages.stocks.ui.identity.pendingName)}</strong><small>{stock.securityCode} · {stock.exchangeCode}</small></span>
                <span className="settings-stock-meta"><span>{copy.stocks.position}</span><strong>{holding ? `${formatNumber(holding.heldShares, 0)} ${copy.stocks.shareUnit}` : copy.stocks.positionEmpty}</strong>{holding && <small>{copy.stocks.cost} {formatMoney(holding.averageCostPerShare)}</small>}</span>
              </Button>
            })}
          </CardContent>
          <CardFooter className="settings-stock-footer"><ShieldCheck size={16} aria-hidden="true" /><span>{copy.stocks.tip}</span></CardFooter>
        </Card>

        <Card className="settings-editor-card" id="settings-parameter-editor" data-loading={parametersLoading ? "true" : "false"} aria-busy={parametersLoading}>
          <CardHeader className="settings-card-header settings-editor-header">
            <SectionHeading label={copy.form.eyebrow} title={interpolate(copy.form.title, { stock: stockLabel })} description={copy.form.description} />
            {selectedStock && <div className="settings-editor-stamp"><span>{selectedStock.exchangeCode}</span><strong>{selectedStock.securityCode}</strong></div>}
          </CardHeader>
          {parametersLoading ? <SettingsParameterSkeleton label={copy.states.parametersLoading} groups={skeletonGroups} /> : activeParameters ? <>
            <CardContent className="settings-editor-content settings-editor-content-ready" key={selectedKey}>
              <FieldSet className="settings-identity-grid">
                <Field>
                  <FieldLabel htmlFor="model-version">{copy.form.modelVersion}</FieldLabel>
                  <Input id="model-version" value={activeParameters.modelVersion} aria-describedby="model-version-description" onChange={(event) => updateValue("modelVersion", event.target.value)} />
                  <FieldDescription id="model-version-description">{copy.form.modelVersionHint}</FieldDescription>
                </Field>
                <Field>
                  <FieldLabel htmlFor="effective-date">{copy.form.effectiveDate}</FieldLabel>
                  <div className="settings-date-field"><CalendarDays size={16} aria-hidden="true" /><Input id="effective-date" type="date" value={activeParameters.effectiveFromDate} aria-describedby="effective-date-description" onChange={(event) => updateValue("effectiveFromDate", event.target.value)} /></div>
                  <FieldDescription id="effective-date-description">{copy.form.effectiveDateHint}</FieldDescription>
                </Field>
              </FieldSet>

              {parameterGroups.map((group) => <section className="settings-group" key={group.key}>
                <div className="settings-group-heading"><SectionHeading label={group.key === "thresholds" ? "01" : group.key === "ratios" ? "02" : "03"} title={copy.groups[group.key].title} description={copy.groups[group.key].description} /><CircleGauge size={20} aria-hidden="true" /></div>
                <FieldSet className="settings-field-grid">
                  {group.fields.map((field) => <Field key={field.key}>
                    <FieldLabel htmlFor={field.key}>{copy.fields[field.key].label}</FieldLabel>
                    <div className="settings-input-wrap"><Input id={field.key} type="number" min="0" max={group.percent ? "1" : undefined} step={field.step ?? "0.01"} value={String(activeParameters[field.key])} aria-describedby={`${field.key}-description`} onChange={(event) => updateValue(field.key, event.target.value)} aria-invalid={group.percent && (activeParameters[field.key] < 0 || activeParameters[field.key] > 1) ? true : undefined} /><span>{group.percent ? copy.form.ratioRange : ""}</span></div>
                    <FieldDescription id={`${field.key}-description`}>{copy.fields[field.key].help}</FieldDescription>
                  </Field>)}
                </FieldSet>
              </section>)}
            </CardContent>
            <CardFooter className="settings-editor-footer settings-editor-footer-ready" key={selectedKey}>
              <div className="settings-editor-hint"><Info size={16} aria-hidden="true" /><span>{invalidParameters ? copy.validation.invalid : copy.form.footerHint}</span></div>
              <div className="settings-editor-actions"><span className="settings-footer-stock">{stockLabel}</span><Button onClick={() => void save()} disabled={!parameters || parametersLoading || saving || invalidParameters}><Save data-icon="inline-start" />{saving ? copy.actions.saving : copy.actions.save}</Button></div>
            </CardFooter>
          </> : <CardContent className="settings-empty-parameters"><EmptyState title={copy.states.noParametersTitle} description={copy.states.noParametersDescription} /></CardContent>}
        </Card>
      </div>
    </PageFrame>
  )
}

function hasInvalidParameters(parameters: SaveStockModelParametersRequest) {
  return Array.from(numericKeys).some((key) => {
    const value = parameters[key]
    return typeof value !== "number" || !Number.isFinite(value) || value < 0 || (ratioKeys.has(key) && value > 1) || (key === "tradingLotSize" && value <= 0)
  })
}
