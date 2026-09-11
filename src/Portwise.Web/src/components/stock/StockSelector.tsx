import { Check, ChevronRight } from "lucide-react"

import { cn, formatMoney, stockKey } from "@/shared/utils/utils"
import { displayStockName } from "@/shared/display/stock-display"
import { hasAnalysisData, recommendationPresentation, type RecommendationDisplayLabels } from "@/shared/display/recommendation-display"
import type { StockRecommendationResult, StockWatchlistItem } from "@/shared/http/api-types"
import { Button } from "@/components/ui/Button"

type StockSelectorLabels = {
  kicker: string
  title: string
  description: string
  selectionHint: string
  pending: string
  listLabel: string
  sectorUnset: string
  recommendationLabels: RecommendationDisplayLabels
  pendingName: string
}

export function StockSelector({ stocks, selectedKey, onSelect, recommendations = [], labels }: { stocks: StockWatchlistItem[]; selectedKey: string | null; onSelect: (stock: StockWatchlistItem) => void; recommendations?: StockRecommendationResult[]; labels: StockSelectorLabels }) {
  return (
    <section className="d-watchlist">
      <div className="d-watchlist-heading"><div><div className="d-kicker"><span>00</span><span>{labels.kicker}</span><i /></div><h2>{labels.title}</h2></div><span className="d-watchlist-note"><strong>{labels.description}</strong>{labels.selectionHint && <small>{labels.selectionHint}</small>}</span></div>
      <div className="stock-rail d-stock-rail" role="group" aria-label={labels.listLabel}>
      {stocks.map((stock) => {
        const selected = stockKey(stock) === selectedKey
        const recommendation = recommendations.find((item) => stockKey(item.analysis) === stockKey(stock))
        const presentation = recommendation && hasAnalysisData(recommendation.analysis)
          ? recommendationPresentation(recommendation.analysis.recommendationCode, labels.recommendationLabels)
          : null
        const isReady = presentation !== null
        const statusLabel = presentation?.label ?? labels.pending
        const statusTone = presentation?.tone ?? "hold"
        const stockName = displayStockName(stock, labels.pendingName)
        const initials = stockName.slice(0, 2)
        return (
          <Button
            key={`${stock.securityCode}-${stock.exchangeCode}`}
            variant="ghost"
            className={cn("d-stock-tile", selected && "d-stock-tile-active")}
            type="button"
            onClick={() => onSelect(stock)}
            aria-pressed={selected}
            aria-label={`${stockName} ${stock.securityCode}.${stock.exchangeCode} · ${statusLabel}`}
          >
            <span className={cn("d-tile-avatar", `d-tile-${statusTone}`)}>{initials}</span>
            <span className="d-tile-name"><strong>{stockName}</strong><span>{stock.securityCode}.{stock.exchangeCode}</span></span>
            <span className="d-tile-quote"><strong>{formatMoney(recommendation?.analysis.closePrice)}</strong><span>{stock.sectorCode ?? labels.sectorUnset}</span></span>
            <span key={`${stockKey(stock)}-${statusLabel}`} className={cn("signal-badge", `signal-${statusTone}`, isReady ? "signal-ready" : "signal-pending")}>{statusLabel}</span>
            {selected ? <Check className="d-tile-arrow" size={15} /> : <ChevronRight className="d-tile-arrow" size={15} />}
          </Button>
        )
      })}
      </div>
    </section>
  )
}
