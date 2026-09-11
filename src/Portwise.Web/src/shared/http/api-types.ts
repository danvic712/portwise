import type { ApiSchema } from "@/shared/http/api-contract"

export type SetupStatus = ApiSchema<"SetupStatus">
export type SetupStockRequest = ApiSchema<"SetupStockRequest">
export type SetupRequest = ApiSchema<"SetupRequest">
export type InitialHoldingInput = ApiSchema<"InitialHoldingInput">
export type SetupResult = ApiSchema<"SetupResult">
export type StockHoldingSnapshot = ApiSchema<"StockHoldingSnapshot">
export type StockWatchlistItem = ApiSchema<"StockWatchlistItem">
export type StockAnalysisResult = ApiSchema<"StockAnalysisResult">
export type StockRecommendationResult = ApiSchema<"StockRecommendationResult">
export type PortfolioRecommendationResult = ApiSchema<"PortfolioRecommendationResult">
export type StockModelParameterSet = ApiSchema<"StockModelParameterSet">
export type SaveStockModelParametersRequest = ApiSchema<"SaveStockModelParametersRequest">
export type BudgetSummary = ApiSchema<"BudgetSummary">
export type RecordCashLedgerEntryRequest = ApiSchema<"RecordCashLedgerEntryRequest">
export type CashLedgerEntryResult = ApiSchema<"CashLedgerEntryResult">
export type RecordPortfolioTradeRequest = ApiSchema<"RecordPortfolioTradeRequest">
export type PortfolioTradeResult = ApiSchema<"PortfolioTradeResult">
export type StockDataSyncRunResult = ApiSchema<"StockDataSyncRunResult">
export type StockDataSyncFailure = ApiSchema<"StockDataSyncFailure">
