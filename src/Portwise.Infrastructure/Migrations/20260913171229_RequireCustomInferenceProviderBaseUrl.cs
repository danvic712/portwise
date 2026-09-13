using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portwise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RequireCustomInferenceProviderBaseUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "public",
                table: "inference_providers",
                keyColumn: "id",
                keyValue: new Guid("01a0929e-0a27-7f4a-8b5c-47a8b02c6d19"),
                column: "base_url",
                value: "https://api.example.com/v1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "public",
                table: "inference_providers",
                keyColumn: "id",
                keyValue: new Guid("01a0929e-0a27-7f4a-8b5c-47a8b02c6d19"),
                column: "base_url",
                value: "https://api.openai.com/v1");
        }
    }
}
