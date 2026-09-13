using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portwise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEditableInferenceProviderBaseUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_base_url_editable",
                schema: "public",
                table: "inference_providers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "inference_providers",
                keyColumn: "id",
                keyValue: new Guid("01a0929e-0a24-7b9e-8af2-14d1c55e9a10"),
                column: "is_base_url_editable",
                value: false);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "inference_providers",
                keyColumn: "id",
                keyValue: new Guid("01a0929e-0a25-7c8d-9be3-25e2d66fab21"),
                column: "is_base_url_editable",
                value: false);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "inference_providers",
                keyColumn: "id",
                keyValue: new Guid("01a0929e-0a26-7d7c-a4f4-36f3e77abc32"),
                column: "is_base_url_editable",
                value: true);

            migrationBuilder.InsertData(
                schema: "public",
                table: "inference_providers",
                columns: new[] { "id", "base_url", "created_at_utc", "is_base_url_editable", "last_verification_error_code", "last_verified_at_utc", "name", "normalized_name", "protected_api_key", "provider_type", "revision", "updated_at_utc", "verification_state" },
                values: new object[] { new Guid("01a0929e-0a27-7f4a-8b5c-47a8b02c6d19"), "https://api.openai.com/v1", new DateTimeOffset(new DateTime(2026, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, null, null, "OpenAI Compatible", "OPENAI COMPATIBLE", null, "openai-compatible", 1L, new DateTimeOffset(new DateTime(2026, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "unverified" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "public",
                table: "inference_providers",
                keyColumn: "id",
                keyValue: new Guid("01a0929e-0a27-7f4a-8b5c-47a8b02c6d19"));

            migrationBuilder.DropColumn(
                name: "is_base_url_editable",
                schema: "public",
                table: "inference_providers");
        }
    }
}
