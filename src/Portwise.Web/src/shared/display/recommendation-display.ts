import type { StockAnalysisResult } from "@/shared/http/api-types"

export type RecommendationTone = "strong" | "add" | "hold" | "reduce"
export type RecommendationKind = "buy" | "trim" | "review" | "hold"

type RecommendationLabelKey = "strongBuy" | "accumulate" | "partialTrim" | "aggressiveTrim" | "reEvaluate" | "hold" | "unknown"
type RecommendationHeadlineKey = "strongBuy" | "accumulate" | "trim" | "reEvaluate" | "noAction" | "default"

export type RecommendationDisplayLabels = {
  label: Record<string, string>
  headline: Record<string, { lead: string; accent: string }>
  signalTitle: Record<string, string>
}

export type RecommendationPresentation = {
  code: string
  tone: RecommendationTone
  kind: RecommendationKind
  isSell: boolean
  labelKey: RecommendationLabelKey
  headlineKey: RecommendationHeadlineKey
  label: string
  headline: { lead: string; accent: string }
  signalTitle: string
}

export type PriceZoneLabels = Record<string, { label: string; copy: string }>

export type PriceZone = {
  code: string
  label: string
  price: number | null
  copy: string
  className: string
}

export function localizeRecommendationExplanation(
  explanation: string | null | undefined,
  explanations: Record<string, string>,
  priceZones: Record<string, string>,
  fallback: string,
) {
  if (!explanation) {
    return fallback
  }

  return explanation
    .split("|")
    .map((part) => {
      const [code, parameter] = part.split(":", 2)
      const template = explanations[code]
      if (!template) {
        return fallback
      }

      return template.replace("{zone}", parameter ? priceZones[parameter] ?? parameter : "")
    })
    .join(" ")
}

export function recommendationPresentation(
  code: string | null | undefined,
  labels: RecommendationDisplayLabels,
): RecommendationPresentation {
  const normalizedCode = code?.trim().toLowerCase() ?? ""
  const classification = classifyRecommendation(normalizedCode)
  const headline = labels.headline[classification.headlineKey]

  return {
    code: normalizedCode,
    tone: classification.tone,
    kind: classification.kind,
    isSell: classification.kind === "trim",
    labelKey: classification.labelKey,
    headlineKey: classification.headlineKey,
    label: labels.label[classification.labelKey],
    headline,
    signalTitle: labels.signalTitle[classification.headlineKey],
  }
}

export function hasAnalysisData(
  analysis: Pick<StockAnalysisResult, "modelStatusCode" | "closePrice" | "modelDividendPerShare" | "priceZoneConfirmed"> | null | undefined,
) {
  return Boolean(
    analysis
      && analysis.modelStatusCode.trim().toLowerCase() !== "unavailable"
      && analysis.closePrice !== null
      && analysis.modelDividendPerShare !== null
      && analysis.priceZoneConfirmed,
  )
}

export function getPriceZones(
  analysis: Pick<StockAnalysisResult, "strongBuyPrice" | "accumulatePrice" | "closePrice" | "partialTrimPrice" | "aggressiveTrimPrice">,
  labels: PriceZoneLabels,
): PriceZone[] {
  return [
    { code: "strong_buy", label: labels.strongBuy.label, price: analysis.strongBuyPrice, copy: labels.strongBuy.copy, className: "zone-buy" },
    { code: "accumulate", label: labels.accumulate.label, price: analysis.accumulatePrice, copy: labels.accumulate.copy, className: "zone-accumulate" },
    { code: "hold", label: labels.hold.label, price: analysis.closePrice, copy: labels.hold.copy, className: "zone-hold" },
    { code: "partial_trim", label: labels.partialTrim.label, price: analysis.partialTrimPrice, copy: labels.partialTrim.copy, className: "zone-trim" },
    { code: "aggressive_trim", label: labels.aggressiveTrim.label, price: analysis.aggressiveTrimPrice, copy: labels.aggressiveTrim.copy, className: "zone-aggressive" },
  ]
}

export function currentPriceZoneLabel(
  analysis: Pick<StockAnalysisResult, "priceZoneCode">,
  labels: Record<string, string>,
) {
  const code = analysis.priceZoneCode ?? ""
  return labels[code] ?? labels.pending
}

function classifyRecommendation(code: string): Omit<RecommendationPresentation, "code" | "isSell" | "label" | "headline" | "signalTitle"> {
  switch (code) {
    case "strong_buy":
      return { tone: "strong", kind: "buy", labelKey: "strongBuy", headlineKey: "strongBuy" }
    case "accumulate":
      return { tone: "add", kind: "buy", labelKey: "accumulate", headlineKey: "accumulate" }
    case "partial_trim":
      return { tone: "reduce", kind: "trim", labelKey: "partialTrim", headlineKey: "trim" }
    case "aggressive_trim":
      return { tone: "reduce", kind: "trim", labelKey: "aggressiveTrim", headlineKey: "trim" }
    case "re_evaluate":
      return { tone: "hold", kind: "review", labelKey: "reEvaluate", headlineKey: "reEvaluate" }
    case "no_action":
      return { tone: "hold", kind: "hold", labelKey: "hold", headlineKey: "noAction" }
    default:
      return { tone: "hold", kind: "hold", labelKey: "unknown", headlineKey: "default" }
  }
}
