using Portwise.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Portwise.Infrastructure.Configurations;

public sealed class CashLedgerEntryConfiguration
    : IEntityTypeConfiguration<CashLedgerEntry>
{
    public void Configure(EntityTypeBuilder<CashLedgerEntry> builder)
    {
        builder.ToTable("cash_ledger_entries");
        builder.HasKey(x => x.Id).HasName("pk_cash_ledger_entries");
        builder.Property(x => x.Id).HasColumnName("cash_ledger_entry_id");
        builder.Property(x => x.PortfolioId).HasColumnName("portfolio_id");
        builder.Property(x => x.SecurityId).HasColumnName("security_id");
        builder.Property(x => x.EntryDate)
            .HasColumnName("entry_date")
            .HasColumnType("date");
        builder.Property(x => x.EntryTypeCode)
            .HasColumnName("entry_type_code")
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(x => x.CashDirectionCode)
            .HasColumnName("cash_direction_code")
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(x => x.CashAmount)
            .HasColumnName("cash_amount")
            .HasPrecision(20, 8)
            .IsRequired();
        builder.Property(x => x.SourceRecordId)
            .HasColumnName("source_record_id")
            .HasMaxLength(200);

        builder.HasIndex(x => new
        {
            x.PortfolioId,
            x.SourceRecordId
        })
            .HasDatabaseName("uq_cash_ledger_entries_portfolio_source_record")
            .HasFilter("source_record_id IS NOT NULL")
            .IsUnique();
        builder.HasIndex(x => x.SecurityId)
            .HasDatabaseName("ix_cash_ledger_entries_security_id");

        builder.HasOne<Portfolio>()
            .WithMany()
            .HasForeignKey(x => x.PortfolioId)
            .HasConstraintName("fk_cash_ledger_entries_portfolios_portfolio_id")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Security>()
            .WithMany()
            .HasForeignKey(x => x.SecurityId)
            .HasConstraintName("fk_cash_ledger_entries_securities_security_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
