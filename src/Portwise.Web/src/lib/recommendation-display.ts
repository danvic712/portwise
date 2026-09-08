import type { StockAnalysisResult } from "@/lib/api-types"

export type RecommendationTone = "strong" | "add" | "hold" | "reduce"
export type RecommendationKind = "buy" | "trim" | "review" | "hold"

type RecommendationLabelKey = "strongBuy" | "accumulate" | "partialTrim" | "aggressiveTrim" | "reEvaluate" | "hold" | "unknown"
type RecommendationHeadlineKey = "strongBuy" | "accumulate" | "trim" | "reEvaluate" | "noAction" | "default"

export type RecommendationDisplayLabels = {
  label?: Record<string, string>
  headline?: Record<string, { lead: string; accent: string }>
  signalTitle?: Record<string, string>
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

type PriceZoneLabels = Record<string, { label: string; copy: string }>

export type PriceZone = {
  code: string
  label: string
  price: number | null
  copy: string
  className: string
}

const defaultLabels: Record<RecommendationLabelKey, string> = {
  strongBuy: "强买入",
  accumulate: "分批加仓",
  partialTrim: "减仓候选",
  aggressiveTrim: "激进减仓",
  reEvaluate: "需要复核",
  hold: "暂不操作",
  unknown: "等待数据",
}

const defaultHeadlines: Record<RecommendationHeadlineKey, { lead: string; accent: string }> = {
  strongBuy: { lead: "可以", accent: "试买" },
  accumulate: { lead: "可以", accent: "加仓" },
  trim: { lead: "考虑", accent: "减仓" },
  reEvaluate: { lead: "需要", accent: "复核" },
  noAction: { lead: "暂不", accent: "动作" },
  default: { lead: "先", accent: "观察" },
}

const defaultSignalTitles: Record<RecommendationHeadlineKey, string> = {
  strongBuy: "价格进入试探区间",
  accumulate: "收益率回到可接受区间",
  trim: "收益率偏低，先保护仓位",
  reEvaluate: "资料需要重新确认",
  noAction: "当前没有需要执行的动作",
  default: "等待收益率重新抬升",
}

export function recommendationPresentation(
  code: string | null | undefined,
  labels: RecommendationDisplayLabels = {},
): RecommendationPresentation {
  const normalizedCode = code?.trim().toLowerCase() ?? ""
  const classification = classifyRecommendation(normalizedCode)
  const headline = labels.headline?.[classification.headlineKey] ?? defaultHeadlines[classification.headlineKey]

  return {
    code: normalizedCode,
    tone: classification.tone,
    kind: classification.kind,
    isSell: classification.kind === "trim",
    labelKey: classification.labelKey,
    headlineKey: classification.headlineKey,
    label: labels.label?.[classification.labelKey] ?? defaultLabels[classification.labelKey],
    headline,
    signalTitle: labels.signalTitle?.[classification.headlineKey] ?? defaultSignalTitles[classification.headlineKey],
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
  labels?: PriceZoneLabels,
): PriceZone[] {
  return [
    { code: "strong_buy", label: labels?.strongBuy?.label ?? "重点买入", price: analysis.strongBuyPrice, copy: labels?.strongBuy?.copy ?? "估值和收益率同时有吸引力，可买入较大一部分。", className: "zone-buy" },
    { code: "accumulate", label: labels?.accumulate?.label ?? "分批加仓", price: analysis.accumulatePrice, copy: labels?.accumulate?.copy ?? "接近目标区间，建议买入小部分，保留现金。", className: "zone-accumulate" },
    { code: "hold", label: labels?.hold?.label ?? "观察持有", price: analysis.closePrice, copy: labels?.hold?.copy ?? "已有仓位可继续持有，新资金暂缓。", className: "zone-hold" },
    { code: "partial_trim", label: labels?.partialTrim?.label ?? "减仓候选", price: analysis.partialTrimPrice, copy: labels?.partialTrim?.copy ?? "收益率偏低，仅在偏离目标较多时考虑减仓。", className: "zone-trim" },
    { code: "aggressive_trim", label: labels?.aggressiveTrim?.label ?? "激进减仓", price: analysis.aggressiveTrimPrice, copy: labels?.aggressiveTrim?.copy ?? "只在仓位与规则都允许时考虑。", className: "zone-aggressive" },
  ]
}

export function currentPriceZoneLabel(
  analysis: Pick<StockAnalysisResult, "priceZoneCode">,
  labels?: Record<string, string>,
) {
  const code = analysis.priceZoneCode ?? ""
  return labels?.[code] ?? ({
    strong_buy: "重点买入",
    accumulate: "分批加仓",
    hold: "观察持有",
    partial_trim: "减仓候选",
    aggressive_trim: "激进减仓",
  }[code] ?? "待确认")
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
