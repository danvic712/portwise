using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portwise.Domain.Models;

namespace Portwise.Infrastructure.Configurations;

public sealed class InitializationStateConfiguration : IEntityTypeConfiguration<InitializationState>
{
    public void Configure(EntityTypeBuilder<InitializationState> builder)
    {
        builder.ToTable("initialization_states", table =>
            table.HasCheckConstraint("ck_initialization_states_revision", "revision > 0"));
        builder.HasKey(x => x.Id).HasName("pk_initialization_states");
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.CompletedAtUtc).HasColumnName("completed_at_utc");
        builder.Property(x => x.Revision)
            .HasColumnName("revision")
            .IsConcurrencyToken();
    }
}
