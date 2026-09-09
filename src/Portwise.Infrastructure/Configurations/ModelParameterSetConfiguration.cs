using Portwise.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Portwise.Infrastructure.Configurations;

public sealed class ModelParameterSetConfiguration
    : IEntityTypeConfiguration<ModelParameterSet>
{
    public void Configure(EntityTypeBuilder<ModelParameterSet> builder)
    {
        builder.ToTable("model_parameter_sets");
        builder.HasKey(x => x.Id).HasName("pk_model_parameter_sets");
        builder.Property(x => x.Id).HasColumnName("model_parameter_set_id");
        builder.Property(x => x.PortfolioId).HasColumnName("portfolio_id");
        builder.Property(x => x.SecurityId).HasColumnName("security_id");
        builder.Property(x => x.ModelVersion)
            .HasColumnName("model_version")
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(x => x.StrongBuyYieldThreshold)
            .HasColumnName("strong_buy_yield_threshold")
            .HasPrecision(20, 8);
        builder.Property(x => x.AccumulationYieldThreshold)
            .HasColumnName("accumulation_yield_threshold")
            .HasPrecision(20, 8);
        builder.Property(x => x.PartialTrimYieldThreshold)
            .HasColumnName("partial_trim_yield_threshold")
            .HasPrecision(20, 8);
        builder.Property(x => x.AggressiveTrimYieldThreshold)
            .HasColumnName("aggressive_trim_yield_threshold")
            .HasPrecision(20, 8);
        builder.Property(x => x.StrongBuyBudgetRatio)
            .HasColumnName("strong_buy_budget_ratio")
            .HasPrecision(20, 8);
        builder.Property(x => x.AccumulateBudgetRatio)
            .HasColumnName("accumulate_budget_ratio")
            .HasPrecision(20, 8);
        builder.Property(x => x.PartialTrimRatio)
            .HasColumnName("partial_trim_ratio")
            .HasPrecision(20, 8);
        builder.Property(x => x.AggressiveTrimRatio)
            .HasColumnName("aggressive_trim_ratio")
            .HasPrecision(20, 8);
        builder.Property(x => x.MaxSecurityWeight)
            .HasColumnName("max_security_weight")
            .HasPrecision(20, 8);
        builder.Property(x => x.MaxSectorWeight)
            .HasColumnName("max_sector_weight")
            .HasPrecision(20, 8);
        builder.Property(x => x.CashReserveRatio)
            .HasColumnName("cash_reserve_ratio")
            .HasPrecision(20, 8);
        builder.Property(x => x.MaxSingleTradeAmount)
            .HasColumnName("max_single_trade_amount")
            .HasPrecision(20, 8);
        builder.Property(x => x.MaxPeriodBudgetAmount)
            .HasColumnName("max_period_budget_amount")
            .HasPrecision(20, 8);
        builder.Property(x => x.TransactionFeeRatio)
            .HasColumnName("transaction_fee_ratio")
            .HasPrecision(20, 8);
        builder.Property(x => x.MinimumTransactionFeeAmount)
            .HasColumnName("minimum_transaction_fee_amount")
            .HasPrecision(20, 8);
        builder.Property(x => x.TradingLotSize).HasColumnName("trading_lot_size");
        builder.Property(x => x.EffectiveFromDate)
            .HasColumnName("effective_from_date")
            .HasColumnType("date");

        builder.HasIndex(x => new
        {
            x.PortfolioId,
            x.SecurityId,
            x.EffectiveFromDate
        })
            .HasDatabaseName("uq_model_parameter_sets_portfolio_security_effective_from")
            .IsUnique();
        builder.HasIndex(x => x.SecurityId)
            .HasDatabaseName("ix_model_parameter_sets_security_id");

        builder.HasOne<Portfolio>()
            .WithMany()
            .HasForeignKey(x => x.PortfolioId)
            .HasConstraintName("fk_model_parameter_sets_portfolios_portfolio_id")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Security>()
            .WithMany()
            .HasForeignKey(x => x.SecurityId)
            .HasConstraintName("fk_model_parameter_sets_securities_security_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
