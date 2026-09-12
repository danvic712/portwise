using Moq;
using Portwise.Application.Configuration;
using Portwise.Application.Configuration.Contracts;
using Portwise.Application.Configuration.Dtos;
using Portwise.Application.Exceptions;
using Portwise.Application.Initialization;
using Portwise.Application.Initialization.Dtos;
using Portwise.Domain.Codes;
using Portwise.Domain.Contracts;
using Portwise.Domain.Enums;
using Portwise.Domain.Models;
using Xunit;
using PortfolioEntity = Portwise.Domain.Models.Portfolio;

namespace Portwise.Application.Tests;

public sealed class InitializationAppServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 9, 12, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetAsync_UsesExplicitStateAndReportsAllCapabilities()
    {
        var (service, _, _) = CreateService();

        var response = await service.GetAsync(CancellationToken.None);

        Assert.False(response.IsComplete);
        Assert.Null(response.Preferences);
        Assert.Equal(
            ["profile", "market", "dividend", "financial", "chat", "embedding"],
            response.Limitations.Select(item => item.CapabilityCode));
        Assert.All(response.Limitations, item => Assert.Equal("unconfigured", item.StatusCode));
    }

    [Fact]
    public async Task CompleteAsync_PersistsBaseConfigurationInOneCommit()
    {
        var (service, unitOfWork, _) = CreateService();

        var response = await service.CompleteAsync(
            new CompleteInitializationRequest(
                "zh-CN",
                "system",
                "长期组合",
                null,
                null,
                null,
                null),
            CancellationToken.None);

        Assert.True(response.Status.IsComplete);
        Assert.Equal("zh-CN", response.Status.Preferences?.LanguageCode);
        Assert.Equal("system", response.Status.Preferences?.ThemeCode);
        Assert.Contains(response.Status.Limitations, item => item.StatusCode == "unconfigured");
        unitOfWork.Verify(item => item.CommitAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_SavesProvidersAndRoutesWithoutReturningSecrets()
    {
        var definition = new StockDataProviderDefinition
        {
            Id = KnownConfigurationIds.FtShareProviderDefinition,
            ProviderKind = StockDataProviderKind.FtShare,
            DisplayName = "FTShare",
            IsEnabled = true
        };
        var stockRoutes = Enum.GetValues<StockDataCapability>()
            .Select(capability => new StockDataRoute
            {
                Id = Guid.CreateVersion7(),
                Capability = capability,
                Revision = 1
            })
            .ToList();
        var inferenceRoutes = Enum.GetValues<InferenceCapability>()
            .Select(capability => new InferenceRoute
            {
                Id = Guid.CreateVersion7(),
                Capability = capability,
                Revision = 1
            })
            .ToList();
        var (service, unitOfWork, _) = CreateService(
            definitions: [definition],
            stockRoutes: stockRoutes,
            inferenceRoutes: inferenceRoutes);

        var response = await service.CompleteAsync(
            new CompleteInitializationRequest(
                "en-US",
                "dark",
                "AI 组合",
                [new CompleteStockDataProviderRequest(
                    definition.Id,
                    "ftshare-main",
                    new SecretUpdateRequest(SecretCodes.Replace, "ftshare-key"))],
                [
                    new("profile", "ftshare-main"),
                    new("market", "ftshare-main"),
                    new("dividend", "ftshare-main"),
                    new("financial", "ftshare-main")
                ],
                [new CompleteInferenceProviderRequest(
                    "cloud",
                    "https://api.example.com/v1",
                    new SecretUpdateRequest(SecretCodes.Replace, "api-key"))],
                [
                    new("chat", "cloud", "chat-model"),
                    new("embedding", "cloud", "embedding-model")
                ]),
            CancellationToken.None);

        Assert.All(
            response.Status.Limitations,
            item => Assert.Equal("configured-unverified", item.StatusCode));
        Assert.DoesNotContain("api-key", response.Status.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("ftshare-key", response.Status.ToString(), StringComparison.Ordinal);
        unitOfWork.Verify(item => item.CommitAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_RejectsWhenInitializationAlreadyExists()
    {
        var state = new InitializationState
        {
            Id = KnownConfigurationIds.InitializationState,
            CompletedAtUtc = Now
        };
        var (service, unitOfWork, _) = CreateService(initializationState: state);

        var exception = await Assert.ThrowsAsync<ApplicationErrorException>(() =>
            service.CompleteAsync(
                new CompleteInitializationRequest("zh-CN", "system", "组合", null, null, null, null),
                CancellationToken.None));

        Assert.Equal(ApplicationErrorCodes.InitializationAlreadyCompleted, exception.ErrorCode);
        unitOfWork.Verify(item => item.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CompleteAsync_RejectsInvalidRouteShapeWithoutCommit()
    {
        var (service, unitOfWork, _) = CreateService();

        var exception = await Assert.ThrowsAsync<ApplicationErrorException>(() =>
            service.CompleteAsync(
                new CompleteInitializationRequest(
                    "zh-CN",
                    "system",
                    "组合",
                    null,
                    [new("profile", null)],
                    null,
                    null),
                CancellationToken.None));

        Assert.Equal(ApplicationErrorCodes.InitializationValidationFailed, exception.ErrorCode);
        unitOfWork.Verify(item => item.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static (
        InitializationAppService Service,
        Mock<IUow> UnitOfWork,
        InitializationData Data) CreateService(
        InitializationState? initializationState = null,
        IReadOnlyList<StockDataProviderDefinition>? definitions = null,
        IReadOnlyList<StockDataRoute>? stockRoutes = null,
        IReadOnlyList<InferenceRoute>? inferenceRoutes = null)
    {
        var data = new InitializationData
        {
            InitializationStates = initializationState is null ? [] : [initializationState],
            Portfolios = [],
            Preferences = [],
            Definitions = definitions?.ToList() ?? [],
            StockProviders = [],
            StockRoutes = stockRoutes?.ToList() ?? [],
            InferenceProviders = [],
            InferenceRoutes = inferenceRoutes?.ToList() ?? []
        };
        var repositories = new Dictionary<Type, object>
        {
            [typeof(InitializationState)] = RepositoryMock.Create(data.InitializationStates),
            [typeof(ApplicationPreference)] = RepositoryMock.Create(data.Preferences),
            [typeof(PortfolioEntity)] = RepositoryMock.Create(data.Portfolios),
            [typeof(StockDataProviderDefinition)] = RepositoryMock.Create(data.Definitions),
            [typeof(StockDataProvider)] = RepositoryMock.Create(data.StockProviders),
            [typeof(StockDataRoute)] = RepositoryMock.Create(data.StockRoutes),
            [typeof(InferenceProvider)] = RepositoryMock.Create(data.InferenceProviders),
            [typeof(InferenceRoute)] = RepositoryMock.Create(data.InferenceRoutes)
        };
        var unitOfWork = new Mock<IUow>();
        unitOfWork
            .Setup(item => item.Get<InitializationState>())
            .Returns(((Mock<IRepository<InitializationState>>)repositories[typeof(InitializationState)]).Object);
        unitOfWork
            .Setup(item => item.Get<ApplicationPreference>())
            .Returns(((Mock<IRepository<ApplicationPreference>>)repositories[typeof(ApplicationPreference)]).Object);
        unitOfWork
            .Setup(item => item.Get<PortfolioEntity>())
            .Returns(((Mock<IRepository<PortfolioEntity>>)repositories[typeof(PortfolioEntity)]).Object);
        unitOfWork
            .Setup(item => item.Get<StockDataProviderDefinition>())
            .Returns(((Mock<IRepository<StockDataProviderDefinition>>)repositories[typeof(StockDataProviderDefinition)]).Object);
        unitOfWork
            .Setup(item => item.Get<StockDataProvider>())
            .Returns(((Mock<IRepository<StockDataProvider>>)repositories[typeof(StockDataProvider)]).Object);
        unitOfWork
            .Setup(item => item.Get<StockDataRoute>())
            .Returns(((Mock<IRepository<StockDataRoute>>)repositories[typeof(StockDataRoute)]).Object);
        unitOfWork
            .Setup(item => item.Get<InferenceProvider>())
            .Returns(((Mock<IRepository<InferenceProvider>>)repositories[typeof(InferenceProvider)]).Object);
        unitOfWork
            .Setup(item => item.Get<InferenceRoute>())
            .Returns(((Mock<IRepository<InferenceRoute>>)repositories[typeof(InferenceRoute)]).Object);
        unitOfWork
            .Setup(item => item.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var secretProtector = new Mock<ISecretProtector>();
        secretProtector
            .Setup(item => item.Protect(It.IsAny<string>(), It.IsAny<SecretProtectionPurpose>()))
            .Returns((string value, SecretProtectionPurpose _) => $"protected:{value}");
        secretProtector
            .Setup(item => item.TryUnprotect(
                It.IsAny<string>(),
                It.IsAny<SecretProtectionPurpose>(),
                out It.Ref<string?>.IsAny))
            .Returns(true);

        return (
            new InitializationAppService(
                unitOfWork.Object,
                secretProtector.Object,
                new FixedTimeProvider(Now)),
            unitOfWork,
            data);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class InitializationData
    {
        public List<InitializationState> InitializationStates { get; init; } = [];
        public List<ApplicationPreference> Preferences { get; init; } = [];
        public List<PortfolioEntity> Portfolios { get; init; } = [];
        public List<StockDataProviderDefinition> Definitions { get; init; } = [];
        public List<StockDataProvider> StockProviders { get; init; } = [];
        public List<StockDataRoute> StockRoutes { get; init; } = [];
        public List<InferenceProvider> InferenceProviders { get; init; } = [];
        public List<InferenceRoute> InferenceRoutes { get; init; } = [];
    }
}
