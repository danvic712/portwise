using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portwise.Domain.Codes;
using Portwise.Domain.Enums;
using Portwise.Domain.Models;

namespace Portwise.Infrastructure.Configurations;

public sealed class InferenceRouteConfiguration : IEntityTypeConfiguration<InferenceRoute>
{
    private static readonly DateTimeOffset SeedTimestamp =
        new(2026, 9, 12, 0, 0, 0, TimeSpan.Zero);

    public void Configure(EntityTypeBuilder<InferenceRoute> builder)
    {
        builder.ToTable("inference_routes", table =>
        {
            table.HasCheckConstraint("ck_inference_routes_revision", "revision > 0");
            table.HasCheckConstraint(
                "ck_inference_routes_binding",
                "(provider_id IS NULL AND model_name IS NULL) OR " +
                "(provider_id IS NOT NULL AND model_name IS NOT NULL)");
        });
        builder.HasKey(x => x.Id).HasName("pk_inference_routes");
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Capability)
            .HasColumnName("capability")
            .HasConversion(
                value => ConfigurationCodeConverters.ToCode(value),
                value => ConfigurationCodeConverters.ToInferenceCapability(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(x => x.ProviderId).HasColumnName("provider_id");
        builder.Property(x => x.ModelName)
            .HasColumnName("model_name")
            .HasMaxLength(200);
        builder.Property(x => x.Revision)
            .HasColumnName("revision")
            .IsConcurrencyToken();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.HasIndex(x => x.Capability)
            .HasDatabaseName("uq_inference_routes_capability")
            .IsUnique();
        builder.HasIndex(x => x.ProviderId)
            .HasDatabaseName("ix_inference_routes_provider_id");
        builder.HasOne<InferenceProvider>()
            .WithMany()
            .HasForeignKey(x => x.ProviderId)
            .HasConstraintName("fk_inference_routes_inference_providers_provider_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            CreateSeed(KnownConfigurationIds.InferenceChatRoute, InferenceCapability.Chat),
            CreateSeed(
                KnownConfigurationIds.InferenceEmbeddingRoute,
                InferenceCapability.Embedding));
    }

    private static InferenceRoute CreateSeed(Guid id, InferenceCapability capability) => new()
    {
        Id = id,
        Capability = capability,
        ProviderId = null,
        ModelName = null,
        Revision = 1,
        UpdatedAtUtc = SeedTimestamp
    };
}
