using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portwise.Domain.Codes;
using Portwise.Domain.Enums;
using Portwise.Domain.Models;

namespace Portwise.Infrastructure.Configurations;

public sealed class StockDataRouteConfiguration : IEntityTypeConfiguration<StockDataRoute>
{
    private static readonly DateTimeOffset SeedTimestamp =
        new(2026, 9, 12, 0, 0, 0, TimeSpan.Zero);

    public void Configure(EntityTypeBuilder<StockDataRoute> builder)
    {
        builder.ToTable("stock_data_routes", table =>
            table.HasCheckConstraint("ck_stock_data_routes_revision", "revision > 0"));
        builder.HasKey(x => x.Id).HasName("pk_stock_data_routes");
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Capability)
            .HasColumnName("capability")
            .HasConversion(
                value => ConfigurationCodeConverters.ToCode(value),
                value => ConfigurationCodeConverters.ToStockDataCapability(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(x => x.ProviderId).HasColumnName("provider_id");
        builder.Property(x => x.Revision)
            .HasColumnName("revision")
            .IsConcurrencyToken();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.HasIndex(x => x.Capability)
            .HasDatabaseName("uq_stock_data_routes_capability")
            .IsUnique();
        builder.HasIndex(x => x.ProviderId)
            .HasDatabaseName("ix_stock_data_routes_provider_id");
        builder.HasOne<StockDataProvider>()
            .WithMany()
            .HasForeignKey(x => x.ProviderId)
            .HasConstraintName("fk_stock_data_routes_stock_data_providers_provider_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            CreateSeed(KnownConfigurationIds.StockProfileRoute, StockDataCapability.Profile),
            CreateSeed(KnownConfigurationIds.StockMarketRoute, StockDataCapability.Market),
            CreateSeed(KnownConfigurationIds.StockDividendRoute, StockDataCapability.Dividend),
            CreateSeed(KnownConfigurationIds.StockFinancialRoute, StockDataCapability.Financial));
    }

    private static StockDataRoute CreateSeed(Guid id, StockDataCapability capability) => new()
    {
        Id = id,
        Capability = capability,
        ProviderId = null,
        Revision = 1,
        UpdatedAtUtc = SeedTimestamp
    };
}
