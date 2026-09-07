using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DividendHarvest.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialSchema : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "portfolios",
            columns: table => new
            {
                portfolio_id = table.Column<Guid>(type: "TEXT", nullable: false),
                portfolio_name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                currency_code = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_portfolios", x => x.portfolio_id);
            });

        migrationBuilder.CreateTable(
            name: "securities",
            columns: table => new
            {
                security_id = table.Column<Guid>(type: "TEXT", nullable: false),
                security_code = table.Column<string>(type: "TEXT", maxLength: 6, nullable: false),
                exchange_code = table.Column<string>(type: "TEXT", maxLength: 4, nullable: false),
                security_name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                market_code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                currency_code = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                sector_code = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_securities", x => x.security_id);
            });

        migrationBuilder.CreateTable(
            name: "cash_ledger_entries",
            columns: table => new
            {
                cash_ledger_entry_id = table.Column<Guid>(type: "TEXT", nullable: false),
                portfolio_id = table.Column<Guid>(type: "TEXT", nullable: false),
                security_id = table.Column<Guid>(type: "TEXT", nullable: true),
                entry_date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                entry_type_code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                cash_direction_code = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                cash_amount = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                source_record_id = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_cash_ledger_entries", x => x.cash_ledger_entry_id);
                table.ForeignKey(
                    name: "FK_cash_ledger_entries_portfolios_portfolio_id",
                    column: x => x.portfolio_id,
                    principalTable: "portfolios",
                    principalColumn: "portfolio_id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_cash_ledger_entries_securities_security_id",
                    column: x => x.security_id,
                    principalTable: "securities",
                    principalColumn: "security_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "dividend_events",
            columns: table => new
            {
                dividend_event_id = table.Column<Guid>(type: "TEXT", nullable: false),
                security_id = table.Column<Guid>(type: "TEXT", nullable: false),
                dividend_per_share = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                dividend_type_code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                dividend_status_code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                announcement_date = table.Column<DateOnly>(type: "TEXT", nullable: true),
                ex_dividend_date = table.Column<DateOnly>(type: "TEXT", nullable: true),
                payment_date = table.Column<DateOnly>(type: "TEXT", nullable: true),
                is_special_dividend = table.Column<bool>(type: "INTEGER", nullable: false),
                published_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                captured_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                data_source = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                source_record_id = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                data_quality_code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_dividend_events", x => x.dividend_event_id);
                table.ForeignKey(
                    name: "FK_dividend_events_securities_security_id",
                    column: x => x.security_id,
                    principalTable: "securities",
                    principalColumn: "security_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "financial_snapshots",
            columns: table => new
            {
                financial_snapshot_id = table.Column<Guid>(type: "TEXT", nullable: false),
                security_id = table.Column<Guid>(type: "TEXT", nullable: false),
                data_as_of_date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                captured_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                published_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                earnings_per_share = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: true),
                dividend_payout_ratio = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: true),
                three_year_average_dividend_payout_ratio = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: true),
                price_to_book_ratio = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: true),
                return_on_equity = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: true),
                data_source = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                source_record_id = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                data_quality_code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_financial_snapshots", x => x.financial_snapshot_id);
                table.ForeignKey(
                    name: "FK_financial_snapshots_securities_security_id",
                    column: x => x.security_id,
                    principalTable: "securities",
                    principalColumn: "security_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "model_parameter_sets",
            columns: table => new
            {
                model_parameter_set_id = table.Column<Guid>(type: "TEXT", nullable: false),
                portfolio_id = table.Column<Guid>(type: "TEXT", nullable: false),
                security_id = table.Column<Guid>(type: "TEXT", nullable: false),
                model_version = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                strong_buy_yield_threshold = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                accumulation_yield_threshold = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                partial_trim_yield_threshold = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                aggressive_trim_yield_threshold = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                strong_buy_budget_ratio = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                accumulate_budget_ratio = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                partial_trim_ratio = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                aggressive_trim_ratio = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                max_security_weight = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                max_sector_weight = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                cash_reserve_ratio = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                max_single_trade_amount = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                max_period_budget_amount = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                transaction_fee_ratio = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                minimum_transaction_fee_amount = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                trading_lot_size = table.Column<int>(type: "INTEGER", nullable: false),
                effective_from_date = table.Column<DateOnly>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_model_parameter_sets", x => x.model_parameter_set_id);
                table.ForeignKey(
                    name: "FK_model_parameter_sets_portfolios_portfolio_id",
                    column: x => x.portfolio_id,
                    principalTable: "portfolios",
                    principalColumn: "portfolio_id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_model_parameter_sets_securities_security_id",
                    column: x => x.security_id,
                    principalTable: "securities",
                    principalColumn: "security_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "portfolio_positions",
            columns: table => new
            {
                portfolio_id = table.Column<Guid>(type: "TEXT", nullable: false),
                security_id = table.Column<Guid>(type: "TEXT", nullable: false),
                held_shares = table.Column<int>(type: "INTEGER", nullable: false),
                core_shares = table.Column<int>(type: "INTEGER", nullable: false),
                target_shares = table.Column<int>(type: "INTEGER", nullable: false),
                average_cost_per_share = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_portfolio_positions", x => new { x.portfolio_id, x.security_id });
                table.ForeignKey(
                    name: "FK_portfolio_positions_portfolios_portfolio_id",
                    column: x => x.portfolio_id,
                    principalTable: "portfolios",
                    principalColumn: "portfolio_id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_portfolio_positions_securities_security_id",
                    column: x => x.security_id,
                    principalTable: "securities",
                    principalColumn: "security_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "portfolio_trades",
            columns: table => new
            {
                portfolio_trade_id = table.Column<Guid>(type: "TEXT", nullable: false),
                portfolio_id = table.Column<Guid>(type: "TEXT", nullable: false),
                security_id = table.Column<Guid>(type: "TEXT", nullable: false),
                trade_date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                trade_direction_code = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                share_quantity = table.Column<int>(type: "INTEGER", nullable: false),
                price_per_share = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                transaction_fee_amount = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                source_record_id = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_portfolio_trades", x => x.portfolio_trade_id);
                table.ForeignKey(
                    name: "FK_portfolio_trades_portfolios_portfolio_id",
                    column: x => x.portfolio_id,
                    principalTable: "portfolios",
                    principalColumn: "portfolio_id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_portfolio_trades_securities_security_id",
                    column: x => x.security_id,
                    principalTable: "securities",
                    principalColumn: "security_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "price_observations",
            columns: table => new
            {
                price_observation_id = table.Column<Guid>(type: "TEXT", nullable: false),
                security_id = table.Column<Guid>(type: "TEXT", nullable: false),
                trading_date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                close_price = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                price_observed_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                data_source = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                source_record_id = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                data_quality_code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_price_observations", x => x.price_observation_id);
                table.ForeignKey(
                    name: "FK_price_observations_securities_security_id",
                    column: x => x.security_id,
                    principalTable: "securities",
                    principalColumn: "security_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "recommendation_snapshots",
            columns: table => new
            {
                recommendation_snapshot_id = table.Column<Guid>(type: "TEXT", nullable: false),
                model_run_id = table.Column<Guid>(type: "TEXT", nullable: false),
                portfolio_id = table.Column<Guid>(type: "TEXT", nullable: false),
                security_id = table.Column<Guid>(type: "TEXT", nullable: false),
                data_as_of_date = table.Column<DateOnly>(type: "TEXT", nullable: true),
                close_price = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: true),
                model_dividend_per_share = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: true),
                dividend_mode_code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                model_status_code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                dividend_reliability_code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                observed_price_zone_code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                price_zone_code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                price_zone_confirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                recommendation_code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                dividend_yield = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: true),
                suggested_buy_shares = table.Column<int>(type: "INTEGER", nullable: false),
                suggested_sell_shares = table.Column<int>(type: "INTEGER", nullable: false),
                suggested_trade_amount = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                estimated_transaction_fee_amount = table.Column<decimal>(type: "TEXT", precision: 20, scale: 8, nullable: false),
                computed_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                model_parameter_set_id = table.Column<Guid>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_recommendation_snapshots", x => x.recommendation_snapshot_id);
                table.ForeignKey(
                    name: "FK_recommendation_snapshots_model_parameter_sets_model_parameter_set_id",
                    column: x => x.model_parameter_set_id,
                    principalTable: "model_parameter_sets",
                    principalColumn: "model_parameter_set_id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_recommendation_snapshots_portfolios_portfolio_id",
                    column: x => x.portfolio_id,
                    principalTable: "portfolios",
                    principalColumn: "portfolio_id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_recommendation_snapshots_securities_security_id",
                    column: x => x.security_id,
                    principalTable: "securities",
                    principalColumn: "security_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_cash_ledger_entries_portfolio_id_source_record_id",
            table: "cash_ledger_entries",
            columns: new[] { "portfolio_id", "source_record_id" },
            unique: true,
            filter: "source_record_id IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_cash_ledger_entries_security_id",
            table: "cash_ledger_entries",
            column: "security_id");

        migrationBuilder.CreateIndex(
            name: "IX_dividend_events_security_id_source_record_id",
            table: "dividend_events",
            columns: new[] { "security_id", "source_record_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_financial_snapshots_security_id_data_as_of_date",
            table: "financial_snapshots",
            columns: new[] { "security_id", "data_as_of_date" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_model_parameter_sets_portfolio_id_security_id_effective_from_date",
            table: "model_parameter_sets",
            columns: new[] { "portfolio_id", "security_id", "effective_from_date" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_model_parameter_sets_security_id",
            table: "model_parameter_sets",
            column: "security_id");

        migrationBuilder.CreateIndex(
            name: "IX_portfolio_positions_security_id",
            table: "portfolio_positions",
            column: "security_id");

        migrationBuilder.CreateIndex(
            name: "IX_portfolio_trades_portfolio_id_source_record_id",
            table: "portfolio_trades",
            columns: new[] { "portfolio_id", "source_record_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_portfolio_trades_portfolio_id_trade_date",
            table: "portfolio_trades",
            columns: new[] { "portfolio_id", "trade_date" });

        migrationBuilder.CreateIndex(
            name: "IX_portfolio_trades_security_id",
            table: "portfolio_trades",
            column: "security_id");

        migrationBuilder.CreateIndex(
            name: "IX_price_observations_security_id_trading_date",
            table: "price_observations",
            columns: new[] { "security_id", "trading_date" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_recommendation_snapshots_model_parameter_set_id",
            table: "recommendation_snapshots",
            column: "model_parameter_set_id");

        migrationBuilder.CreateIndex(
            name: "IX_recommendation_snapshots_model_run_id_portfolio_id_security_id",
            table: "recommendation_snapshots",
            columns: new[] { "model_run_id", "portfolio_id", "security_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_recommendation_snapshots_portfolio_id",
            table: "recommendation_snapshots",
            column: "portfolio_id");

        migrationBuilder.CreateIndex(
            name: "IX_recommendation_snapshots_security_id",
            table: "recommendation_snapshots",
            column: "security_id");

        migrationBuilder.CreateIndex(
            name: "IX_securities_exchange_code_security_code",
            table: "securities",
            columns: new[] { "exchange_code", "security_code" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "cash_ledger_entries");

        migrationBuilder.DropTable(
            name: "dividend_events");

        migrationBuilder.DropTable(
            name: "financial_snapshots");

        migrationBuilder.DropTable(
            name: "portfolio_positions");

        migrationBuilder.DropTable(
            name: "portfolio_trades");

        migrationBuilder.DropTable(
            name: "price_observations");

        migrationBuilder.DropTable(
            name: "recommendation_snapshots");

        migrationBuilder.DropTable(
            name: "model_parameter_sets");

        migrationBuilder.DropTable(
            name: "portfolios");

        migrationBuilder.DropTable(
            name: "securities");
    }
}
