using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portwise.Infrastructure.Migrations;

/// <inheritdoc />
public partial class EnforceSinglePortfolio : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "portfolio_scope",
            table: "portfolios",
            type: "TEXT",
            maxLength: 32,
            nullable: false,
            defaultValue: "default");

        migrationBuilder.CreateIndex(
            name: "IX_portfolios_portfolio_scope",
            table: "portfolios",
            column: "portfolio_scope",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_portfolios_portfolio_scope",
            table: "portfolios");

        migrationBuilder.DropColumn(
            name: "portfolio_scope",
            table: "portfolios");
    }
}
