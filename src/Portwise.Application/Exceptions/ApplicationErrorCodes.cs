namespace Portwise.Application.Exceptions;

public static class ApplicationErrorCodes
{
    public const string SetupValidationFailed = "setup_validation_failed";
    public const string SetupAlreadyCompleted = "setup_already_completed";
    public const string SetupNotCompleted = "setup_not_completed";
    public const string PreferencesNotConfigured = "preferences_not_configured";
    public const string PreferencesValidationFailed = "preferences_validation_failed";
    public const string PreferencesRevisionConflict = "preferences_revision_conflict";
    public const string StockDataProviderValidationFailed = "stock_data_provider_validation_failed";
    public const string StockDataProviderDefinitionUnavailable = "stock_data_provider_definition_unavailable";
    public const string StockDataProviderAlreadyExists = "stock_data_provider_already_exists";
    public const string StockDataProviderNotFound = "stock_data_provider_not_found";
    public const string StockDataProviderRevisionConflict = "stock_data_provider_revision_conflict";
    public const string StockDataProviderInUse = "stock_data_provider_in_use";
    public const string StockDataRouteValidationFailed = "stock_data_route_validation_failed";
    public const string StockDataRouteRevisionConflict = "stock_data_route_revision_conflict";
    public const string InferenceProviderValidationFailed = "inference_provider_validation_failed";
    public const string InferenceProviderNameConflict = "inference_provider_name_conflict";
    public const string InferenceProviderNotFound = "inference_provider_not_found";
    public const string InferenceProviderRevisionConflict = "inference_provider_revision_conflict";
    public const string InferenceProviderInUse = "inference_provider_in_use";
    public const string InferenceRouteValidationFailed = "inference_route_validation_failed";
    public const string InferenceRouteRevisionConflict = "inference_route_revision_conflict";
    public const string ModelParameterValidationFailed = "model_parameter_validation_failed";
    public const string ModelParameterVersionAlreadyExists = "model_parameter_version_already_exists";
    public const string StockAnalysisValidationFailed = "stock_analysis_validation_failed";
    public const string StockDataSyncValidationFailed = "stock_data_sync_validation_failed";
    public const string StockDataProviderUnavailable = "stock_data_provider_unavailable";
    public const string StockDataUnavailable = "stock_data_unavailable";
    public const string StockMarketDataUnavailable = "stock_market_data_unavailable";
    public const string StockDividendDataUnavailable = "stock_dividend_data_unavailable";
    public const string StockFinancialDataUnavailable = "stock_financial_data_unavailable";
    public const string StockNotConfigured = "stock_not_configured";
    public const string BudgetValidationFailed = "budget_validation_failed";
    public const string CashLedgerEntryConflict = "cash_ledger_entry_conflict";
    public const string PortfolioTradeValidationFailed = "portfolio_trade_validation_failed";
    public const string PortfolioPositionMissingForTrade = "portfolio_position_missing_for_trade";
    public const string PortfolioTradeConflict = "portfolio_trade_conflict";
    public const string Unknown = "application_error_unknown";

    public static IReadOnlyList<string> All { get; } =
    [
        SetupValidationFailed,
        SetupAlreadyCompleted,
        SetupNotCompleted,
        PreferencesNotConfigured,
        PreferencesValidationFailed,
        PreferencesRevisionConflict,
        StockDataProviderValidationFailed,
        StockDataProviderDefinitionUnavailable,
        StockDataProviderAlreadyExists,
        StockDataProviderNotFound,
        StockDataProviderRevisionConflict,
        StockDataProviderInUse,
        StockDataRouteValidationFailed,
        StockDataRouteRevisionConflict,
        InferenceProviderValidationFailed,
        InferenceProviderNameConflict,
        InferenceProviderNotFound,
        InferenceProviderRevisionConflict,
        InferenceProviderInUse,
        InferenceRouteValidationFailed,
        InferenceRouteRevisionConflict,
        ModelParameterValidationFailed,
        ModelParameterVersionAlreadyExists,
        StockAnalysisValidationFailed,
        StockDataSyncValidationFailed,
        StockDataProviderUnavailable,
        StockDataUnavailable,
        StockMarketDataUnavailable,
        StockDividendDataUnavailable,
        StockFinancialDataUnavailable,
        StockNotConfigured,
        BudgetValidationFailed,
        CashLedgerEntryConflict,
        PortfolioTradeValidationFailed,
        PortfolioPositionMissingForTrade,
        PortfolioTradeConflict
    ];

    public static IReadOnlyList<string> ExpectedStockSyncFailures { get; } =
    [
        StockDataUnavailable,
        StockMarketDataUnavailable,
        StockDividendDataUnavailable,
        StockFinancialDataUnavailable,
        StockDataSyncValidationFailed,
        StockNotConfigured
    ];
}
