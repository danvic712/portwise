using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portwise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockDataSyncSettingsAndPerStockJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "batch_id",
                schema: "public",
                table: "stock_data_sync_jobs",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "security_id",
                schema: "public",
                table: "stock_data_sync_jobs",
                type: "uuid",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "stock_data_sync_settings",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    time_zone_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    run_times_json = table.Column<string>(type: "jsonb", nullable: false),
                    revision = table.Column<long>(type: "bigint", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_data_sync_settings", x => x.id);
                    table.CheckConstraint("ck_stock_data_sync_settings_revision", "revision > 0");
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "stock_data_sync_settings",
                columns: new[] { "id", "created_at_utc", "enabled", "revision", "run_times_json", "time_zone_id", "updated_at_utc" },
                values: new object[] { new Guid("01a0929e-0a28-7d0f-9b6e-58b9c13d7e20"), new DateTimeOffset(new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, 1L, "[\"18:00\"]", "Asia/Shanghai", new DateTimeOffset(new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.CreateIndex(
                name: "ix_stock_data_sync_jobs_batch",
                schema: "public",
                table: "stock_data_sync_jobs",
                columns: new[] { "batch_id", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_stock_data_sync_jobs_security",
                schema: "public",
                table: "stock_data_sync_jobs",
                column: "security_id");

            migrationBuilder.AddForeignKey(
                name: "fk_stock_data_sync_jobs_securities_security_id",
                schema: "public",
                table: "stock_data_sync_jobs",
                column: "security_id",
                principalSchema: "public",
                principalTable: "securities",
                principalColumn: "security_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_stock_data_sync_jobs_securities_security_id",
                schema: "public",
                table: "stock_data_sync_jobs");

            migrationBuilder.DropTable(
                name: "stock_data_sync_settings",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "ix_stock_data_sync_jobs_batch",
                schema: "public",
                table: "stock_data_sync_jobs");

            migrationBuilder.DropIndex(
                name: "ix_stock_data_sync_jobs_security",
                schema: "public",
                table: "stock_data_sync_jobs");

            migrationBuilder.DropColumn(
                name: "batch_id",
                schema: "public",
                table: "stock_data_sync_jobs");

            migrationBuilder.DropColumn(
                name: "security_id",
                schema: "public",
                table: "stock_data_sync_jobs");
        }
    }
}
