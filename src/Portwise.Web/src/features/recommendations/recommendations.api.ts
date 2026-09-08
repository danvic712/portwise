import { apiGet, apiPost } from "@/lib/api-client"
import type { BudgetSummary, PortfolioRecommendationResult, StockWatchlistItem } from "@/lib/api-types"

export async function getRecommendations() {
  return apiGet<PortfolioRecommendationResult>("/api/v1/recommendations")
}

export async function getWatchlist() {
  return apiGet<StockWatchlistItem[]>("/api/v1/stocks")
}

export async function getBudgetSummary() {
  return apiGet<BudgetSummary>("/api/v1/budgets/summary")
}

export async function createRecommendationSnapshot() {
  return apiPost("/api/v1/recommendations/snapshots")
}
