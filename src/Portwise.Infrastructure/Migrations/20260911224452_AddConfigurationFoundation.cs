using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Portwise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurationFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "application_preferences",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    theme_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_application_preferences", x => x.id);
                    table.CheckConstraint("ck_application_preferences_revision", "revision > 0");
                });

            migrationBuilder.CreateTable(
                name: "inference_providers",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    provider_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    base_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    protected_api_key = table.Column<string>(type: "text", nullable: true),
                    verification_state = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    last_verified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_verification_error_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inference_providers", x => x.id);
                    table.CheckConstraint("ck_inference_providers_revision", "revision > 0");
                });

            migrationBuilder.CreateTable(
                name: "initialization_states",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_initialization_states", x => x.id);
                    table.CheckConstraint("ck_initialization_states_revision", "revision > 0");
                });

            migrationBuilder.CreateTable(
                name: "stock_data_provider_definitions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_data_provider_definitions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inference_routes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    capability = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: true),
                    model_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inference_routes", x => x.id);
                    table.CheckConstraint("ck_inference_routes_binding", "(provider_id IS NULL AND model_name IS NULL) OR (provider_id IS NOT NULL AND model_name IS NOT NULL)");
                    table.CheckConstraint("ck_inference_routes_revision", "revision > 0");
                    table.ForeignKey(
                        name: "fk_inference_routes_inference_providers_provider_id",
                        column: x => x.provider_id,
                        principalSchema: "public",
                        principalTable: "inference_providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ftshare_provider_settings",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mcp_endpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    stock_profile_tool_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    stock_market_data_tool_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    stock_dividend_events_tool_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    stock_financial_snapshots_tool_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    security_code_argument_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    exchange_code_argument_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    request_timeout_seconds = table.Column<int>(type: "integer", nullable: false),
                    max_retry_count = table.Column<int>(type: "integer", nullable: false),
                    retry_delay_milliseconds = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ftshare_provider_settings", x => x.id);
                    table.CheckConstraint("ck_ftshare_provider_settings_max_retry_count", "max_retry_count >= 0");
                    table.CheckConstraint("ck_ftshare_provider_settings_request_timeout_seconds", "request_timeout_seconds > 0");
                    table.CheckConstraint("ck_ftshare_provider_settings_retry_delay_milliseconds", "retry_delay_milliseconds >= 0");
                    table.ForeignKey(
                        name: "fk_ftshare_settings_provider_definitions_definition_id",
                        column: x => x.provider_definition_id,
                        principalSchema: "public",
                        principalTable: "stock_data_provider_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_data_providers",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_definition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    protected_credentials = table.Column<string>(type: "text", nullable: true),
                    verification_state = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    last_verified_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_verification_error_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_data_providers", x => x.id);
                    table.CheckConstraint("ck_stock_data_providers_revision", "revision > 0");
                    table.ForeignKey(
                        name: "fk_stock_providers_provider_definitions_definition_id",
                        column: x => x.provider_definition_id,
                        principalSchema: "public",
                        principalTable: "stock_data_provider_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_data_routes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    capability = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: true),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_data_routes", x => x.id);
                    table.CheckConstraint("ck_stock_data_routes_revision", "revision > 0");
                    table.ForeignKey(
                        name: "fk_stock_data_routes_stock_data_providers_provider_id",
                        column: x => x.provider_id,
                        principalSchema: "public",
                        principalTable: "stock_data_providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "inference_routes",
                columns: new[] { "id", "capability", "model_name", "provider_id", "revision", "updated_at_utc" },
                values: new object[,]
                {
                    { new Guid("01a0929e-0a22-7eec-bdf8-94c2db32b317"), "chat", null, null, 1L, new DateTimeOffset(new DateTime(2026, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("01a0929e-0a23-7c2e-9948-e2028030f99a"), "embedding", null, null, 1L, new DateTimeOffset(new DateTime(2026, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "stock_data_provider_definitions",
                columns: new[] { "id", "created_at_utc", "display_name", "is_enabled", "provider_kind", "updated_at_utc" },
                values: new object[] { new Guid("01a0929e-0a1c-7ffa-86f5-e6626219e9d2"), new DateTimeOffset(new DateTime(2026, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "FTShare", true, "ftshare", new DateTimeOffset(new DateTime(2026, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.InsertData(
                schema: "public",
                table: "stock_data_routes",
                columns: new[] { "id", "capability", "provider_id", "revision", "updated_at_utc" },
                values: new object[,]
                {
                    { new Guid("01a0929e-0a1e-79eb-b895-61b60ce05be8"), "profile", null, 1L, new DateTimeOffset(new DateTime(2026, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("01a0929e-0a1f-7fa1-9e4c-bc69e95fa39e"), "market", null, 1L, new DateTimeOffset(new DateTime(2026, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("01a0929e-0a20-7e10-a15e-f1b39354fb0b"), "dividend", null, 1L, new DateTimeOffset(new DateTime(2026, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("01a0929e-0a21-74d9-a33f-e04567185f75"), "financial", null, 1L, new DateTimeOffset(new DateTime(2026, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "ftshare_provider_settings",
                columns: new[] { "id", "exchange_code_argument_name", "max_retry_count", "mcp_endpoint", "provider_definition_id", "request_timeout_seconds", "retry_delay_milliseconds", "security_code_argument_name", "stock_dividend_events_tool_name", "stock_financial_snapshots_tool_name", "stock_market_data_tool_name", "stock_profile_tool_name" },
                values: new object[] { new Guid("01a0929e-0a1d-721b-9e74-ce8d31633069"), "exchange_code", 2, "https://market.ft.tech/gateway/mcp", new Guid("01a0929e-0a1c-7ffa-86f5-e6626219e9d2"), 30, 250, "security_code", "get_stock_dividend_events", "get_stock_financial_snapshots", "get_stock_market_data", "get_stock_profile" });

            migrationBuilder.CreateIndex(
                name: "uq_ftshare_provider_settings_provider_definition_id",
                schema: "public",
                table: "ftshare_provider_settings",
                column: "provider_definition_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_inference_providers_normalized_name",
                schema: "public",
                table: "inference_providers",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inference_routes_provider_id",
                schema: "public",
                table: "inference_routes",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "uq_inference_routes_capability",
                schema: "public",
                table: "inference_routes",
                column: "capability",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_stock_data_provider_definitions_provider_kind",
                schema: "public",
                table: "stock_data_provider_definitions",
                column: "provider_kind",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_stock_data_providers_provider_definition_id",
                schema: "public",
                table: "stock_data_providers",
                column: "provider_definition_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stock_data_routes_provider_id",
                schema: "public",
                table: "stock_data_routes",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "uq_stock_data_routes_capability",
                schema: "public",
                table: "stock_data_routes",
                column: "capability",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "application_preferences",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ftshare_provider_settings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "inference_routes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "initialization_states",
                schema: "public");

            migrationBuilder.DropTable(
                name: "stock_data_routes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "inference_providers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "stock_data_providers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "stock_data_provider_definitions",
                schema: "public");
        }
    }
}
