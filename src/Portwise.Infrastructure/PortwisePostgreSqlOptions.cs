using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

namespace Portwise.Infrastructure;

internal static class PortwisePostgreSqlOptions
{
    internal const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=portwise;Username=portwise;Password=portwise";

    internal static void Configure(
        DbContextOptionsBuilder optionsBuilder,
        string connectionString)
    {
        optionsBuilder
            .UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions
                    .SetPostgresVersion(17, 0)
                    .MigrationsHistoryTable("ef_migrations", "public")
                    .EnableRetryOnFailure(
                         maxRetryCount: 5,
                         maxRetryDelay: TimeSpan.FromSeconds(10),
                         errorCodesToAdd: null))
            .ReplaceService<IHistoryRepository, PortwiseHistoryRepository>();
    }
}

#pragma warning disable EF1001
// Npgsql exposes the history table mapping through an internal extension point.
// Keep this adapter minimal and re-check it whenever EF Core or Npgsql is upgraded;
// the rationale and verification boundary are recorded in ADR-0005.
internal sealed class PortwiseHistoryRepository(HistoryRepositoryDependencies dependencies)
    : NpgsqlHistoryRepository(dependencies)
{
    protected override void ConfigureTable(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<HistoryRow> history)
    {
        base.ConfigureTable(history);
        history
            .Property(row => row.MigrationId)
            .HasColumnName("migration_id");
        history
            .Property(row => row.ProductVersion)
            .HasColumnName("product_version");
        history
            .HasKey(row => row.MigrationId)
            .HasName("pk_ef_migrations");
    }
}
#pragma warning restore EF1001

internal static class PortwiseDbContextOptionsBuilderExtensions
{
    internal static DbContextOptionsBuilder UsePortwisePostgreSql(
        this DbContextOptionsBuilder optionsBuilder,
        string connectionString)
    {
        PortwisePostgreSqlOptions.Configure(optionsBuilder, connectionString);
        return optionsBuilder;
    }
}
