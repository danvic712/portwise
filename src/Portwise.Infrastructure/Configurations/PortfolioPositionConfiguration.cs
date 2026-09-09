using Portwise.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Portwise.Infrastructure.Configurations;

public sealed class PortfolioPositionConfiguration
    : IEntityTypeConfiguration<PortfolioPosition>
{
    public void Configure(EntityTypeBuilder<PortfolioPosition> builder)
    {
        builder.ToTable("portfolio_positions");
        builder.HasKey(x => new { x.PortfolioId, x.SecurityId })
            .HasName("pk_portfolio_positions");
        builder.Property(x => x.PortfolioId).HasColumnName("portfolio_id");
        builder.Property(x => x.SecurityId).HasColumnName("security_id");
        builder.Property(x => x.HeldShares).HasColumnName("held_shares");
        builder.Property(x => x.CoreShares).HasColumnName("core_shares");
        builder.Property(x => x.TargetShares).HasColumnName("target_shares");
        builder.Property(x => x.AverageCostPerShare)
            .HasColumnName("average_cost_per_share")
            .HasPrecision(20, 8);
        builder.HasIndex(x => x.SecurityId)
            .HasDatabaseName("ix_portfolio_positions_security_id");
        builder.HasOne<Portfolio>()
            .WithMany()
            .HasForeignKey(x => x.PortfolioId)
            .HasConstraintName("fk_portfolio_positions_portfolios_portfolio_id")
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Security>()
            .WithMany()
            .HasForeignKey(x => x.SecurityId)
            .HasConstraintName("fk_portfolio_positions_securities_security_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
