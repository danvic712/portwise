import { CalendarDays, Check, CircleGauge, Info, Save, ShieldCheck, SlidersHorizontal } from "lucide-react"
import { useCallback, useEffect, useRef, useState } from "react"

import { EmptyState, ErrorState } from "@/components/async-state"
import { PageFrame } from "@/components/page-frame"
import { PageTitle, SectionHeading } from "@/components/page-heading"
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card"
import { Field, FieldDescription, FieldLabel, FieldSet } from "@/components/ui/field"
import { Input } from "@/components/ui/input"
import { getApiErrorMessage } from "@/lib/api-errors"
import { interpolate, useLocale } from "@/lib/i18n"
import { displayStockName } from "@/lib/stock-display"
import { formatMoney, formatNumber, stockKey, today } from "@/lib/utils"
import type { SaveStockModelParametersRequest, StockWatchlistItem } from "@/lib/api-types"
import type { QueryPatch } from "@/lib/navigation"
import { getSettingsParameters, getSettingsStocks, updateSettingsParameters } from "@/features/settings/settings.api"
import { SettingsPageSkeleton, SettingsParameterSkeleton } from "@/features/settings/SettingsSkeleton"
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

export function SettingsPage({ onNavigate, onReplaceQuery, initialStockKey }: { onNavigate: (path: string) => void; onReplaceQuery: (patch: QueryPatch) => void; initialStockKey: string }) {
  const { messages } = useLocale()
  const copy = messages.settings.ui
  const readErrorRef = useRef(copy.states.readError)
  const parameterErrorRef = useRef(copy.states.parametersReadError)
  const [stocks, setStocks] = useState<StockWatchlistItem[]>([])
  const [selectedKey, setSelectedKey] = useState(initialStockKey)
  const [parameters, setParameters] = useState<SaveStockModelParametersRequest | null>(null)
  const [parametersStockKey, setParametersStockKey] = useState("")
  const [loading, setLoading] = useState(true)
  const [parametersLoading, setParametersLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [message, setMessage] = useState<string | null>(null)

  useEffect(() => {
    readErrorRef.current = copy.states.readError
    parameterErrorRef.current = copy.states.parametersReadError
  }, [copy.states.parametersReadError, copy.states.readError])

  const loadStocks = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const list = await getSettingsStocks()
      setStocks(list)
      setSelectedKey((current) => current && list.some((stock) => stockKey(stock) === current) ? current : list[0] ? stockKey(list[0]) : "")
    } catch (loadError) {
      setError(getApiErrorMessage(loadError, readErrorRef.current, messages.common.ui.errors))
    } finally {
      setLoading(false)
    }
  }, [messages.common.ui.errors])

  const loadParameters = useCallback(async (signal?: AbortSignal) => {
    const stock = stocks.find((item) => stockKey(item) === selectedKey)
    if (!stock) {
      setParameters(null)
      setParametersStockKey("")
      setParametersLoading(false)
      return
    }

    setParametersLoading(true)
    setParameters(null)
    setParametersStockKey("")
    setError(null)
    setMessage(null)
    try {
      const result = await getSettingsParameters(stock.securityCode, stock.exchangeCode, signal)
      if (!signal?.aborted) {
        setParameters(result)
        setParametersStockKey(selectedKey)
      }
    } catch (loadError) {
      if (!signal?.aborted) setError(getApiErrorMessage(loadError, parameterErrorRef.current, messages.common.ui.errors))
    } finally {
      if (!signal?.aborted) setParametersLoading(false)
    }
  }, [messages.common.ui.errors, selectedKey, stocks])

  useEffect(() => {
    const timeoutId = window.setTimeout(() => { void loadStocks() }, 0)
    return () => window.clearTimeout(timeoutId)
  }, [loadStocks])

  useEffect(() => {
    const controller = new AbortController()
    const timeoutId = window.setTimeout(() => { void loadParameters(controller.signal) }, 0)
    return () => {
      window.clearTimeout(timeoutId)
      controller.abort()
    }
  }, [loadParameters])

  const selectedStock = stocks.find((stock) => stockKey(stock) === selectedKey)
  const activeParameters = parametersStockKey === selectedKey ? parameters : null
  const invalidParameters = activeParameters ? hasInvalidParameters(activeParameters) : false

  function changeStock(stock: StockWatchlistItem) {
    const nextKey = stockKey(stock)
    if (nextKey === selectedKey) return
    setSelectedKey(nextKey)
    setParametersLoading(true)
    setParameters(null)
    setParametersStockKey("")
    setError(null)
    setMessage(null)
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

    setSaving(true)
    setError(null)
    setMessage(null)
    try {
      const saved = await updateSettingsParameters(activeParameters)
      setParameters(saved)
      setMessage(copy.states.successMessage)
    } catch (saveError) {
      setError(getApiErrorMessage(saveError, copy.states.recordError, messages.common.ui.errors))
    } finally {
      setSaving(false)
    }
  }

  if (loading) {
    return <PageFrame currentPath="/settings" onNavigate={onNavigate} dataState="pending" contentClassName="settings-page-wrap"><SettingsPageSkeleton label={copy.states.loading} groups={skeletonGroups} /></PageFrame>
  }

  if (error && !stocks.length) {
    return <PageFrame currentPath="/settings" onNavigate={onNavigate} dataState="unknown" contentClassName="settings-page-wrap"><ErrorState message={error} onRetry={() => void loadStocks()} /></PageFrame>
  }

  if (!stocks.length) {
    return <PageFrame currentPath="/settings" onNavigate={onNavigate} dataState="unknown" contentClassName="settings-page-wrap"><EmptyState title={copy.states.emptyTitle} description={copy.states.emptyDescription} action={<Button onClick={() => onNavigate("/setup")}>{copy.actions.goSetup}</Button>} /></PageFrame>
  }

  const stockLabel = selectedStock ? `${displayStockName(selectedStock, messages.stocks.ui.identity.pendingName)} · ${selectedStock.securityCode}` : copy.form.stockFallback

  return (
    <PageFrame currentPath="/settings" onNavigate={onNavigate} dataState="unknown" contentClassName="settings-page-wrap">
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
          <CardContent className="settings-stock-list" role="tablist" aria-label={copy.stocks.title}>
            {stocks.map((stock) => {
              const key = stockKey(stock)
              const holding = stock.holding
              const isSelected = selectedKey === key
              return <Button key={key} variant="ghost" className={`settings-stock-item ${isSelected ? "settings-stock-item-active" : ""}`} type="button" onClick={() => changeStock(stock)} role="tab" aria-selected={isSelected} aria-controls="settings-parameter-editor">
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
                  <Input id="model-version" value={activeParameters.modelVersion} onChange={(event) => updateValue("modelVersion", event.target.value)} />
                  <FieldDescription>{copy.form.modelVersionHint}</FieldDescription>
                </Field>
                <Field>
                  <FieldLabel htmlFor="effective-date">{copy.form.effectiveDate}</FieldLabel>
                  <div className="settings-date-field"><CalendarDays size={16} aria-hidden="true" /><Input id="effective-date" type="date" value={activeParameters.effectiveFromDate || today()} onChange={(event) => updateValue("effectiveFromDate", event.target.value)} /></div>
                  <FieldDescription>{copy.form.effectiveDateHint}</FieldDescription>
                </Field>
              </FieldSet>

              {parameterGroups.map((group) => <section className="settings-group" key={group.key}>
                <div className="settings-group-heading"><SectionHeading label={group.key === "thresholds" ? "01" : group.key === "ratios" ? "02" : "03"} title={copy.groups[group.key].title} description={copy.groups[group.key].description} /><CircleGauge size={20} aria-hidden="true" /></div>
                <FieldSet className="settings-field-grid">
                  {group.fields.map((field) => <Field key={field.key}>
                    <FieldLabel htmlFor={field.key}>{copy.fields[field.key].label}</FieldLabel>
                    <div className="settings-input-wrap"><Input id={field.key} type="number" min="0" max={group.percent ? "1" : undefined} step={field.step ?? "0.01"} value={String(activeParameters[field.key])} onChange={(event) => updateValue(field.key, event.target.value)} aria-invalid={group.percent && (activeParameters[field.key] < 0 || activeParameters[field.key] > 1) ? true : undefined} /><span>{group.percent ? copy.form.ratioRange : ""}</span></div>
                    <FieldDescription>{copy.fields[field.key].help}</FieldDescription>
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
