using Portwise.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Portwise.Infrastructure.Tests;

public sealed class DatabaseLifecycleTests
{
    [Fact]
    public async Task MigrateAsync_CreatesCurrentSchemaAndRecordsInitialMigration()
    {
        var databasePath = CreateDatabasePath();

        try
        {
            await using var dbContext = CreateDbContext(databasePath);
            var lifecycle = new DatabaseLifecycle(dbContext);

            await lifecycle.MigrateAsync();

            var appliedMigrations = await dbContext.Database
                .GetAppliedMigrationsAsync();
            var tableNames = await ReadTableNamesAsync(dbContext);

            Assert.Contains("20260907225950_InitialSchema", appliedMigrations);
            Assert.Contains("__EFMigrationsHistory", tableNames);
            Assert.Contains("portfolios", tableNames);
            Assert.Contains("securities", tableNames);
            Assert.Contains("recommendation_snapshots", tableNames);
        }
        finally
        {
            DeleteDatabase(databasePath);
        }
    }

    [Fact]
    public async Task MigrateAsync_WhenCalledTwice_RemainsIdempotent()
    {
        var databasePath = CreateDatabasePath();

        try
        {
            await using var dbContext = CreateDbContext(databasePath);
            var lifecycle = new DatabaseLifecycle(dbContext);

            await lifecycle.MigrateAsync();
            var migrationsAfterFirstRun = await dbContext.Database
                .GetAppliedMigrationsAsync();

            await lifecycle.MigrateAsync();

            var migrationsAfterSecondRun = await dbContext.Database
                .GetAppliedMigrationsAsync();

            Assert.Equal(migrationsAfterFirstRun, migrationsAfterSecondRun);
        }
        finally
        {
            DeleteDatabase(databasePath);
        }
    }

    [Fact]
    public async Task MigrateAsync_EnforcesTheSinglePortfolioInvariant()
    {
        var databasePath = CreateDatabasePath();

        try
        {
            await using var dbContext = CreateDbContext(databasePath);
            var lifecycle = new DatabaseLifecycle(dbContext);
            await lifecycle.MigrateAsync();

            dbContext.Portfolios.Add(new Portfolio { Name = "第一组合" });
            await dbContext.SaveChangesAsync();
            dbContext.Portfolios.Add(new Portfolio { Name = "第二组合" });

            await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
        }
        finally
        {
            DeleteDatabase(databasePath);
        }
    }

    private static PortwiseDbContext CreateDbContext(string databasePath)
    {
        var options = new DbContextOptionsBuilder<PortwiseDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;

        return new PortwiseDbContext(options);
    }

    private static async Task<IReadOnlyCollection<string>> ReadTableNamesAsync(
        PortwiseDbContext dbContext)
    {
        var tableNames = new List<string>();
        var connection = dbContext.Database.GetDbConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tableNames.Add(reader.GetString(0));
        }

        return tableNames;
    }

    private static string CreateDatabasePath()
        => Path.Combine(Path.GetTempPath(), $"portwise-{Guid.NewGuid():N}.db");

    private static void DeleteDatabase(string databasePath)
    {
        if (File.Exists(databasePath))
        {
            File.Delete(databasePath);
        }
    }
}
