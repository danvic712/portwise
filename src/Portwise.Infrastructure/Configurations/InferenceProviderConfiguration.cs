using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portwise.Domain.Codes;
using Portwise.Domain.Enums;
using Portwise.Domain.Models;

namespace Portwise.Infrastructure.Configurations;

public sealed class InferenceProviderConfiguration : IEntityTypeConfiguration<InferenceProvider>
{
    private static readonly DateTimeOffset SeedTimestamp =
        new(2026, 9, 12, 0, 0, 0, TimeSpan.Zero);

    public void Configure(EntityTypeBuilder<InferenceProvider> builder)
    {
        builder.ToTable("inference_providers", table =>
            table.HasCheckConstraint("ck_inference_providers_revision", "revision > 0"));
        builder.HasKey(x => x.Id).HasName("pk_inference_providers");
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.NormalizedName)
            .HasColumnName("normalized_name")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(x => x.ProviderType)
            .HasColumnName("provider_type")
            .HasConversion(
                value => ConfigurationCodeConverters.ToCode(value),
                value => ConfigurationCodeConverters.ToInferenceProviderType(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(x => x.BaseUrl)
            .HasColumnName("base_url")
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(x => x.IsBaseUrlEditable)
            .HasColumnName("is_base_url_editable")
            .IsRequired();
        builder.Property(x => x.ProtectedApiKey).HasColumnName("protected_api_key");
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
        builder.HasIndex(x => x.NormalizedName)
            .HasDatabaseName("uq_inference_providers_normalized_name")
            .IsUnique();

        builder.HasData(
            CreateSeed(
                KnownConfigurationIds.InferenceOpenAiProvider,
                "OpenAI",
                "https://api.openai.com/v1",
                isBaseUrlEditable: false),
            CreateSeed(
                KnownConfigurationIds.InferenceDeepSeekProvider,
                "DeepSeek",
                "https://api.deepseek.com/v1",
                isBaseUrlEditable: false),
            CreateSeed(
                KnownConfigurationIds.InferenceAzureOpenAiProvider,
                "Azure OpenAI",
                "https://your-resource.openai.azure.com/openai/v1",
                isBaseUrlEditable: true),
            CreateSeed(
                KnownConfigurationIds.InferenceOpenAiCompatibleProvider,
                "OpenAI Compatible",
                "https://api.openai.com/v1",
                isBaseUrlEditable: true));
    }

    private static InferenceProvider CreateSeed(
        Guid id,
        string name,
        string baseUrl,
        bool isBaseUrlEditable) => new()
    {
        Id = id,
        Name = name,
        NormalizedName = InferenceProvider.NormalizeName(name),
        ProviderType = InferenceProviderType.OpenAiCompatible,
        BaseUrl = baseUrl,
        IsBaseUrlEditable = isBaseUrlEditable,
        ProtectedApiKey = null,
        VerificationState = ProviderVerificationState.Unverified,
        LastVerifiedAtUtc = null,
        LastVerificationErrorCode = null,
        Revision = 1,
        CreatedAtUtc = SeedTimestamp,
        UpdatedAtUtc = SeedTimestamp
    };
}
