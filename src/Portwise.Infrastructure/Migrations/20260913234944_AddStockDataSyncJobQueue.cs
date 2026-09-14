using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portwise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockDataSyncJobQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stock_data_sync_jobs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trigger_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    deduplication_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    available_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    started_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lease_expires_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lease_owner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    result_json = table.Column<string>(type: "jsonb", nullable: true),
                    error_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_data_sync_jobs", x => x.id);
                    table.CheckConstraint("ck_stock_data_sync_jobs_attempt_count", "attempt_count >= 0");
                    table.CheckConstraint("ck_stock_data_sync_jobs_status_code", "status_code IN ('pending', 'running', 'completed', 'completed_with_failures', 'failed')");
                });

            migrationBuilder.CreateIndex(
                name: "ix_stock_data_sync_jobs_claim",
                schema: "public",
                table: "stock_data_sync_jobs",
                columns: new[] { "status_code", "available_at_utc", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "uq_stock_data_sync_jobs_deduplication_key",
                schema: "public",
                table: "stock_data_sync_jobs",
                column: "deduplication_key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_data_sync_jobs",
                schema: "public");
        }
    }
}
