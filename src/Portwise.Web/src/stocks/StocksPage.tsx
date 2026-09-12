import { Database, RefreshCw } from "lucide-react"
import { useCallback, useEffect, useRef, useState } from "react"

import { EmptyState, ErrorState, LoadingState } from "@/components/feedback/AsyncState"
import { PageFrame } from "@/components/layout/PageFrame"
import { PageTitle, SectionHeading } from "@/components/layout/PageHeading"
import { PriceLadder, PriceZoneBoard } from "@/components/stock/PriceLadder"
import { StockSelector } from "@/components/stock/StockSelector"
import { StockActionSummary } from "@/stocks/StockActionSummary"
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/Alert"
import { Badge } from "@/components/ui/Badge"
import { Button } from "@/components/ui/Button"
import { Card, CardContent, CardHeader } from "@/components/ui/Card"
import { Separator } from "@/components/ui/Separator"
import { getApiErrorMessage } from "@/shared/http/api-errors"
import { interpolate, useLocale } from "@/shared/i18n/i18n"
import { displayStockName, exchangeLabel } from "@/shared/display/stock-display"
import { hasAnalysisData, localizeRecommendationExplanation } from "@/shared/display/recommendation-display"
import { formatDate, formatDateTime, formatMoney, formatPercent, stockKey } from "@/shared/utils/utils"
import type { StockAnalysisResult, StockDataSyncRunResult, StockModelParameterSet, StockWatchlistItem } from "@/shared/http/api-types"
import { isRequestAborted, useLatestRequest } from "@/shared/hooks/useLatestRequest"
import { getStockModelParameters, getStockAnalysis, getWatchedStocks, syncStocks } from "@/stocks/stocks.api"
import { StockDetailSkeleton } from "@/stocks/StockDetailSkeleton"
import "./stocks.css"

