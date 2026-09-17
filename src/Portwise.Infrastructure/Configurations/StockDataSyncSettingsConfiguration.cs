using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portwise.Domain.Codes;
using Portwise.Domain.Models;

namespace Portwise.Infrastructure.Configurations;

public sealed class StockDataSyncSettingsConfiguration : IEntityTypeConfiguration<StockDataSyncSettings>
{
    public void Configure(EntityTypeBuilder<StockDataSyncSettings> builder)
    {
        builder.ToTable("stock_data_sync_settings", table =>
            table.HasCheckConstraint("ck_stock_data_sync_settings_revision", "revision > 0"));
        builder.HasKey(settings => settings.Id).HasName("pk_stock_data_sync_settings");
        builder.Property(settings => settings.Id).HasColumnName("id");
        builder.Property(settings => settings.Enabled).HasColumnName("enabled");
        builder.Property(settings => settings.TimeZoneId)
            .HasColumnName("time_zone_id")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(settings => settings.RunTimesJson)
            .HasColumnName("run_times_json")
            .HasColumnType("jsonb")
            .IsRequired();
        builder.Property(settings => settings.Revision)
            .HasColumnName("revision")
            .IsConcurrencyToken();
        builder.Property(settings => settings.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(settings => settings.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.HasData(new StockDataSyncSettings
        {
            Id = KnownConfigurationIds.StockDataSyncSettings,
            Enabled = true,
            TimeZoneId = "Asia/Shanghai",
            RunTimesJson = "[\"18:00\"]",
            Revision = 1,
            CreatedAtUtc = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAtUtc = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero)
        });
    }
}
