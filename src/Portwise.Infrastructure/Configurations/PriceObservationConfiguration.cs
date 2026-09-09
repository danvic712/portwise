using Portwise.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Portwise.Infrastructure.Configurations;

public sealed class PriceObservationConfiguration
    : IEntityTypeConfiguration<PriceObservation>
{
    public void Configure(EntityTypeBuilder<PriceObservation> builder)
    {
        builder.ToTable("price_observations");
        builder.HasKey(x => x.Id).HasName("pk_price_observations");
        builder.Property(x => x.Id).HasColumnName("price_observation_id");
        builder.Property(x => x.SecurityId).HasColumnName("security_id");
        builder.Property(x => x.TradingDate)
            .HasColumnName("trading_date")
            .HasColumnType("date");
        builder.Property(x => x.ClosePrice)
            .HasColumnName("close_price")
            .HasPrecision(20, 8);
        builder.Property(x => x.PriceObservedAt)
            .HasColumnName("price_observed_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(x => x.DataSource)
            .HasColumnName("data_source")
            .HasMaxLength(64)
            .IsRequired();
        builder.Property(x => x.SourceRecordId)
            .HasColumnName("source_record_id")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(x => x.DataQualityCode)
            .HasColumnName("data_quality_code")
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.SecurityId,
            x.TradingDate
        })
            .HasDatabaseName("uq_price_observations_security_trading_date")
            .IsUnique();

        builder.HasOne<Security>()
            .WithMany()
            .HasForeignKey(x => x.SecurityId)
            .HasConstraintName("fk_price_observations_securities_security_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