export function StocksPage({ onNavigate }: { onNavigate: (path: string) => void }) {
  const { messages } = useLocale()
  const copy = messages.stocks.ui
  const [stocks, setStocks] = useState<StockWatchlistItem[]>([])
  const [selectedKey, setSelectedKey] = useState<string | null>(null)
  const [analysis, setAnalysis] = useState<StockAnalysisResult | null>(null)
  const [parameters, setParameters] = useState<StockModelParameterSet | null>(null)
  const [loading, setLoading] = useState(true)
  const [detailLoading, setDetailLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [detailError, setDetailError] = useState<string | null>(null)
  const [syncing, setSyncing] = useState(false)
  const [syncResult, setSyncResult] = useState<StockDataSyncRunResult | null>(null)
  const [detailRefreshVersion, setDetailRefreshVersion] = useState(0)
  const { begin: beginStocks } = useLatestRequest()
  const { begin: beginDetail } = useLatestRequest()
  const { begin: beginSync } = useLatestRequest()
  const stocksRef = useRef<StockWatchlistItem[]>([])
  const readErrorRef = useRef(copy.states.readError)
  const analysisErrorRef = useRef(copy.states.analysisError)
  const detailStageRef = useRef<HTMLDivElement>(null)
  const [detailMinHeight, setDetailMinHeight] = useState<number | null>(null)

  useEffect(() => {
    readErrorRef.current = copy.states.readError
    analysisErrorRef.current = copy.states.analysisError
  }, [copy.states.analysisError, copy.states.readError])

  const applyStocks = useCallback((list: StockWatchlistItem[]) => {
    stocksRef.current = list
    setStocks(list)
    setSelectedKey((current) => current && list.some((stock) => stockKey(stock) === current) ? current : list[0] ? stockKey(list[0]) : null)
  }, [])

  const loadStocks = useCallback(async ({ preserveView = false }: { preserveView?: boolean } = {}) => {
    const request = beginStocks()
    if (!preserveView) setLoading(true)
    setError(null)
    try {
      const list = await getWatchedStocks(request.signal)
      if (!request.isCurrent()) return
      applyStocks(list)
    } catch (loadError) {
      if (request.isCurrent() && !isRequestAborted(loadError, request.signal)) setError(getApiErrorMessage(loadError, readErrorRef.current, messages.common.ui.errors))
    } finally {
      if (!preserveView && request.isCurrent()) setLoading(false)
    }
  }, [applyStocks, beginStocks, messages.common.ui.errors])

  useEffect(() => {
    const timeoutId = window.setTimeout(() => { void loadStocks() }, 0)
    return () => window.clearTimeout(timeoutId)
  }, [loadStocks])

  const loadDetail = useCallback(async () => {
    const request = beginDetail()
    const stock = stocksRef.current.find((item) => stockKey(item) === selectedKey)
    if (!stock) {
      if (request.isCurrent()) {
        setAnalysis(null)
        setParameters(null)
        setDetailLoading(false)
      }
      return
    }

    setDetailLoading(true)
    setDetailMinHeight((current) => current ?? detailStageRef.current?.getBoundingClientRect().height ?? null)
    setDetailError(null)
    try {
      const [analysisResult, parameterResult] = await Promise.all([
        getStockAnalysis(stock.securityCode, stock.exchangeCode, request.signal),
        getStockModelParameters(stock.securityCode, stock.exchangeCode, request.signal),
      ])
      if (request.isCurrent()) {
        setAnalysis(analysisResult.analysis)
        setParameters(parameterResult)
      }
    } catch (detailLoadError) {
      if (request.isCurrent() && !isRequestAborted(detailLoadError, request.signal)) {
        setAnalysis(null)
        setParameters(null)
        setDetailError(getApiErrorMessage(detailLoadError, analysisErrorRef.current, messages.common.ui.errors))
      }
    } finally {
      if (request.isCurrent()) setDetailLoading(false)
    }
  }, [beginDetail, messages.common.ui.errors, selectedKey])

  useEffect(() => {
    if (detailLoading || detailMinHeight === null) return

    const timeoutId = window.setTimeout(() => setDetailMinHeight(null), 420)
    return () => window.clearTimeout(timeoutId)
  }, [detailLoading, detailMinHeight])

  useEffect(() => {
    const timeoutId = window.setTimeout(() => { void loadDetail() }, 0)
    return () => window.clearTimeout(timeoutId)
  }, [detailRefreshVersion, loadDetail])

  async function syncAll() {
    const request = beginSync()
    setSyncing(true)
    setSyncResult(null)
    setError(null)
    try {
      const result = await syncStocks(request.signal)
      if (!request.isCurrent()) return
      setSyncResult(result)
      await loadStocks({ preserveView: true })
      if (request.isCurrent()) setDetailRefreshVersion((current) => current + 1)
    } catch (syncError) {
      if (request.isCurrent() && !isRequestAborted(syncError, request.signal)) setError(getApiErrorMessage(syncError, copy.states.syncError, messages.common.ui.errors))
    } finally {
      if (request.isCurrent()) setSyncing(false)
    }
  }

  function selectStock(stock: StockWatchlistItem) {
    const nextKey = stockKey(stock)
    if (nextKey === selectedKey) return

    setDetailMinHeight((current) => current ?? detailStageRef.current?.getBoundingClientRect().height ?? null)
    setSelectedKey(nextKey)
    setAnalysis(null)
    setParameters(null)
    setDetailError(null)
    setDetailLoading(true)
  }

  if (loading) {
    return <PageFrame currentPath="/stocks" onNavigate={onNavigate} dataState="pending" contentClassName="stocks-page-wrap"><LoadingState label={copy.states.loadingList} /></PageFrame>
  }

  if (error && !stocks.length) {
    return <PageFrame currentPath="/stocks" onNavigate={onNavigate} dataState="unknown" contentClassName="stocks-page-wrap"><ErrorState message={error} onRetry={() => void loadStocks()} /></PageFrame>
  }

  if (!stocks.length) {
    return <PageFrame currentPath="/stocks" onNavigate={onNavigate} dataState="unknown" contentClassName="stocks-page-wrap"><EmptyState title={copy.states.emptyTitle} description={copy.states.emptyDescription} action={<Button onClick={() => onNavigate("/onboarding")}>{copy.actions.goSetup}</Button>} /></PageFrame>
  }

  const selectedStock = stocks.find((stock) => stockKey(stock) === selectedKey) ?? stocks[0]
  const analysisReady = hasAnalysisData(analysis)
  const lastUpdated = analysis?.computedAt ? formatDateTime(analysis.computedAt) : null
  const selectedRecommendation = analysis
    ? [{ analysis, suggestedBuyShares: 0, suggestedSellShares: 0, suggestedTradeAmount: 0, estimatedTransactionFeeAmount: 0 }]
    : []

  return (
    <PageFrame currentPath="/stocks" onNavigate={onNavigate} lastUpdated={lastUpdated} dataState={analysisReady ? "synced" : "pending"} contentClassName="stocks-page-wrap">
      <PageTitle
        eyebrow={copy.page.eyebrow}
        title={copy.page.title}
        description={copy.page.description}
        actions={<Button onClick={() => void syncAll()} disabled={syncing}>{syncing ? <><RefreshCw className="spin" data-icon="inline-start" />{copy.actions.syncing}</> : <><RefreshCw data-icon="inline-start" />{copy.actions.syncAll}</>}</Button>}
      />

      {error && <Alert variant="destructive" className="d-inline-alert"><AlertTitle>{copy.states.dataWarningTitle}</AlertTitle><AlertDescription>{error}</AlertDescription></Alert>}

      <div className="stocks-layout">
        <Card className="stock-directory">
          <CardHeader className="stock-directory-header">
            <SectionHeading label={copy.page.watchlistLabel} title={interpolate(copy.page.watchlistTitle, { count: stocks.length })} description={copy.page.watchlistDescription} />
          </CardHeader>
          <CardContent>
            <StockSelector
              stocks={stocks}
              recommendations={selectedRecommendation}
              selectedKey={selectedKey}
              onSelect={selectStock}
              labels={{
                kicker: copy.selector.kicker,
                title: copy.selector.title,
                description: interpolate(copy.selector.description, { count: stocks.length }),
                selectionHint: copy.selector.selectionHint,
                listLabel: copy.selector.listLabel,
                pending: copy.selector.pending,
                sectorUnset: copy.selector.sectorUnset,
                recommendationLabels: { label: copy.selector.signals, headline: messages.dividendStrategy.ui.overview.decision.headlines, signalTitle: messages.dividendStrategy.ui.overview.decision.signalTitles },
                pendingName: copy.identity.pendingName,
              }}
            />
          </CardContent>
        </Card>

        <Card className="stock-detail">
          <CardContent className="stock-detail-content">
            <div ref={detailStageRef} className="stock-detail-stage" style={detailMinHeight ? { minHeight: detailMinHeight } : undefined}>
              <div className={`stock-detail-scene ${detailLoading ? "stock-detail-scene-loading" : "stock-detail-scene-ready"}`} key={`${selectedKey ?? "empty"}-${detailLoading ? "loading" : analysis?.computedAt ?? "pending"}`}>
                {detailLoading ? <StockDetailSkeleton label={copy.states.loadingAnalysis} /> : analysisReady && analysis ? <StockDetail analysis={analysis} parameters={parameters} onNavigate={onNavigate} /> : <StockDetailPending stock={selectedStock} message={detailError ?? (analysis ? copy.states.pendingMessage : null)} onSync={() => void syncAll()} />}
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {syncResult && <section className="sync-results"><Alert variant={syncResult.partiallyFailedStockCount ? "attention" : "default"}><AlertTitle><Database className="sync-results-icon" />{copy.states.syncComplete}</AlertTitle><AlertDescription><span>{interpolate(copy.sync.attempted, { count: syncResult.attemptedStockCount, complete: syncResult.fullyCompletedStockCount, failed: syncResult.partiallyFailedStockCount })}</span>{syncResult.failures.length > 0 && <span> {interpolate(copy.sync.failures, { items: syncResult.failures.map((failure) => `${failure.securityCode} ${failure.dataKind}`).join(", ") })}</span>}</AlertDescription></Alert></section>}
    </PageFrame>
  )
}

function StockDetail({ analysis, parameters, onNavigate }: { analysis: StockAnalysisResult; parameters: StockModelParameterSet | null; onNavigate: (path: string) => void }) {
  const { locale, messages } = useLocale()
  const copy = messages.stocks.ui
  const decisionCopy = messages.dividendStrategy.ui.overview.decision
  const name = displayStockName(analysis, copy.identity.pendingName)
  const identity = interpolate(copy.detail.dataIdentity, { code: analysis.securityCode, exchange: analysis.exchangeCode, exchangeLabel: exchangeLabel(analysis.exchangeCode, copy.identity.exchange), date: formatDate(analysis.dataAsOfDate) })

  return (
    <>
      <div className="stock-detail-header">
        <div>
          <div className="d-kicker"><span>01</span><span>{copy.detail.readingLabel}</span><i /></div>
          <div className="stock-detail-identity"><div className="d-stock-avatar">{name.slice(0, 2)}</div><div><h2 className="stock-detail-name">{name}</h2><p className="stock-detail-code">{identity}</p></div></div>
        </div>
        <Badge variant={analysis.priceZoneConfirmed ? "accent" : "attention"}>{analysis.priceZoneConfirmed ? copy.detail.confirmed : copy.detail.unconfirmed}</Badge>
      </div>

      <StockActionSummary analysis={analysis} labels={{ ...decisionCopy, recommendationLabels: messages.dividendStrategy.ui.overview.stockSelector.signals }} />

      <div className="stock-analysis-grid">
        <AnalysisTile label={copy.detail.metrics.closePrice} value={formatMoney(analysis.closePrice)} />
        <AnalysisTile label={copy.detail.metrics.dividendYield} value={formatPercent(analysis.dividendYield)} />
        <AnalysisTile label={copy.detail.metrics.modelDividend} value={formatMoney(analysis.modelDividendPerShare)} />
        <AnalysisTile label={copy.detail.metrics.heldShares} value={analysis.heldShares.toLocaleString(locale)} />
        <AnalysisTile label={copy.detail.metrics.coreShares} value={analysis.coreShares.toLocaleString(locale)} />
        <AnalysisTile label={copy.detail.metrics.satelliteShares} value={analysis.satelliteShares.toLocaleString(locale)} />
      </div>

      <Separator />
      <section className="stock-detail-section">
        <div className="d-section-head">
          <div><div className="d-kicker"><span>02</span><span>{copy.detail.priceSectionLabel}</span><i /></div><h2>{copy.detail.priceSectionTitle}</h2><p>{copy.detail.priceSectionDescription}</p></div>
          <div className="d-current-price"><span>{copy.detail.currentPrice}</span><strong>{formatMoney(analysis.closePrice)}</strong></div>
        </div>
        <Card className="stock-ladder-card"><CardContent><PriceLadder analysis={analysis} /><PriceZoneBoard analysis={analysis} /></CardContent></Card>
      </section>

      <Separator />
      <section className="stock-detail-section">
        <SectionHeading label={copy.detail.parametersLabel} title={copy.detail.parametersTitle} description={copy.detail.parametersDescription} />
        {parameters ? <div className="parameter-grid">{parameterEntries(parameters, copy.detail.parameterNames).map(([label, value]) => <div className="parameter-item" key={label}><span>{label}</span><strong>{value}</strong></div>)}</div> : <EmptyState title={copy.detail.missingParametersTitle} description={copy.detail.missingParametersDescription} action={<Button variant="outline" onClick={() => onNavigate(`/settings?stock=${encodeURIComponent(analysis.securityCode)}&exchange=${encodeURIComponent(analysis.exchangeCode)}`)}>{copy.actions.configure}</Button>} />}
        <div className="note-box">{localizeRecommendationExplanation(analysis.explanation, decisionCopy.explanations, decisionCopy.priceZones, copy.states.explanationFallback)}</div>
      </section>
    </>
  )
}

function AnalysisTile({ label, value }: { label: string; value: string }) {
  return <div className="analysis-tile"><span>{label}</span><strong>{value}</strong></div>
}

function StockDetailPending({ stock, message, onSync }: { stock: StockWatchlistItem; message: string | null; onSync: () => void }) {
  const { messages } = useLocale()
  const copy = messages.stocks.ui
  const name = displayStockName(stock, copy.identity.pendingName)
  const identity = interpolate(copy.detail.pendingIdentity, { code: stock.securityCode, exchange: stock.exchangeCode })

  return (
    <div className="stock-detail-pending">
      <div className="d-kicker"><span>01</span><span>{copy.detail.readingLabel}</span><i /></div>
      <div className="stock-detail-identity"><div className="d-stock-avatar">{name.slice(0, 2)}</div><div><h2 className="stock-detail-name">{name}</h2><p className="stock-detail-code">{identity}</p></div></div>
      <h3>{copy.states.pendingTitle}</h3>
      <p>{copy.states.pendingDescription}</p>
      {message && <p className="stock-pending-message">{message}</p>}
      <Button variant="outline" onClick={onSync}><RefreshCw data-icon="inline-start" />{copy.actions.resync}</Button>
    </div>
  )
}

function parameterEntries(parameters: StockModelParameterSet, labels: Record<string, string>) {
  return [
    [labels.modelVersion, parameters.modelVersion],
    [labels.strongBuyYield, formatPercent(parameters.strongBuyYieldThreshold)],
    [labels.accumulationYield, formatPercent(parameters.accumulationYieldThreshold)],
    [labels.partialTrimYield, formatPercent(parameters.partialTrimYieldThreshold)],
    [labels.aggressiveTrimYield, formatPercent(parameters.aggressiveTrimYieldThreshold)],
    [labels.maxSecurityWeight, formatPercent(parameters.maxSecurityWeight)],
    [labels.maxSectorWeight, formatPercent(parameters.maxSectorWeight)],
    [labels.cashReserveRatio, formatPercent(parameters.cashReserveRatio)],
    [labels.maxSingleTradeAmount, formatMoney(parameters.maxSingleTradeAmount)],
    [labels.tradingLotSize, `${parameters.tradingLotSize} ${labels.tradingLotSuffix}`],
    [labels.effectiveFromDate, formatDate(parameters.effectiveFromDate)],
  ] as const
}
