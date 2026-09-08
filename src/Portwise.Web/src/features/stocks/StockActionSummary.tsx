import { ArrowDownRight, ArrowUpRight, ShieldCheck } from "lucide-react"

import { interpolate, type OverviewCopy } from "@/lib/i18n"
import { currentPriceZoneLabel, recommendationPresentation } from "@/lib/recommendation-display"
import type { StockAnalysisResult } from "@/lib/api-types"

type StockActionLabels = Pick<OverviewCopy["decision"], "actionLabel" | "headlines" | "signalTitles" | "priceZones" | "inZone">

export function StockActionSummary({ analysis, labels }: { analysis: StockAnalysisResult; labels: StockActionLabels }) {
  const presentation = recommendationPresentation(analysis.recommendationCode, { headline: labels.headlines, signalTitle: labels.signalTitles })
  const tone = presentation.tone
  const headline = presentation.headline
  const zone = currentPriceZoneLabel(analysis, labels.priceZones)
  const Icon = tone === "reduce" ? ArrowDownRight : tone === "hold" ? ShieldCheck : ArrowUpRight

  return (
    <section className={`stock-action-summary stock-action-summary-${tone}`} aria-live="polite" aria-label={labels.actionLabel}>
      <div className="stock-action-summary-mark" aria-hidden="true"><Icon size={18} /></div>
      <div className="stock-action-summary-copy">
        <span>{labels.actionLabel}</span>
        <h3>{headline.lead}<em>{headline.accent}</em></h3>
        <p>{presentation.signalTitle}</p>
      </div>
      <div className="stock-action-summary-zone">
        <span>{interpolate(labels.inZone, { zone })}</span>
        <strong>{zone}</strong>
      </div>
    </section>
  )
}
