using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portwise.Domain.Codes;
using Portwise.Domain.Models;

namespace Portwise.Infrastructure.Configurations;

public sealed class FtShareProviderSettingsConfiguration
    : IEntityTypeConfiguration<FtShareProviderSettings>
{
    public void Configure(EntityTypeBuilder<FtShareProviderSettings> builder)
    {
        builder.ToTable("ftshare_provider_settings", table =>
        {
            table.HasCheckConstraint(
                "ck_ftshare_provider_settings_request_timeout_seconds",
                "request_timeout_seconds > 0");
            table.HasCheckConstraint(
                "ck_ftshare_provider_settings_max_retry_count",
                "max_retry_count >= 0");
            table.HasCheckConstraint(
                "ck_ftshare_provider_settings_retry_delay_milliseconds",
                "retry_delay_milliseconds >= 0");
        });
        builder.HasKey(x => x.Id).HasName("pk_ftshare_provider_settings");
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ProviderDefinitionId).HasColumnName("provider_definition_id");
        builder.Property(x => x.McpEndpoint)
            .HasColumnName("mcp_endpoint")
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(x => x.StockProfileToolName)
            .HasColumnName("stock_profile_tool_name")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.StockMarketDataToolName)
            .HasColumnName("stock_market_data_tool_name")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.StockDividendEventsToolName)
            .HasColumnName("stock_dividend_events_tool_name")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.StockFinancialSnapshotsToolName)
            .HasColumnName("stock_financial_snapshots_tool_name")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.SecurityCodeArgumentName)
            .HasColumnName("security_code_argument_name")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.ExchangeCodeArgumentName)
            .HasColumnName("exchange_code_argument_name")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.RequestTimeoutSeconds).HasColumnName("request_timeout_seconds");
        builder.Property(x => x.MaxRetryCount).HasColumnName("max_retry_count");
        builder.Property(x => x.RetryDelayMilliseconds).HasColumnName("retry_delay_milliseconds");
        builder.HasIndex(x => x.ProviderDefinitionId)
            .HasDatabaseName("uq_ftshare_provider_settings_provider_definition_id")
            .IsUnique();
        builder.HasOne<StockDataProviderDefinition>()
            .WithOne()
            .HasForeignKey<FtShareProviderSettings>(x => x.ProviderDefinitionId)
            .HasConstraintName("fk_ftshare_settings_provider_definitions_definition_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(new FtShareProviderSettings
        {
            Id = KnownConfigurationIds.FtShareProviderSettings,
            ProviderDefinitionId = KnownConfigurationIds.FtShareProviderDefinition,
            McpEndpoint = "https://market.ft.tech/gateway/mcp",
            StockProfileToolName = "get_stock_profile",
            StockMarketDataToolName = "get_stock_market_data",
            StockDividendEventsToolName = "get_stock_dividend_events",
            StockFinancialSnapshotsToolName = "get_stock_financial_snapshots",
            SecurityCodeArgumentName = "security_code",
            ExchangeCodeArgumentName = "exchange_code",
            RequestTimeoutSeconds = 30,
            MaxRetryCount = 2,
            RetryDelayMilliseconds = 250
        });
    }
}
