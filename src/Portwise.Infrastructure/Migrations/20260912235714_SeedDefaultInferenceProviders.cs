using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Portwise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultInferenceProviders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "public",
                table: "inference_providers",
                columns: new[] { "id", "base_url", "created_at_utc", "last_verification_error_code", "last_verified_at_utc", "name", "normalized_name", "protected_api_key", "provider_type", "revision", "updated_at_utc", "verification_state" },
                values: new object[,]
                {
                    { new Guid("01a0929e-0a24-7b9e-8af2-14d1c55e9a10"), "https://api.openai.com/v1", new DateTimeOffset(new DateTime(2026, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "OpenAI", "OPENAI", null, "openai-compatible", 1L, new DateTimeOffset(new DateTime(2026, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "unverified" },
                    { new Guid("01a0929e-0a25-7c8d-9be3-25e2d66fab21"), "https://api.deepseek.com/v1", new DateTimeOffset(new DateTime(2026, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "DeepSeek", "DEEPSEEK", null, "openai-compatible", 1L, new DateTimeOffset(new DateTime(2026, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "unverified" },
                    { new Guid("01a0929e-0a26-7d7c-a4f4-36f3e77abc32"), "https://your-resource.openai.azure.com/openai/v1", new DateTimeOffset(new DateTime(2026, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "Azure OpenAI", "AZURE OPENAI", null, "openai-compatible", 1L, new DateTimeOffset(new DateTime(2026, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "unverified" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "public",
                table: "inference_providers",
                keyColumn: "id",
                keyValue: new Guid("01a0929e-0a24-7b9e-8af2-14d1c55e9a10"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "inference_providers",
                keyColumn: "id",
                keyValue: new Guid("01a0929e-0a25-7c8d-9be3-25e2d66fab21"));

            migrationBuilder.DeleteData(
                schema: "public",
                table: "inference_providers",
                keyColumn: "id",
                keyValue: new Guid("01a0929e-0a26-7d7c-a4f4-36f3e77abc32"));
        }
    }
}
