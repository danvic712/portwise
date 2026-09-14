using Moq;
using Portwise.Application.Configuration;
using Portwise.Application.Configuration.Contracts;
using Portwise.Application.Configuration.Dtos;
using Portwise.Application.Exceptions;
using Portwise.Application.Initialization;
using Portwise.Application.Initialization.Dtos;
using Portwise.Application.Initialization.Validators;
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
        var openAiProvider = new InferenceProvider
        {
            Id = KnownConfigurationIds.InferenceOpenAiProvider,
            Name = "OpenAI",
            NormalizedName = InferenceProvider.NormalizeName("OpenAI"),
            BaseUrl = "https://api.openai.com/v1",
            Revision = 1
        };
        var deepSeekProvider = new InferenceProvider
        {
            Id = KnownConfigurationIds.InferenceDeepSeekProvider,
            Name = "DeepSeek",
            NormalizedName = InferenceProvider.NormalizeName("DeepSeek"),
            BaseUrl = "https://api.deepseek.com/v1",
            Revision = 1
        };
        var (service, unitOfWork, _) = CreateService(
            definitions: [definition],
            stockRoutes: stockRoutes,
            inferenceProviders: [openAiProvider, deepSeekProvider],
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
                    openAiProvider.Id,
                    new SecretUpdateRequest(SecretCodes.Replace, "openai-key")),
                 new CompleteInferenceProviderRequest(
                    deepSeekProvider.Id,
                    new SecretUpdateRequest(SecretCodes.Replace, "deepseek-key"))],
                [
                    new("chat", openAiProvider.Id, "chat-model"),
                    new("embedding", deepSeekProvider.Id, "embedding-model")
                ]),
            CancellationToken.None);

        Assert.All(
            response.Status.Limitations,
            item => Assert.Equal("configured-unverified", item.StatusCode));
        Assert.DoesNotContain("openai-key", response.Status.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("deepseek-key", response.Status.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("ftshare-key", response.Status.ToString(), StringComparison.Ordinal);
        Assert.Equal(2, openAiProvider.Revision);
        Assert.Equal(2, deepSeekProvider.Revision);
        Assert.Equal(openAiProvider.Id, inferenceRoutes.Single(route => route.Capability == InferenceCapability.Chat).ProviderId);
        Assert.Equal(deepSeekProvider.Id, inferenceRoutes.Single(route => route.Capability == InferenceCapability.Embedding).ProviderId);
        unitOfWork.Verify(item => item.CommitAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_BindsAllStockCapabilitiesToTheOnlyConfiguredProvider()
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
        var (service, unitOfWork, _) = CreateService(
            definitions: [definition],
            stockRoutes: stockRoutes);

        await service.CompleteAsync(
            new CompleteInitializationRequest(
                "zh-CN",
                "system",
                "长期组合",
                [new CompleteStockDataProviderRequest(
                    definition.Id,
                    definition.DisplayName,
                    new SecretUpdateRequest(SecretCodes.Replace, "ftshare-key"))],
                null,
                null,
                null),
            CancellationToken.None);

        var providerIds = stockRoutes.Select(route => route.ProviderId).ToHashSet();
        Assert.Single(providerIds);
        Assert.DoesNotContain(null, providerIds);
        Assert.All(stockRoutes, route => Assert.Equal(2, route.Revision));
        unitOfWork.Verify(item => item.CommitAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_SavesEditableInferenceProviderBaseUrl()
    {
        var provider = new InferenceProvider
        {
            Id = KnownConfigurationIds.InferenceAzureOpenAiProvider,
            Name = "Azure OpenAI",
            NormalizedName = InferenceProvider.NormalizeName("Azure OpenAI"),
            BaseUrl = "https://your-resource.openai.azure.com/openai/v1",
            IsBaseUrlEditable = true,
            Revision = 1
        };
        var routes = Enum.GetValues<InferenceCapability>()
            .Select(capability => new InferenceRoute
            {
                Id = Guid.CreateVersion7(),
                Capability = capability,
                Revision = 1
            })
            .ToList();
        var (service, unitOfWork, _) = CreateService(
            inferenceProviders: [provider],
            inferenceRoutes: routes);

        await service.CompleteAsync(
            new CompleteInitializationRequest(
                "zh-CN",
                "system",
                "Azure 组合",
                null,
                null,
                [new CompleteInferenceProviderRequest(
                    provider.Id,
                    new SecretUpdateRequest(SecretCodes.Replace, "azure-key"),
                    "https://custom-resource.openai.azure.com/openai/v1")],
                [
                    new("chat", provider.Id, "gpt-4.1-mini"),
                    new("embedding", null, null)
                ]),
            CancellationToken.None);

        Assert.Equal("https://custom-resource.openai.azure.com/openai/v1", provider.BaseUrl);
        Assert.Equal(2, provider.Revision);
        Assert.Equal("configured-unverified",
            (await service.GetAsync(CancellationToken.None)).Limitations
                .Single(item => item.CapabilityCode == "chat").StatusCode);
        unitOfWork.Verify(item => item.CommitAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CompleteAsync_RejectsBaseUrlChangeForFixedInferenceProvider()
    {
        var provider = new InferenceProvider
        {
            Id = KnownConfigurationIds.InferenceOpenAiProvider,
            Name = "OpenAI",
            NormalizedName = InferenceProvider.NormalizeName("OpenAI"),
            BaseUrl = "https://api.openai.com/v1",
            IsBaseUrlEditable = false,
            Revision = 1
        };
        var (service, unitOfWork, _) = CreateService(inferenceProviders: [provider]);

        var exception = await Assert.ThrowsAsync<ApplicationErrorException>(() =>
            service.CompleteAsync(
                new CompleteInitializationRequest(
                    "zh-CN",
                    "system",
                    "OpenAI 组合",
                    null,
                    null,
                    [new CompleteInferenceProviderRequest(
                        provider.Id,
                        new SecretUpdateRequest(SecretCodes.Keep, null),
                        "https://other.example/v1")],
                    null),
                CancellationToken.None));

        Assert.Equal(ApplicationErrorCodes.InitializationValidationFailed, exception.ErrorCode);
        unitOfWork.Verify(item => item.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CompleteAsync_SavesMultipleInitialStocksInTheSameCommit()
    {
        var securityRepository = RepositoryMock.Create<Security>([]);
        var positionRepository = RepositoryMock.Create<PortfolioPosition>([]);
        var (service, unitOfWork, _) = CreateService(
            securityRepository: securityRepository,
            positionRepository: positionRepository);

        var response = await service.CompleteAsync(
            new CompleteInitializationRequest(
                "zh-CN",
                "system",
                "长期组合",
                null,
                null,
                null,
                null,
                [
                    new InitialStockRequest(" 000001 ", " szse ", 100),
                    new InitialStockRequest("600000", "SSE", 50)
                ]),
            CancellationToken.None);

        var securities = securityRepository.Invocations
                .Where(invocation => invocation.Method.Name == nameof(IRepository<Security>.AddAsync))
                .Select(invocation => invocation.Arguments[0])
                .OfType<Security>()
                .ToList();
        Assert.Equal(2, securities.Count);
        var security = Assert.Single(securities, item => item.SecurityCode == "000001");
        Assert.Equal("000001", security.SecurityCode);
        Assert.Equal("SZSE", security.ExchangeCode);
        Assert.Equal(MarketCodes.AShare, security.MarketCode);
        Assert.NotEqual(Guid.Empty, security.Id);
        Assert.All(
            securities,
            item => positionRepository.Verify(repository => repository.AddAsync(
                It.Is<PortfolioPosition>(position =>
                    position.SecurityId == item.Id
                    && position.HeldShares == (item.SecurityCode == "000001" ? 100 : 50)
                    && position.CoreShares == position.HeldShares
                    && position.TargetShares == position.HeldShares
                    && position.AverageCostPerShare == 0m),
                It.IsAny<CancellationToken>()), Times.Once));
        Mock.Get(unitOfWork.Object.Get<StockDataSyncJob>())
            .Verify(repository => repository.AddAsync(
                It.Is<StockDataSyncJob>(job =>
                    job.TriggerCode == "initialization"
                    && job.DeduplicationKey == "initialization"
                    && job.StatusCode == "pending"),
                CancellationToken.None), Times.Once);
        unitOfWork.Verify(item => item.CommitAsync(CancellationToken.None), Times.Once);
        Assert.True(response.Status.IsComplete);
    }

    [Fact]
    public async Task CompleteAsync_RejectsAnInvalidInitialStockWithoutWriting()
    {
        var securityRepository = RepositoryMock.Create<Security>([]);
        var positionRepository = RepositoryMock.Create<PortfolioPosition>([]);
        var (service, unitOfWork, _) = CreateService(
            securityRepository: securityRepository,
            positionRepository: positionRepository);

        var exception = await Assert.ThrowsAsync<ApplicationErrorException>(() =>
            service.CompleteAsync(
                new CompleteInitializationRequest(
                    "zh-CN",
                    "system",
                    "长期组合",
                    null,
                    null,
                    null,
                    null,
                    [new InitialStockRequest("123", "SSE", 100)]),
                CancellationToken.None));

        Assert.Equal(ApplicationErrorCodes.InitializationValidationFailed, exception.ErrorCode);
        securityRepository.Verify(repository => repository.AddAsync(
            It.IsAny<Security>(),
            It.IsAny<CancellationToken>()), Times.Never);
        positionRepository.Verify(repository => repository.AddAsync(
            It.IsAny<PortfolioPosition>(),
            It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(item => item.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
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
        IReadOnlyList<InferenceProvider>? inferenceProviders = null,
        IReadOnlyList<InferenceRoute>? inferenceRoutes = null,
        Mock<IRepository<Security>>? securityRepository = null,
        Mock<IRepository<PortfolioPosition>>? positionRepository = null)
    {
        var data = new InitializationData
        {
            InitializationStates = initializationState is null ? [] : [initializationState],
            Portfolios = [],
            Preferences = [],
            Definitions = definitions?.ToList() ?? [],
            StockProviders = [],
            StockRoutes = stockRoutes?.ToList() ?? [],
            InferenceProviders = inferenceProviders?.ToList() ?? [],
            InferenceRoutes = inferenceRoutes?.ToList() ?? [],
            Securities = [],
            Positions = []
        };
        securityRepository ??= RepositoryMock.Create<Security>(data.Securities);
        positionRepository ??= RepositoryMock.Create<PortfolioPosition>(data.Positions);
        var repositories = new Dictionary<Type, object>
        {
            [typeof(InitializationState)] = RepositoryMock.Create(data.InitializationStates),
            [typeof(ApplicationPreference)] = RepositoryMock.Create(data.Preferences),
            [typeof(PortfolioEntity)] = RepositoryMock.Create(data.Portfolios),
            [typeof(StockDataProviderDefinition)] = RepositoryMock.Create(data.Definitions),
            [typeof(StockDataProvider)] = RepositoryMock.Create(data.StockProviders),
            [typeof(StockDataRoute)] = RepositoryMock.Create(data.StockRoutes),
            [typeof(InferenceProvider)] = RepositoryMock.Create(data.InferenceProviders),
            [typeof(InferenceRoute)] = RepositoryMock.Create(data.InferenceRoutes),
            [typeof(Security)] = securityRepository,
            [typeof(PortfolioPosition)] = positionRepository,
            [typeof(StockDataSyncJob)] = RepositoryMock.Create<StockDataSyncJob>([])
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
            .Setup(item => item.Get<Security>())
            .Returns(securityRepository.Object);
        unitOfWork
            .Setup(item => item.Get<PortfolioPosition>())
            .Returns(positionRepository.Object);
        unitOfWork
            .Setup(item => item.Get<StockDataSyncJob>())
            .Returns(((Mock<IRepository<StockDataSyncJob>>)repositories[typeof(StockDataSyncJob)]).Object);
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
                new FixedTimeProvider(Now),
                new InitialStockRequestValidator()),
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
        public List<Security> Securities { get; init; } = [];
        public List<PortfolioPosition> Positions { get; init; } = [];
    }
}
