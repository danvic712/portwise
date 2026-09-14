using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portwise.Domain.Models;

namespace Portwise.Infrastructure.Configurations;

public sealed class StockDataSyncJobConfiguration : IEntityTypeConfiguration<StockDataSyncJob>
{
    public void Configure(EntityTypeBuilder<StockDataSyncJob> builder)
    {
        builder.ToTable("stock_data_sync_jobs", table =>
        {
            table.HasCheckConstraint("ck_stock_data_sync_jobs_attempt_count", "attempt_count >= 0");
            table.HasCheckConstraint("ck_stock_data_sync_jobs_status_code",
                "status_code IN ('pending', 'running', 'completed', 'completed_with_failures', 'failed')");
        });
        builder.HasKey(job => job.Id).HasName("pk_stock_data_sync_jobs");
        builder.Property(job => job.Id).HasColumnName("id");
        builder.Property(job => job.TriggerCode).HasColumnName("trigger_code")
            .HasMaxLength(32).IsRequired();
        builder.Property(job => job.StatusCode).HasColumnName("status_code")
            .HasMaxLength(32).IsRequired();
        builder.Property(job => job.DeduplicationKey).HasColumnName("deduplication_key")
            .HasMaxLength(100);
        builder.Property(job => job.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(job => job.AvailableAtUtc).HasColumnName("available_at_utc");
        builder.Property(job => job.StartedAtUtc).HasColumnName("started_at_utc");
        builder.Property(job => job.LeaseExpiresAtUtc).HasColumnName("lease_expires_at_utc");
        builder.Property(job => job.LeaseOwnerId).HasColumnName("lease_owner_id");
        builder.Property(job => job.CompletedAtUtc).HasColumnName("completed_at_utc");
        builder.Property(job => job.AttemptCount).HasColumnName("attempt_count");
        builder.Property(job => job.ResultJson).HasColumnName("result_json").HasColumnType("jsonb");
        builder.Property(job => job.ErrorCode).HasColumnName("error_code").HasMaxLength(100);
        builder.HasIndex(job => job.DeduplicationKey)
            .HasDatabaseName("uq_stock_data_sync_jobs_deduplication_key").IsUnique();
        builder.HasIndex(job => new { job.StatusCode, job.AvailableAtUtc, job.CreatedAtUtc })
            .HasDatabaseName("ix_stock_data_sync_jobs_claim");
    }
}
