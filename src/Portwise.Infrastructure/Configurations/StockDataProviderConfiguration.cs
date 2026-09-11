using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portwise.Domain.Models;

namespace Portwise.Infrastructure.Configurations;

public sealed class StockDataProviderConfiguration : IEntityTypeConfiguration<StockDataProvider>
{
    public void Configure(EntityTypeBuilder<StockDataProvider> builder)
    {
        builder.ToTable("stock_data_providers", table =>
            table.HasCheckConstraint("ck_stock_data_providers_revision", "revision > 0"));
        builder.HasKey(x => x.Id).HasName("pk_stock_data_providers");
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ProviderDefinitionId).HasColumnName("provider_definition_id");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.ProtectedCredentials)
            .HasColumnName("protected_credentials");
        builder.Property(x => x.VerificationState)
            .HasColumnName("verification_state")
            .HasConversion(
                value => ConfigurationCodeConverters.ToCode(value),
                value => ConfigurationCodeConverters.ToProviderVerificationState(value))
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(x => x.LastVerifiedAtUtc).HasColumnName("last_verified_at_utc");
        builder.Property(x => x.LastVerificationErrorCode)
            .HasColumnName("last_verification_error_code")
            .HasMaxLength(100);
        builder.Property(x => x.Revision)
            .HasColumnName("revision")
            .IsConcurrencyToken();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.HasIndex(x => x.ProviderDefinitionId)
            .HasDatabaseName("uq_stock_data_providers_provider_definition_id")
            .IsUnique();
        builder.HasOne<StockDataProviderDefinition>()
            .WithOne()
            .HasForeignKey<StockDataProvider>(x => x.ProviderDefinitionId)
            .HasConstraintName("fk_stock_providers_provider_definitions_definition_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
