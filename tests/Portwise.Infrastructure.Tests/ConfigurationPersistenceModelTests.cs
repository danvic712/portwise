using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Portwise.Domain.Codes;
using Portwise.Domain.Enums;
using Portwise.Domain.Models;
using Xunit;

namespace Portwise.Infrastructure.Tests;

public sealed class ConfigurationPersistenceModelTests
{
    private static readonly Type[] ConfigurationEntityTypes =
    [
        typeof(ApplicationPreference),
        typeof(InitializationState),
        typeof(StockDataProviderDefinition),
        typeof(FtShareProviderSettings),
        typeof(StockDataProvider),
        typeof(StockDataRoute),
        typeof(InferenceProvider),
        typeof(InferenceRoute)
    ];

    [Fact]
    public void ConfigurationEntities_UseUuidV7IdPrimaryKeys()
    {
        using var context = CreateContext();

        foreach (var entityType in ConfigurationEntityTypes)
        {
            var metadata = context.Model.FindEntityType(entityType);
            Assert.NotNull(metadata);
            var primaryKey = metadata.FindPrimaryKey();
            Assert.NotNull(primaryKey);
            var property = Assert.Single(primaryKey.Properties);
            var tableName = metadata.GetTableName();
            Assert.NotNull(tableName);
            var table = StoreObjectIdentifier.Table(tableName, metadata.GetSchema());

            Assert.Equal(nameof(ApplicationPreference.Id), property.Name);
            Assert.Equal("id", property.GetColumnName(table));
        }

        var identifiers = new[]
        {
            ApplicationPreference.Create(
                ApplicationLanguage.ZhCn,
                ApplicationTheme.System,
                DateTimeOffset.UnixEpoch).Id,
            new InitializationState().Id,
            new StockDataProviderDefinition().Id,
            new FtShareProviderSettings().Id,
            new StockDataProvider().Id,
            new StockDataRoute().Id,
            new InferenceProvider().Id,
            new InferenceRoute().Id
        };

        Assert.All(identifiers, AssertUuidV7);
    }

    [Fact]
    public void MigrationManagedConfiguration_SeedsOnlyNonSensitiveRecords()
    {
        using var context = CreateContext();
        var model = context.GetService<IDesignTimeModel>().Model;

        var definitionSeeds = model
            .FindEntityType(typeof(StockDataProviderDefinition))!
            .GetSeedData();
        var settingsSeeds = model
            .FindEntityType(typeof(FtShareProviderSettings))!
            .GetSeedData();
        var stockRouteSeeds = model
            .FindEntityType(typeof(StockDataRoute))!
            .GetSeedData();
        var inferenceRouteSeeds = model
            .FindEntityType(typeof(InferenceRoute))!
            .GetSeedData();
        var stockProviderSeeds = model
            .FindEntityType(typeof(StockDataProvider))!
            .GetSeedData();
        var inferenceProviderSeeds = model
            .FindEntityType(typeof(InferenceProvider))!
            .GetSeedData();

        Assert.Single(definitionSeeds);
        Assert.Single(settingsSeeds);
        Assert.Equal(4, stockRouteSeeds.Count());
        Assert.Equal(2, inferenceRouteSeeds.Count());
        Assert.Empty(stockProviderSeeds);
        Assert.Empty(inferenceProviderSeeds);

        AssertUuidV7(KnownConfigurationIds.FtShareProviderDefinition);
        AssertUuidV7(KnownConfigurationIds.FtShareProviderSettings);
        AssertUuidV7(KnownConfigurationIds.StockProfileRoute);
        AssertUuidV7(KnownConfigurationIds.StockMarketRoute);
        AssertUuidV7(KnownConfigurationIds.StockDividendRoute);
        AssertUuidV7(KnownConfigurationIds.StockFinancialRoute);
        AssertUuidV7(KnownConfigurationIds.InferenceChatRoute);
        AssertUuidV7(KnownConfigurationIds.InferenceEmbeddingRoute);
    }

    private static PortwiseDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PortwiseDbContext>()
            .UseNpgsql("Host=localhost;Database=portwise_model_tests;Username=portwise")
            .Options;
        return new PortwiseDbContext(options);
    }

    private static void AssertUuidV7(Guid value) =>
        Assert.Equal('7', value.ToString("D")[14]);
}
