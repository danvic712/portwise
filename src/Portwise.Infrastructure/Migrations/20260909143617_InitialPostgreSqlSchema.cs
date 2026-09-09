using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portwise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSqlSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "portfolios",
                schema: "public",
                columns: table => new
                {
                    portfolio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    portfolio_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    portfolio_scope = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "default")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portfolios", x => x.portfolio_id);
                });

            migrationBuilder.CreateTable(
                name: "securities",
                schema: "public",
                columns: table => new
                {
                    security_id = table.Column<Guid>(type: "uuid", nullable: false),
                    security_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    exchange_code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    security_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    market_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    sector_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_securities", x => x.security_id);
                });

            migrationBuilder.CreateTable(
                name: "cash_ledger_entries",
                schema: "public",
                columns: table => new
                {
                    cash_ledger_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    portfolio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    security_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entry_date = table.Column<DateOnly>(type: "date", nullable: false),
                    entry_type_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    cash_direction_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    cash_amount = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    source_record_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cash_ledger_entries", x => x.cash_ledger_entry_id);
                    table.ForeignKey(
                        name: "fk_cash_ledger_entries_portfolios_portfolio_id",
                        column: x => x.portfolio_id,
                        principalSchema: "public",
                        principalTable: "portfolios",
                        principalColumn: "portfolio_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_cash_ledger_entries_securities_security_id",
                        column: x => x.security_id,
                        principalSchema: "public",
                        principalTable: "securities",
                        principalColumn: "security_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dividend_events",
                schema: "public",
                columns: table => new
                {
                    dividend_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    security_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dividend_per_share = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    dividend_type_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    dividend_status_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    announcement_date = table.Column<DateOnly>(type: "date", nullable: true),
                    ex_dividend_date = table.Column<DateOnly>(type: "date", nullable: true),
                    payment_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_special_dividend = table.Column<bool>(type: "boolean", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    captured_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    data_source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    source_record_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    data_quality_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dividend_events", x => x.dividend_event_id);
                    table.ForeignKey(
                        name: "fk_dividend_events_securities_security_id",
                        column: x => x.security_id,
                        principalSchema: "public",
                        principalTable: "securities",
                        principalColumn: "security_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "financial_snapshots",
                schema: "public",
                columns: table => new
                {
                    financial_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    security_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_as_of_date = table.Column<DateOnly>(type: "date", nullable: false),
                    captured_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    earnings_per_share = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: true),
                    dividend_payout_ratio = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: true),
                    three_year_average_dividend_payout_ratio = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: true),
                    price_to_book_ratio = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: true),
                    return_on_equity = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: true),
                    data_source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    source_record_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    data_quality_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_financial_snapshots", x => x.financial_snapshot_id);
                    table.ForeignKey(
                        name: "fk_financial_snapshots_securities_security_id",
                        column: x => x.security_id,
                        principalSchema: "public",
                        principalTable: "securities",
                        principalColumn: "security_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "model_parameter_sets",
                schema: "public",
                columns: table => new
                {
                    model_parameter_set_id = table.Column<Guid>(type: "uuid", nullable: false),
                    portfolio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    security_id = table.Column<Guid>(type: "uuid", nullable: false),
                    model_version = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    strong_buy_yield_threshold = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    accumulation_yield_threshold = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    partial_trim_yield_threshold = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    aggressive_trim_yield_threshold = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    strong_buy_budget_ratio = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    accumulate_budget_ratio = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    partial_trim_ratio = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    aggressive_trim_ratio = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    max_security_weight = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    max_sector_weight = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    cash_reserve_ratio = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    max_single_trade_amount = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    max_period_budget_amount = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    transaction_fee_ratio = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    minimum_transaction_fee_amount = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    trading_lot_size = table.Column<int>(type: "integer", nullable: false),
                    effective_from_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_model_parameter_sets", x => x.model_parameter_set_id);
                    table.ForeignKey(
                        name: "fk_model_parameter_sets_portfolios_portfolio_id",
                        column: x => x.portfolio_id,
                        principalSchema: "public",
                        principalTable: "portfolios",
                        principalColumn: "portfolio_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_model_parameter_sets_securities_security_id",
                        column: x => x.security_id,
                        principalSchema: "public",
                        principalTable: "securities",
                        principalColumn: "security_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "portfolio_positions",
                schema: "public",
                columns: table => new
                {
                    portfolio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    security_id = table.Column<Guid>(type: "uuid", nullable: false),
                    held_shares = table.Column<int>(type: "integer", nullable: false),
                    core_shares = table.Column<int>(type: "integer", nullable: false),
                    target_shares = table.Column<int>(type: "integer", nullable: false),
                    average_cost_per_share = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portfolio_positions", x => new { x.portfolio_id, x.security_id });
                    table.ForeignKey(
                        name: "fk_portfolio_positions_portfolios_portfolio_id",
                        column: x => x.portfolio_id,
                        principalSchema: "public",
                        principalTable: "portfolios",
                        principalColumn: "portfolio_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_portfolio_positions_securities_security_id",
                        column: x => x.security_id,
                        principalSchema: "public",
                        principalTable: "securities",
                        principalColumn: "security_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "portfolio_trades",
                schema: "public",
                columns: table => new
                {
                    portfolio_trade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    portfolio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    security_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trade_date = table.Column<DateOnly>(type: "date", nullable: false),
                    trade_direction_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    share_quantity = table.Column<int>(type: "integer", nullable: false),
                    price_per_share = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    transaction_fee_amount = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    source_record_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portfolio_trades", x => x.portfolio_trade_id);
                    table.ForeignKey(
                        name: "fk_portfolio_trades_portfolios_portfolio_id",
                        column: x => x.portfolio_id,
                        principalSchema: "public",
                        principalTable: "portfolios",
                        principalColumn: "portfolio_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_portfolio_trades_securities_security_id",
                        column: x => x.security_id,
                        principalSchema: "public",
                        principalTable: "securities",
                        principalColumn: "security_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "price_observations",
                schema: "public",
                columns: table => new
                {
                    price_observation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    security_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trading_date = table.Column<DateOnly>(type: "date", nullable: false),
                    close_price = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    price_observed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    data_source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    source_record_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    data_quality_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_price_observations", x => x.price_observation_id);
                    table.ForeignKey(
                        name: "fk_price_observations_securities_security_id",
                        column: x => x.security_id,
                        principalSchema: "public",
                        principalTable: "securities",
                        principalColumn: "security_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "recommendation_snapshots",
                schema: "public",
                columns: table => new
                {
                    recommendation_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    model_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    portfolio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    security_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_as_of_date = table.Column<DateOnly>(type: "date", nullable: true),
                    close_price = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: true),
                    model_dividend_per_share = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: true),
                    dividend_mode_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    model_status_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    dividend_reliability_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    observed_price_zone_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    price_zone_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    price_zone_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    recommendation_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    dividend_yield = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: true),
                    suggested_buy_shares = table.Column<int>(type: "integer", nullable: false),
                    suggested_sell_shares = table.Column<int>(type: "integer", nullable: false),
                    suggested_trade_amount = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    estimated_transaction_fee_amount = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    computed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    model_parameter_set_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recommendation_snapshots", x => x.recommendation_snapshot_id);
                    table.ForeignKey(
                        name: "fk_recommendation_snapshots_model_parameter_set_id",
                        column: x => x.model_parameter_set_id,
                        principalSchema: "public",
                        principalTable: "model_parameter_sets",
                        principalColumn: "model_parameter_set_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_recommendation_snapshots_portfolios_portfolio_id",
                        column: x => x.portfolio_id,
                        principalSchema: "public",
                        principalTable: "portfolios",
                        principalColumn: "portfolio_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_recommendation_snapshots_securities_security_id",
                        column: x => x.security_id,
                        principalSchema: "public",
                        principalTable: "securities",
                        principalColumn: "security_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cash_ledger_entries_security_id",
                schema: "public",
                table: "cash_ledger_entries",
                column: "security_id");

            migrationBuilder.CreateIndex(
                name: "uq_cash_ledger_entries_portfolio_source_record",
                schema: "public",
                table: "cash_ledger_entries",
                columns: new[] { "portfolio_id", "source_record_id" },
                unique: true,
                filter: "source_record_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "uq_dividend_events_security_source_record",
                schema: "public",
                table: "dividend_events",
                columns: new[] { "security_id", "source_record_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_financial_snapshots_security_data_as_of_date",
                schema: "public",
                table: "financial_snapshots",
                columns: new[] { "security_id", "data_as_of_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_model_parameter_sets_security_id",
                schema: "public",
                table: "model_parameter_sets",
                column: "security_id");

            migrationBuilder.CreateIndex(
                name: "uq_model_parameter_sets_portfolio_security_effective_from",
                schema: "public",
                table: "model_parameter_sets",
                columns: new[] { "portfolio_id", "security_id", "effective_from_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_portfolio_positions_security_id",
                schema: "public",
                table: "portfolio_positions",
                column: "security_id");

            migrationBuilder.CreateIndex(
                name: "ix_portfolio_trades_portfolio_trade_date",
                schema: "public",
                table: "portfolio_trades",
                columns: new[] { "portfolio_id", "trade_date" });

            migrationBuilder.CreateIndex(
                name: "ix_portfolio_trades_security_id",
                schema: "public",
                table: "portfolio_trades",
                column: "security_id");

            migrationBuilder.CreateIndex(
                name: "uq_portfolio_trades_portfolio_source_record",
                schema: "public",
                table: "portfolio_trades",
                columns: new[] { "portfolio_id", "source_record_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_portfolios_portfolio_scope",
                schema: "public",
                table: "portfolios",
                column: "portfolio_scope",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_price_observations_security_trading_date",
                schema: "public",
                table: "price_observations",
                columns: new[] { "security_id", "trading_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_recommendation_snapshots_model_parameter_set_id",
                schema: "public",
                table: "recommendation_snapshots",
                column: "model_parameter_set_id");

            migrationBuilder.CreateIndex(
                name: "ix_recommendation_snapshots_portfolio_id",
                schema: "public",
                table: "recommendation_snapshots",
                column: "portfolio_id");

            migrationBuilder.CreateIndex(
                name: "ix_recommendation_snapshots_security_id",
                schema: "public",
                table: "recommendation_snapshots",
                column: "security_id");

            migrationBuilder.CreateIndex(
                name: "uq_recommendation_snapshots_model_run_portfolio_security",
                schema: "public",
                table: "recommendation_snapshots",
                columns: new[] { "model_run_id", "portfolio_id", "security_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_securities_exchange_security_code",
                schema: "public",
                table: "securities",
                columns: new[] { "exchange_code", "security_code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cash_ledger_entries",
                schema: "public");

            migrationBuilder.DropTable(
                name: "dividend_events",
                schema: "public");

            migrationBuilder.DropTable(
                name: "financial_snapshots",
                schema: "public");

            migrationBuilder.DropTable(
                name: "portfolio_positions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "portfolio_trades",
                schema: "public");

            migrationBuilder.DropTable(
                name: "price_observations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "recommendation_snapshots",
                schema: "public");

            migrationBuilder.DropTable(
                name: "model_parameter_sets",
                schema: "public");

            migrationBuilder.DropTable(
                name: "portfolios",
                schema: "public");

            migrationBuilder.DropTable(
                name: "securities",
                schema: "public");
        }
    }
}
