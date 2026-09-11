using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portwise.Domain.Codes;
using Portwise.Domain.Enums;
using Portwise.Domain.Models;

namespace Portwise.Infrastructure.Configurations;

public sealed class StockDataProviderDefinitionConfiguration
    : IEntityTypeConfiguration<StockDataProviderDefinition>
{
    private static readonly DateTimeOffset SeedTimestamp =
        new(2026, 9, 12, 0, 0, 0, TimeSpan.Zero);

    public void Configure(EntityTypeBuilder<StockDataProviderDefinition> builder)
    {
        builder.ToTable("stock_data_provider_definitions");
        builder.HasKey(x => x.Id).HasName("pk_stock_data_provider_definitions");
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ProviderKind)
            .HasColumnName("provider_kind")
            .HasConversion(
                value => ConfigurationCodeConverters.ToCode(value),
                value => ConfigurationCodeConverters.ToStockDataProviderKind(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(x => x.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.IsEnabled).HasColumnName("is_enabled");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.HasIndex(x => x.ProviderKind)
            .HasDatabaseName("uq_stock_data_provider_definitions_provider_kind")
            .IsUnique();

        builder.HasData(new StockDataProviderDefinition
        {
            Id = KnownConfigurationIds.FtShareProviderDefinition,
            ProviderKind = StockDataProviderKind.FtShare,
            DisplayName = "FTShare",
            IsEnabled = true,
            CreatedAtUtc = SeedTimestamp,
            UpdatedAtUtc = SeedTimestamp
        });
    }
}
