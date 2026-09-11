using Portwise.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Portwise.Infrastructure;

internal sealed class PortwiseDbContext(DbContextOptions<PortwiseDbContext> options)
    : DbContext(options)
{
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();

    public DbSet<Security> Securities => Set<Security>();

    public DbSet<PortfolioPosition> PortfolioPositions => Set<PortfolioPosition>();

    public DbSet<ModelParameterSet> ModelParameterSets => Set<ModelParameterSet>();

    public DbSet<PriceObservation> PriceObservations => Set<PriceObservation>();

    public DbSet<DividendEvent> DividendEvents => Set<DividendEvent>();

    public DbSet<FinancialSnapshot> FinancialSnapshots => Set<FinancialSnapshot>();

    public DbSet<CashLedgerEntry> CashLedgerEntries => Set<CashLedgerEntry>();

    public DbSet<RecommendationSnapshot> RecommendationSnapshots => Set<RecommendationSnapshot>();

    public DbSet<PortfolioTrade> PortfolioTrades => Set<PortfolioTrade>();

    public DbSet<ApplicationPreference> ApplicationPreferences => Set<ApplicationPreference>();

    public DbSet<InitializationState> InitializationStates => Set<InitializationState>();

    public DbSet<StockDataProviderDefinition> StockDataProviderDefinitions =>
        Set<StockDataProviderDefinition>();

    public DbSet<FtShareProviderSettings> FtShareProviderSettings => Set<FtShareProviderSettings>();

    public DbSet<StockDataProvider> StockDataProviders => Set<StockDataProvider>();

    public DbSet<StockDataRoute> StockDataRoutes => Set<StockDataRoute>();

    public DbSet<InferenceProvider> InferenceProviders => Set<InferenceProvider>();

    public DbSet<InferenceRoute> InferenceRoutes => Set<InferenceRoute>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PortwiseDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
