using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portwise.Domain.Models;

namespace Portwise.Infrastructure.Configurations;

public sealed class ApplicationPreferenceConfiguration : IEntityTypeConfiguration<ApplicationPreference>
{
    public void Configure(EntityTypeBuilder<ApplicationPreference> builder)
    {
        builder.ToTable("application_preferences", table =>
            table.HasCheckConstraint("ck_application_preferences_revision", "revision > 0"));
        builder.HasKey(x => x.Id).HasName("pk_application_preferences");
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Language)
            .HasColumnName("language_code")
            .HasConversion(
                value => ConfigurationCodeConverters.ToCode(value),
                value => ConfigurationCodeConverters.ToApplicationLanguage(value))
            .HasMaxLength(10)
            .IsRequired();
        builder.Property(x => x.Theme)
            .HasColumnName("theme_code")
            .HasConversion(
                value => ConfigurationCodeConverters.ToCode(value),
                value => ConfigurationCodeConverters.ToApplicationTheme(value))
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(x => x.Revision)
            .HasColumnName("revision")
            .IsConcurrencyToken();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
    }
}
