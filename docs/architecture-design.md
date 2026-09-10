# Portwise 应用架构设计

## 1. 目标与边界

Portwise 是一个面向个人 A 股长期投资者的策略研究与组合记录工具。当前内置股息策略，后端使用 ASP.NET Core Controllers，前端和后端最终由同一个 Host 镜像提供。后端负责持久化用户的组合、持仓、交易和模型数据，并通过 FTShare MCP Adapter 获取外部股票资料、行情、股息和财务数据。

系统只提供可解释的参考结果，不执行交易，也不把外部数据源当作用户持仓或交易记录的归属地。

本文档是当前实现地图；难以逆转的架构选择记录在 [`docs/adr/`](adr/)，领域术语和产品范围记录在根目录 [`CONTEXT.md`](../CONTEXT.md)。实现变更必须同步更新相应 ADR 或本文件，不能让本地 agent 笔记取代受版本控制的决策记录。

## 2. 分层与依赖方向

```text
Portwise Host
├── Application.Shared (Contracts / Dtos / Validators / Exceptions)
├── Application.Setup module
├── Application.Stocks module
├── Application.Portfolio module
├── Application.Recommendations module
└── Infrastructure
    ├── Contracts
    ├── Configurations
    ├── Repositories
    └── FtShare

Application  ──depends on──> Domain
Infrastructure ──depends on──> Application + Domain
Host ──composes──> Application + Infrastructure
Domain ──depends on──> nothing in this solution
```

Host 只负责 HTTP 路由、依赖注入组合和运行时启动；业务规则属于 Application 或 Domain；外部系统和 EF Core 实现属于 Infrastructure。

## 3. 运行时配置

Host 项目的配置文件位于 `src/Portwise/`：

- `appsettings.json`：本地开发默认配置，提供 PostgreSQL 连接字符串示例。
- `appsettings.Production.json`：生产环境非敏感默认配置；实际 PostgreSQL 连接通过 `ConnectionStrings__Default` 环境变量注入。

ASP.NET Core 默认环境名为 `Production`（大小写不敏感）时，会自动加载 `appsettings.Production.json`。环境变量仍作为后置覆盖层，可用于部署时覆盖连接字符串、FTShare MCP 地址和工具参数。两个文件只保存非敏感默认值，FTShare key 不进入源代码、镜像或 Git。

## 3.1 多语言资源

根目录 `locales/` 是跨层共享的文本资源源文件，当前包含 `zh-CN` 和 `en-US` 两个语言目录；每个语言目录按业务领域拆分为 `common.json`、`setup.json`、`stocks.json`、`portfolio.json` 和 `dividend-strategy.json`。异常定义使用稳定的 `error_code` 作为 JSON 键，并支持由 Application 异常提供的命名参数插值。业务领域 JSON 可以同时包含保留的 `ui` 节点，供前端页面读取产品文案；Application 异常目录加载器只读取顶层错误定义并显式忽略 `ui` 节点，避免前端文案改变异常目录语义。错误目录与参数插值仍由独立的 `IApplicationErrorLocalizer` 负责，HTTP 请求的语言协商交给 ASP.NET Core 的 `RequestLocalizationMiddleware`。

Application 项目通过 `EmbeddedResource` 将根目录 `locales/**/*.json` 编译嵌入 `Portwise.Application.dll`。运行时只从程序集资源读取文本，不读取可被容器或请求任意替换的本地文件。Host 注册 `RequestLocalizationOptions`，由 ASP.NET Core 根据 `Accept-Language` 的标准质量权重选择受支持语言；不支持、质量为 0 或缺失时回退到 `zh-CN`，并在 ProblemDetails 扩展中返回 `CultureInfo.CurrentUICulture` 的 canonical name。后台同步、CLI 和测试等非 HTTP 入口仍可通过 `IApplicationErrorLocalizer` 显式选择语言或使用默认语言。

根目录 JSON 是跨层共享的文本源；前端不直接读取 Application DLL，前端构建可按需复制/导入同一组资源保持显示文本一致。当前生产前端通过 API 消费后端返回的 `error_code`、`locale` 和安全的 `detail`，不包含 FTShare key 或后端凭据。

### 3.1.1 本地化完整性约束

- Web 页面、组件和格式化工具不得内置用户可见的中文或英文默认文案；必须从当前 locale 的 `ui` 节点读取。
- API 错误必须传递稳定的 `error_code`，由 `IApplicationErrorLocalizer` 按 `Accept-Language` 选择 `detail`；验证失败使用对应语言的 `validation_message`，不能把 FluentValidation 或领域异常原文直接返回给用户。
- Domain 推荐计算只返回稳定的解释代码（例如 `unavailable`、`confirmed:<zone>`），由 Web 层使用 `dividend-strategy.ui.overview.decision.explanations` 渲染；领域层不依赖 UI 语言资源。
- 股票未同步时 API 保持名称为空，由 Web 的 `stocks.ui.identity.pendingName` 负责显示本地化占位名称；交易所名称同样由 locale 提供。
- 金额、数字、百分比和日期格式必须跟随当前 `portwise-locale`，不得固定为单一语言格式。
- Backend validation, domain, infrastructure and health-check diagnostics must not embed Chinese prose. They use English diagnostic text or stable codes; Chinese locale resources remain the only source for user-facing Chinese responses. External-provider protocol aliases are not UI copy and must be treated as data normalization rules.

## 4. Repo 分层布局

Repo 根目录还包含跨层共享的多语言资源：

```text
locales/
├── zh-CN/
└── en-US/
```

源代码布局如下：

```text
src/
├── Portwise.Domain/
│   ├── Contracts/                      # Repository、Uow 等持久化抽象
│   │   ├── IRepository.cs
│   │   └── IUow.cs
│   ├── Models/                         # 数据库实体模型
│   │   ├── Portfolio.cs
│   │   ├── PortfolioPosition.cs
│   │   ├── Security.cs                    # 可选 sector_code
│   │   ├── ModelParameterSet.cs
│   │   ├── PriceObservation.cs
│   │   ├── DividendEvent.cs
│   │   ├── FinancialSnapshot.cs
│   │   ├── CashLedgerEntry.cs
│   │   ├── RecommendationSnapshot.cs
│   │   └── PortfolioTrade.cs
│   ├── Codes/                          # 业务代码表（状态、区域和建议）
│   ├── Enums/                           # 真正的枚举；当前暂无枚举
│   ├── Exceptions/                     # 跨层可识别的持久化异常
│   ├── Portfolio/                      # 持仓领域规则
│   ├── Securities/                     # A 股标识领域规则
│   ├── DividendModel/                  # 股息率与价格区域计算
│   └── Recommendations/                # 推荐规则组合入口
│       └── RecommendationModule.cs
├── Portwise.Application/
│   ├── Contracts/                      # 仅跨 module 的错误、本地化和诊断 Interface
│   │   ├── IApplicationErrorCatalog.cs
│   │   ├── IApplicationErrorLocalizer.cs
│   │   └── IDiagnosticContext.cs
│   ├── Dtos/                           # 仅跨 module 的共享 DTO
│   │   └── StockHoldingSnapshot.cs
│   ├── Validators/                     # 跨 module 的通用规则和错误格式化
│   │   ├── AShareValidationRules.cs
│   │   └── ValidationErrorFormatter.cs
│   ├── Exceptions/                     # Application 统一异常模型
│   ├── Localization/                   # 错误目录与本地化实现
│   ├── ApplicationServiceCollectionExtensions.cs
│   ├── Setup/                          # Setup module：contract、DTO、validation、实现
│   │   ├── Contracts/ISetupAppService.cs
│   │   ├── Dtos/{InitialHoldingInput,SetupRequest,SetupResult,SetupStatus,SetupStockRequest,SetupStockResult}.cs
│   │   ├── Validators/{InitialHoldingInput,SetupRequest,SetupStockRequest}Validator.cs
│   │   └── SetupAppService.cs
│   ├── Stocks/                         # Market Data module：股票事实、关注列表和同步
│   │   ├── Contracts/                   # 股票资料与同步 Interface
│   │   ├── Dtos/                        # 股票资料、行情、股息、财务和同步结果
│   │   ├── Validators/                  # 股票事实同步请求验证器
│   │   ├── StockWatchlistAppService.cs
│   │   ├── StockFactSyncAppService.cs
│   │   ├── StockPriceObservationAppService.cs
│   │   ├── StockDividendEventAppService.cs
│   │   ├── StockFinancialSnapshotAppService.cs
│   │   ├── StockDailyDataSyncAppService.cs
│   │   ├── StocksMapper.cs
│   │   └── DailySyncSchedule.cs
│   ├── Portfolio/                      # Portfolio module：现金流水、交易和持仓变更
│   │   ├── Contracts/{IBudgetAppService,IPortfolioTradeAppService}.cs
│   │   ├── Dtos/{BudgetSummary,CashLedgerEntryResult,PortfolioTradeResult,RecordCashLedgerEntryRequest,RecordPortfolioTradeRequest}.cs
│   │   ├── Validators/{RecordCashLedgerEntryRequest,RecordPortfolioTradeRequest}Validator.cs
│   │   ├── PortfolioMapper.cs
│   │   ├── BudgetAppService.cs
│   │   └── PortfolioTradeAppService.cs
│   └── Recommendations/                # Recommendation module：模型、单股分析和组合建议
│       ├── Contracts/                   # 推荐与模型参数 Interface
│       ├── Dtos/                        # 分析、建议、快照和模型参数 DTO
│       ├── Validators/                  # 分析和模型参数请求验证器
│       ├── RecommendationsMapper.cs
│       ├── StockModelParameterAppService.cs
│       ├── StockAnalysisAppService.cs
│       ├── StockRecommendationAppService.cs
│       ├── PortfolioAllocationAppService.cs
│       ├── PortfolioRecommendationAppService.cs
│       └── RecommendationSnapshotAppService.cs
├── Portwise.Infrastructure/
│   ├── Contracts/                      # Infrastructure Adapter Interface
│   │   ├── IDatabaseLifecycle.cs
│   │   └── IFtShareMcpToolInvoker.cs
│   ├── Configurations/                 # EF Core Fluent Entity Configuration
│   ├── Repositories/                   # Repository、Uow 实现
│   │   ├── EFRepository.cs
│   │   └── EFUow.cs
│   ├── InfrastructureServiceCollectionExtensions.cs
│   ├── Migrations/                     # EF Core 可追踪数据库迁移
│   ├── DatabaseLifecycle.cs            # 数据库迁移和连接检查
│   ├── PortwiseDbContext.cs     # EF Core DbContext
│   ├── Exceptions/                     # Infrastructure Adapter 异常
│   │   └── FtShareProviderException.cs
│   └── FtShare/                        # FTShare MCP Adapter 实现
├── Portwise/                    # ASP.NET Core Controllers Host
    ├── appsettings.json                # 本地默认配置
    ├── appsettings.Production.json     # 生产环境配置
    ├── Controllers/                    # 业务 HTTP Controller
    │   ├── SetupController.cs
    │   ├── StocksController.cs
    │   ├── BudgetsController.cs
    │   ├── RecommendationsController.cs
    │   └── PortfolioController.cs
    ├── Background/                     # ASP.NET Core 后台调度
    │   ├── DailyStockDataSyncHostedService.cs
    │   ├── StockDataSyncRunner.cs
    │   ├── StockDataSyncBackgroundService.cs
    │   └── StockDataSyncTaskQueue.cs
    ├── Contracts/                      # Host 层可替换边界
    │   └── IStockDataSyncRunner.cs
    ├── Diagnostics/                    # 隐私感知的 Activity 诊断上下文
    │   ├── ActivityDiagnosticContext.cs
    │   └── PortwiseActivitySource.cs
    ├── ExceptionHandling/              # 异常编排与 ProblemDetails 输出
    │   └── ApplicationExceptionHandler.cs
    ├── HostServiceCollectionExtensions.cs
    ├── WebApplicationExtensions.cs
    ├── HealthChecks/                   # 原生 ASP.NET Core Health Checks
    └── wwwroot/                        # 前端构建产物（由 Portwise.Web 构建写入）
└── Portwise.Web/                # Vite + React + shadcn/ui 前端源码
│   ├── src/features/                   # 每个 feature 自有页面、组件、API、CSS
│   ├── src/components/                 # 跨页面的公共组合组件
│   │   ├── site-header.tsx             # 公共顶部导航与主题/语言操作
│   │   ├── site-footer.tsx             # 公共页脚与数据口径提示
│   │   ├── page-frame.tsx              # 页面级公共布局组合
│   │   ├── page-heading.tsx            # 页面标题与区块标题
│   │   ├── site-navigation.ts          # 公共导航配置
│   │   └── ui/                         # shadcn/ui 源码组件
│   ├── openapi/                        # 从 Host Controller 元数据生成的版本化 HTTP 合约
│   │   └── portwise_v1.json
│   └── src/lib/                        # Axios client、HTTP 合约适配器和共享展示工具
│       ├── api-contract.generated.ts  # openapi-typescript 生成；禁止手工修改
│       ├── api-contract.ts             # 生成合约到前端运行时模型的唯一适配器
│       ├── api-types.ts                # feature 使用的领域友好类型别名
│       ├── navigation.ts                # 路由、Setup gate、历史记录和查询参数 Adapter
│       ├── recommendation-display.ts  # 操作建议展示语义与价格阶梯
│       └── stock-display.ts           # 股票身份与名称展示
```

Application 的业务实现按业务能力归并到 `Setup`、`Stocks`、`Portfolio` 和 `Recommendations` 四个 module。每个 module 共置自己拥有的 Interface、DTO、Validator、实现和测试；因此修改一个用例时，主要知识和验证都集中在同一目录。module 内的 public type 使用对应的 `Portwise.Application.<Module>.(Contracts|Dtos|Validators)` namespace，`ModuleNamespaceArchitectureTests` 会阻止新的类型泄漏回技术桶。`Contracts`、`Dtos` 和 `Validators` 根目录只保留真正跨 module 的错误、本地化、诊断、共享持仓 DTO 和通用 A 股规则，避免技术桶重新变成所有业务的汇聚点。目录归并不等于合并 HTTP 契约：价格、股息和财务同步仍然保持独立的 Interface 与 AppService，因为它们具有不同的数据校验、幂等键和结果类型；资料、行情、股息和财务四类事实的共同摄取、Security 解析、FTShare 调用、幂等写入和逐类失败策略由 `IStockFactSyncAppService` / `StockFactSyncAppService` 这个深模块承载，三个 HTTP AppService 只是验证后转发。单股分析、组合分配和建议快照也保持独立的用例边界。前端按相同的业务边界拆分 feature，但只通过版本化 HTTP API 访问后端，不直接引用 Application 或 Infrastructure。

前端公共页面框架由 `SiteHeader`、`SiteFooter`、`PageFrame`、`PageTitle`、`SectionHeading` 和 `site-navigation` 组成；`application-shell.tsx` 是负责 Setup gate、路由选择和 feature 懒加载的应用编排 module，不是公共视觉组件。所有页面通过 `PageFrame` 复用框架，页面专属状态与布局留在对应 feature。`src/lib/navigation.ts` 是页面 shell 的导航 module：它集中路由识别、Setup gate 所需的路径判断、浏览器 history/popstate Adapter、查询参数更新和组合股票选择的 session 持久化；`App.tsx` 只负责提供 LocaleProvider，feature 通过 `onNavigate`/`onReplaceQuery` seam 操作导航，不直接写 `window.history` 或 `sessionStorage`。查询参数优先于旧 session 选择，避免从今日决策跳转到组合页面时恢复错误股票。操作建议的代码归类、买卖方向、展示状态、价格区间和未知代码降级统一由 `src/lib/recommendation-display.ts` 提供；页面和组件只消费归一化后的展示结果，不自行解析 `recommendation_code` 或 `price_zone_code`。`index.css` 只承载 token、reset 和跨页面共享原子样式；今日决策页的布局、等待态、就绪态、骨架屏、装饰和响应式样式统一位于 `features/recommendations/recommendations.css`。交互控件优先使用 `src/components/ui` 中的 shadcn/ui 原语，feature 样式只负责业务变体与布局。

前端使用 Node `24.16.0` 与 pnpm `12.3.4` 构建静态资源，输出到 Host 的 `wwwroot/`；该目录是构建产物并保持本地生成，不提交源代码仓库。根目录 `Dockerfile` 使用 Node Alpine、.NET SDK Alpine 和 ASP.NET Core Alpine 三阶段构建：前两阶段只负责编译，最终镜像只保留 .NET publish 输出，因此不会携带 Node、pnpm、源码或测试依赖。单镜像构建流程必须先完成前端构建，再执行 ASP.NET Core publish；开发预览使用 Vite proxy 将 `/api` 转发到本地 Host。

前端 HTTP 类型不再手工复制 Controller DTO。Host 构建通过 `Microsoft.Extensions.ApiDescription.Server` 调用同一组 Controller、版本元数据和 `AddOpenApi` 配置，生成并提交 `src/Portwise.Web/openapi/portwise_v1.json`；前端的 `pnpm api:generate` 使用 `openapi-typescript` 生成 `src/lib/api-contract.generated.ts`。`api-contract.ts` 是生成类型与浏览器运行时 JSON 之间唯一的 Adapter，负责把 wire numeric（`number | string`）归一为 UI 使用的 `number`；feature 的 Axios module 只引用 `api-types.ts` 的领域友好别名，不直接依赖生成文件。新增或修改 Controller DTO 时，后端构建和前端生成都会暴露合约漂移，避免两套手工类型长期分叉。

`Stocks` 同时承载交易日同步编排，因为该编排只围绕关注股票的外部事实更新；交易日同步通过 `IStockFactSyncAppService.SyncAsync` 一次传递单只股票的规范化引用，并消费包含资料、行情、股息、财务结果和逐类失败的 `StockFactSyncResult`。如果未来出现多个互不相关的调度任务，再单独引入 `Operations` 模块。`StockModelParameterAppService` 与单股分析、组合分配、建议快照同属 `Recommendations` module，因为模型参数是建议规则的输入；Portfolio module 只拥有现金流水、交易和持仓不变量。

各层的依赖注入通过对应的扩展类集中注册：Application 使用 `ApplicationServiceCollectionExtensions.AddPortwiseApplication`，Infrastructure 使用 `InfrastructureServiceCollectionExtensions.AddPortwiseInfrastructure`，Host 使用 `HostServiceCollectionExtensions.AddPortwiseHost`。Application 根扩展只负责 FluentValidation、跨 module 的本地化/诊断和 `TimeProvider`，再调用 `AddPortwiseSetupModule`、`AddPortwiseStocksModule`、`AddPortwisePortfolioModule`、`AddPortwiseRecommendationsModule`；每个 module 自己拥有实现到 Interface 的注册清单，新增用例不会把具体类型重新塞回技术桶。Host 另提供 `HostServiceCollectionExtensions.AddPortwise(WebApplicationBuilder)` 作为启动组合入口，按固定顺序组合三层注册。Host 对 `WebApplication` 的异常处理中间件、Controller/健康检查路由和数据库 migration 统一放在 `WebApplicationExtensions`；手动、每日定时和 Setup 后台同步都通过 `StockDataSyncRunner` 集中创建 scoped 生命周期并解析应用服务，运行器统一串行化执行、run ID、诊断上下文和取消语义，避免不同入口同时写库；`Program.cs` 只保留配置构建、组合扩展调用、应用构建和启动顺序。

公共基础能力也遵循相同的组合边界：Swagger/Serilog 注册在 `HostServiceCollectionExtensions`，Swagger UI、Serilog HTTP 请求日志中间件和其他 `WebApplication` 行为在 `WebApplicationExtensions`；Mapperly 映射定义由 `StocksMapper`、`PortfolioMapper` 和 `RecommendationsMapper` 分别归属各自 module，由构建期生成实际映射代码。

约束：

1. 本项目声明的所有 Interface 必须位于所属 module 的 `Contracts/` 文件夹；跨 module 的 Interface 才放在项目根 `Contracts/`；一个文件只放一个 Interface。
2. 所有 DTO 必须位于所属 module 的 `Dtos/` 文件夹；跨 module 的 DTO 才放在项目根 `Dtos/`；一个 DTO 文件只允许包含一个 DTO class/record。
3. 所有自定义 Exception 必须位于项目根 `Exceptions/` 文件夹；一个文件只放一个 Exception。错误码较多时使用通用异常类型和集中式错误码/参数工厂，不为每个业务错误码创建一个同构异常类。
4. 如果新增真正的枚举，必须放到所属项目的 `Enums/` 文件夹；字符串业务代码集中放在 `Codes/`，不创建空的伪实现。
5. `Application` 的业务实现类使用 `*AppService` 后缀；对应 Interface 使用 `I*AppService`。
6. 数据访问实现按照 `salary-insights` 拆分到 `Infrastructure/Repositories/` 和 Infrastructure 根目录的 DbContext，不使用单独的 `Persistence` 命名层。

## 5. Interface 与 DTO 规则

### 5.1 Contracts 归属

- `Domain/Contracts`：跨层需要依赖的通用持久化抽象，包括 `IRepository<TEntity>` 和 `IUow`。
- `Application/{Setup,Stocks,Portfolio,Recommendations}/Contracts`：各 module 的用例和外部资料 Interface；只有错误、本地化和诊断等跨 module Interface 位于 `Application/Contracts`。
- `Infrastructure/Contracts`：Infrastructure 内部 Adapter 的可替换抽象，包括 `IFtShareMcpToolInvoker`。

Application 不认识 EF Core、PostgreSQL、HTTP 或 MCP SDK。Host 只依赖 Application 的 AppService Interface 和 Infrastructure 的组合注册扩展，不直接使用数据访问实现。

### 5.2 DTO

Application 的 DTO 按所属 module 放在 `Application/{Setup,Stocks,Portfolio,Recommendations}/Dtos/`，用于 Controller 与用例之间的输入输出，以及外部资料 Adapter 规范化后的结果；`Application/Dtos/` 只保留跨 module 的共享 DTO。DTO 不承担数据库实体职责，也不包含 Repository、DbContext 或 MCP 客户端。

一个 DTO 文件只能包含一个 DTO 类型，文件名必须与类型名一致。

### 5.3 公共对象映射

- Riok.Mapperly 是本项目的默认对象映射框架。它在编译期生成映射代码，减少运行时反射和隐式约定，适合当前的单镜像部署。
- 映射声明放在拥有目标 DTO 的 module 根目录（例如 `Application/Stocks/StocksMapper.cs`），使用 partial mapper 和显式的 MapProperty/额外映射参数表达名称差异；不再设置跨 module 的 `ApplicationMapper` 汇聚点。
- Application 返回 DTO，不返回 Domain Model；AppService 负责领域编排，Mapper 只负责数据形状转换，不包含查询、验证、事务或业务规则。
- 需要上下文才能完成的输出映射可以使用额外映射参数；仍然无法由通用 Mapper 表达的派生计算结果，应保留在 AppService/Domain，不要强行塞入映射器。
- 新增 DTO 或 Model 字段时，必须检查 Mapperly 的编译期诊断并补充对应映射；不得通过关闭警告掩盖未映射字段。

## 6. Uow 与 Repository 设计

本项目参考 `salary-insights` 的通用 EF Core 数据访问模式，但保留本项目的 `IUow` 命名和“所有数据访问只能通过 Uow”的约束。

### 6.1 Domain Contracts

```csharp
public interface IRepository<TEntity> where TEntity : class
{
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, object>>? orderBy = null,
        bool descending = false,
        CancellationToken cancellationToken = default,
        bool asNoTracking = true);

    Task<TEntity?> SingleOrDefaultAsync(
        CancellationToken cancellationToken = default,
        bool asNoTracking = true);

    Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default,
        bool asNoTracking = true);

    Task<IReadOnlyList<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        IReadOnlyList<Expression<Func<TEntity, object>>>? orderBy = null,
        bool descending = false,
        CancellationToken cancellationToken = default,
        bool asNoTracking = true);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    void Remove(TEntity entity);

    Task<int> RemoveWhereAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);
}

public interface IUow
{
    IRepository<TEntity> Get<TEntity>() where TEntity : class;

    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
```

`IRepository<TEntity>` 提供实体集合的行为型查询和写入能力；`IUow.Get<TEntity>()` 是 Application 获取 Repository 的唯一入口。Application 只提交谓词、排序选择器列表和追踪意图，不接触 `IQueryable`、`DbSet`、`Include` 或 EF Core 异步扩展。Application 不注入具体 Repository，不保留按用例命名的专用 Repository Interface。

读操作默认使用 no-tracking；只有需要在同一 UoW 中修改已加载实体时才显式传入 `asNoTracking: false`。返回值使用已完成的集合或实体任务，不把查询 provider 延迟到 Application。

### 6.2 Infrastructure 实现

```text
SetupAppService
      │
      ▼
     IUow
      │
      ├── Get<Portfolio>() ─────────────┐
      ├── Get<Security>()               │
      ├── Get<PortfolioPosition>()      │
      └── Get<ModelParameterSet>()      │
                                       ▼
                              EFUow / EFRepository<TEntity>
                                       │
                                       ▼
                             PortwiseDbContext
                                       │
                                       ▼
                         PostgreSQL 17 / public
```

- `EFUow` 使用按实体类型缓存的 lazy Repository，与 `salary-insights` 的 `ConcurrentDictionary<Type, Lazy<object>>` 模式一致。
- `EFRepository<TEntity>` 是真正的 EF adapter：在内部组合 `DbContext.Set<TEntity>()`、过滤、排序、追踪策略和 EF Core 异步执行，然后只返回 Repository 合约要求的结果。
- `EFUow.CommitAsync` 统一调用 `SaveChangesAsync`；数据库更新失败在 Infrastructure 转换为 Domain 的 `UnitOfWorkCommitException`，并只标记 PostgreSQL SQLSTATE `23505` 唯一约束失败；Application 不引用 `DbUpdateException`。
- 一个用例的多个实体写入在一次提交中完成；Repository 不提前提交，也不把可延迟执行的查询对象交给调用方。
- `PortwiseDbContext` 位于 Infrastructure 根目录；`EFRepository<TEntity>` 和 `EFUow` 位于 `Infrastructure/Repositories/`，均为 Infrastructure 内部实现，避免 Host/Application 绕过 `IUow` 直接访问 DbContext。
- `DatabaseLifecycle` 位于 Infrastructure 根目录，负责数据库连接检查和执行 EF Core migration；它与 `EFUow` 分离，避免业务事务抽象承担宿主生命周期职责。
- Host 的 `/healthz`、`/readyz` 使用 ASP.NET Core 原生 Health Checks；数据库健康检查通过 Infrastructure 的 `IDatabaseLifecycle.CanConnectAsync` 实现。健康检查是运行状态入口，不属于业务 API 版本范围。
- 数据库健康检查刻意手写为 `DatabaseHealthCheck : IHealthCheck` 而不是使用官方 `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` 包的 `AddDbContextCheck<TContext>()`：`PortwiseDbContext` 是 Infrastructure 内部类型（`internal sealed`），Host 只能通过 `IDatabaseLifecycle` 这个 Infrastructure Contract 访问数据库连通性；引入 `AddDbContextCheck<TContext>()` 需要把 `DbContext` 类型暴露给 Host，会破坏“Host/Application 不直接访问 DbContext”的封装边界。这是明确的架构取舍，不是遗漏标准实现。
- Host 启动时只能通过 Infrastructure 的 `IDatabaseLifecycle.MigrateAsync` 应用待执行 migration，不直接解析 DbContext，也不让 `IUow` 承担数据库生命周期职责。
- `Migrations/` 保存由当前模型生成的 PostgreSQL 初始 schema 及后续可审查、可回放的结构变更；migration history 固定使用 `public.ef_migrations (migration_id, product_version)`，未来修改表结构时必须生成新的 EF Core migration，不能只修改 Fluent Configuration。
- PostgreSQL 17+ 是独立运行时依赖；Compose 通过 named volume 持久化数据库，生产环境使用外部或托管 PostgreSQL。Host 镜像只保留可选文件日志目录，不保存关系数据。

## 7. Domain Models 与 Fluent 配置

数据库实体位于 `Domain/Models/`，只保存实体状态，不依赖 EF Core。所有表名、列名、长度、精度、索引、主键和外键关系均位于 Infrastructure 的独立 Fluent Configuration 文件中：

- `Configurations/PortfolioConfiguration.cs`
- `Configurations/PortfolioPositionConfiguration.cs`
- `Configurations/SecurityConfiguration.cs`
- `Configurations/ModelParameterSetConfiguration.cs`
- `Configurations/PriceObservationConfiguration.cs`
- `Configurations/DividendEventConfiguration.cs`
- `Configurations/FinancialSnapshotConfiguration.cs`
- `Configurations/CashLedgerEntryConfiguration.cs`
- `Configurations/RecommendationSnapshotConfiguration.cs`
- `Configurations/PortfolioTradeConfiguration.cs`

`PortwiseDbContext.OnModelCreating` 使用：

```csharp
modelBuilder.ApplyConfigurationsFromAssembly(
    typeof(PortwiseDbContext).Assembly);
```

因此实体类不使用 Data Annotation，也不需要知道数据库表结构。新增实体时必须同时新增对应的 `IEntityTypeConfiguration<TEntity>` 文件；一个配置文件只负责一个实体。

V1 只有一个组合上下文。`PortfolioConfiguration` 使用 `portfolio_scope` 默认值和唯一索引在数据库层强制单组合不变量；`SetupAppService` 仍先给出正常的已完成错误，若并发初始化在提交阶段触发唯一约束，则把 Infrastructure 的 commit 错误转换为相同的 `setup_already_completed` Application 错误。

当前基础关系：

```text
Portfolio 1 ───────────── * PortfolioPosition * ───────────── 1 Security
Portfolio 1 ───────────── * ModelParameterSet * ───────────── 1 Security
Security 1 ───────────── * PriceObservation
Security 1 ───────────── * DividendEvent
Security 1 ───────────── * FinancialSnapshot
Portfolio 1 ──────────── * CashLedgerEntry
Security 0..1 ────────── * CashLedgerEntry
Portfolio 1 ──────────── * RecommendationSnapshot
Security 1 ───────────── * RecommendationSnapshot
ModelParameterSet 0..1 ─ * RecommendationSnapshot
Portfolio 1 ──────────── * PortfolioTrade
Security 1 ───────────── * PortfolioTrade
```

组合删除持仓采用级联关系；股票删除持仓采用限制关系；股票的 `ExchangeCode + SecurityCode` 具有唯一索引。

## 8. Application 用例数据流

首次建账的调用路径如下：

```text
Controller POST /api/v1/setup
        │
        ▼
ISetupAppService
        │
        ▼
SetupAppService
        ├── IUow.Get<Portfolio>() 检查是否已完成建账
        ├── IUow.Get<TEntity>() 添加组合、股票占位资料和期初持仓
        ├── IUow.CommitAsync() 一次提交
        └── IStockDataSyncScheduler.TrySchedule() 投递后台同步触发器
```

Setup 不调用 `IStockDataProvider`，因此 FTShare 未配置、暂时不可用或响应较慢都不会阻塞用户完成初始化。Setup 只保存 A 股代码、交易所、A-share/CNY 基础占位值和可选期初持仓，并在提交成功后以非阻塞方式投递后台同步；队列已满时仅把 `stockDataSyncScheduled` 返回为 `false`，初始化数据仍然保留，后续交易日同步和手动同步仍可尝试。后台同步再通过 `IStockFactSyncAppService` 获取并校验每只股票的资料、行情、股息和财务数据，资料成功后以显式 `asNoTracking: false` 加载并更新 `Security`，缺失资料时关注列表显示 `待同步 {securityCode}`，不伪造股票名称。

### 8.1 Controller 与请求验证

Controller 只负责 HTTP 绑定、调用 `I*AppService` 和返回响应，不包含业务计算、数据库访问、外部调用或手写输入判断。来自 HTTP、MCP 和定时任务的请求参数在 Application 边界通过 FluentValidation 验证；验证器位于所属 module 的 `Validators/`（跨 module 的通用规则位于 `Application/Validators/`），并通过 Application 程序集扫描注册。

FluentValidation 负责请求形状、字段范围、跨字段关系和集合重复项；Domain 仍负责不可绕过的业务不变量。验证失败转换为统一的 400 ProblemDetails，已知 Application 异常由 Host 的统一异常处理器映射为稳定的 HTTP 响应，避免在每个 Controller 中复制异常分支。

### 8.2 Watchlist 查询

`GET /api/v1/stocks` 通过 `IStockWatchlistAppService` 返回已配置的 A 股股票，按股票代码和交易所稳定排序；如果存在对应的 `PortfolioPosition`，响应同时包含当前持股、核心仓、目标股数和平均成本，否则 `holding` 为 `null`。Controller 不直接查询 `Security` 或 `PortfolioPosition`，股票资料和持仓摘要由 Application 用例统一组装。

### 8.3 股票模型参数

`POST /api/v1/stocks/model-parameters` 为已配置股票保存一个不可覆盖的模型参数版本；参数按 `portfolio_id + security_id + effective_from_date` 唯一。`GET /api/v1/stocks/{securityCode}/{exchangeCode}/model-parameters` 返回当前日期已经生效的最新版本，未来版本不会提前使用。四个收益率阈值必须满足 `strong_buy > accumulation > partial_trim > aggressive_trim > 0`，预算、持仓上限、现金保留比例和交易费用参数均由 FluentValidation 与 Domain 规则共同约束。

Application 只返回 `StockModelParameterSet` DTO，不返回 `ModelParameterSet` 实体；重复生效日期返回 409，未配置股票返回 404，未完成首次建账返回 409。

### 8.4 收盘行情快照

`POST /api/v1/stocks/{securityCode}/{exchangeCode}/price-observations/sync` 通过 `IStockDataProvider.GetMarketDataAsync` 获取 FTShare 规范化的收盘行情，并按 `security_id + trading_date` 幂等保存到 `price_observations`。快照包含收盘价、交易日期、观测时间、来源记录和数据质量代码；缺少价格、日期或来源信息时不会提交数据库。

同一股票同一交易日重复同步时直接返回已有快照，不重复写入。外部行情不可用映射为 503，未配置股票映射为 404，请求参数仍由 FluentValidation 验证。

### 8.5 股息事实同步

`POST /api/v1/stocks/{securityCode}/{exchangeCode}/dividend-events/sync` 通过 `IStockDataProvider.GetDividendEventsAsync` 获取 FTShare 股息事实，并按 `security_id + source_record_id` 幂等保存到 `dividend_events`。事件保留股息类型、实施状态、公告/除息/发放日期、特别股息标记、来源和数据质量，拟派与已取消事件不会在同步阶段被改写为已实施。

批量同步先完成所有事件的身份和领域校验，再统一提交新增事件；空列表表示本次来源没有返回事件，不产生写入。后续 TTM 实际每股股息计算只选择已实施、常规现金且有有效除息日期的事件。

### 8.6 财务事实同步

`POST /api/v1/stocks/{securityCode}/{exchangeCode}/financial-snapshots/sync` 通过 `IStockDataProvider.GetFinancialSnapshotsAsync` 获取带数据日期的 FTShare 财务事实，并按 `security_id + data_as_of_date` 幂等保存到 `financial_snapshots`。快照包含盈利、股息支付率、三年平均股息支付率、P/B（PB）、ROE、来源和数据质量。

财务数据缺失或外部服务不可用不会产生部分提交；支付率原始值保留在事实表中，由 Domain 可靠性规则判断是否通过或失败。

### 8.7 组合现金流水与预算摘要

`POST /api/v1/budgets/entries` 通过 `IBudgetAppService` 记录组合层的实际现金流水，支持预算入金、已收到股息、买入、卖出、费用和现金调整。股票相关流水必须关联已配置的 A 股；代码和流水类型/方向由 FluentValidation 与 Domain 规则共同校验。`GET /api/v1/budgets/summary` 按组合汇总流入和流出，并返回 `cash_balance_amount = total_inflow_amount - total_outflow_amount`。它是实际现金流水余额，不是已经扣除现金储备的可用预算；组合建议用例再根据组合市值和当前有效参数的最大 `cash_reserve_ratio` 计算 `available_budget_amount`。

现金流水是用户实际资金的来源；预计股息不会自动写入流水，也不会因为 TTM 股息存在而增加预算。该摘要为股票分析和组合建议提供可追溯的预算输入；现金保留比例在建议计算中应用，再投资比例和自动记录实际股息仍由后续现金流水录入流程负责。

### 8.8 当前股票分析

`GET /api/v1/stocks/{securityCode}/{exchangeCode}/analysis` 通过 `IStockRecommendationAppService` 编排单股结果：`IStockAnalysisAppService` 读取当前已生效模型参数、按交易日期去重后的最近两个有效行情、股息事实和可选持仓，组装为 Domain `RecommendationModule.CalculateStock` 的输入，由纯计算模块生成 TTM 实际每股股息、当前股息率及四个参考价格边界；随后由单股推荐用例调用 `IPortfolioAllocationAppService` 在单股票范围内计算建议股数。`StockAnalysisResult` 只承载可复用的单股分析事实，`StockRecommendationResult` 在外层承载建议买卖股数、金额和预计手续费，避免把组合分配结果混入分析 DTO。`ObservedPriceZoneCode` 是最新有效收盘价的直接区域，`PriceZoneCode` 只有在最近两个有效交易日一致时才有值；未确认时不生成买入或减仓股数。历史回放中的股息和财务事实还必须满足 `published_at` 不晚于 `data_as_of_date`；缺失公开时间的事实仍可参与当前 V1 计算，但不应被伪造为有明确公开时点。

分析结果明确区分 `unavailable`、`cautious`、`failed`、`re_evaluate` 和价格区域；取消分红时可靠性代码为 `failed`，模型状态单独为 `re_evaluate`，建议代码为 `re_evaluate`。当前没有完整股息可靠性财务资料时只返回谨慎参考和 `no_action`，不生成买卖股数。单股分析阶段不读取组合现金或全组合市值，也不计算交易数量；`RecommendationModule.CalculateStock` 负责在 Domain 内组合单股规则，`RecommendationModule.AllocatePortfolio` 负责在 Domain 内组合预算、集中度和交易数量规则。两个入口都是纯计算模块，Application 的 `*AppService` 只负责读取事实、组装输入和映射输出，避免把业务规则重新散落到用例编排中。`StockAnalysisResult.SecurityId` 是后续 Application 阶段传递的规范本地身份。多股票之间的资金排序和集中度竞争由组合建议用例统一处理，建议快照由独立用例保存。

### 8.9 多股票组合建议

`GET /api/v1/recommendations` 通过 `IPortfolioRecommendationAppService` 读取关注列表、每只股票的单股分析结果和组合预算，再把这些已按股票代码与交易所对齐的输入交给 `IPortfolioAllocationAppService`。前者是组合建议用例编排，后者是 Application 适配器：它在组合范围读取有效模型参数后调用 Domain `RecommendationModule.AllocatePortfolio`，由该深模块独立负责本期预算、行业/单股约束、交易数量和稳定排序。资金分配顺序固定为强买入区、分批加仓区，再按可靠性通过状态和目标股数缺口排序；相同条件保持关注列表的稳定顺序，不使用随机排序。

组合建议会先从现金流水余额扣除组合现金保留比例，再逐只应用预算比例、单股/单次/单期金额上限、行业/单股/单期金额上限、交易单位和手续费，并把已分配的买入金额从剩余预算中扣除。现金保留比例虽然按股票参数保存，但组合计算取当前有效参数中的最大值。减仓仍保护核心仓，不占用买入预算。`Security.SectorCode` 来自 FTShare 股票资料的可选 `sector_code`/`industry_code` 字段；缺失行业资料时跳过行业上限，不推断或伪造行业归属。

`POST /api/v1/recommendations/snapshots` 使用同一份组合建议生成唯一 `model_run_id`，在一个 UoW 提交中保存每只股票的 `RecommendationSnapshot`。`StockAnalysisResult` 携带由单股分析阶段解析出的本地 `SecurityId`，快照用例直接使用该规范身份，不按代码再次查询或重建股票关系。快照保留原始数据日期、参数版本、状态、价格区域和建议数量，不覆盖行情、股息、财务或现金流水事实，便于复现和后续回放。

### 8.10 交易日数据同步

`IStockDailyDataSyncAppService` 按关注列表逐只调用 `IStockFactSyncAppService`；事实同步模块按股票复用一次 Security 上下文，依次执行资料、行情、股息和财务快照同步。失败项记录股票、数据类型、稳定 `error_code` 和结构化 `parameters`，不在后台结果中固化某一种语言的展示文案；HTTP 请求先由 `RequestLocalizationMiddleware` 根据 `Accept-Language` 设置 `CurrentUICulture`，异常展示再由 `IApplicationErrorLocalizer` 使用该 culture 生成文本，后台日志和其他非 HTTP 消费者使用默认语言或显式 culture 在展示边界本地化。其他数据类型及其他股票继续执行，避免单个 FTShare 数据缺口阻断整批更新。结果中的 `FullyCompletedStockCount` 只统计四类数据全部成功的股票，`PartiallyFailedStockCount` 统计至少一类失败的股票。`POST /api/v1/stocks/sync` 提供手动触发入口。

股票事实与建议的 Application 数据流如下：

```text
StockDailyDataSyncAppService ──┐
                               ▼
                     StockFactSyncAppService
                               ├── Security 上下文
                               ├── FTShare profile/market/dividend/financial
                               └── 四类事实幂等写入

StockAnalysisAppService ──> RecommendationModule.CalculateStock
                                      │
                                      ▼
                          StockAnalysisResult(+ SecurityId)
                                      │
StockRecommendationAppService ──────┴──> PortfolioAllocationAppService
                                      │                 │
PortfolioRecommendationAppService ──┘                 ▼
                                      RecommendationModule.AllocatePortfolio
                                                        │
                                                        ▼
                                      Stock/Portfolio RecommendationResult
                                      │
                                      ▼
                                      RecommendationSnapshotAppService
                                      (直接使用同一分析结果写入快照)
```

Host 的 `DailyStockDataSyncHostedService` 按 `DailySync:LocalTime` 和 `DailySync:TimeZoneId` 调度，默认使用上海时间每日 18:00，并跳过周末；A 股法定节假日由数据源实际返回结果决定，重复快照通过事实同步用例幂等处理。`StockDataSyncBackgroundService` 监听有界队列，在 Setup 提交后执行一次即时后台同步；它与每日调度和 HTTP 手动同步共享 `StockDataSyncRunner` 的串行闸门。runner 为每次实际执行生成 run ID，并统一记录触发来源、结果和失败摘要。生产环境可以通过 `DailySync:Enabled=false` 关闭定时调度，但 Setup 后的一次性队列同步和手动接口仍然可用；后台单次失败不会终止 hosted service，详细的逐项失败仍由手动同步接口返回，后续运行会重试。

### 8.11 交易记录与持仓成本

`POST /api/v1/portfolio/trades` 通过 `IPortfolioTradeAppService` 记录已发生的买入或卖出。买入会按成交价、数量和手续费重算加权平均成本；实际卖出只不能超过当前持股，允许真实历史交易使当前持仓低于核心仓。核心仓保护只用于系统生成普通减仓建议，不阻止用户补录已经发生的交易。提供 `source_record_id` 时，按组合范围幂等处理重复提交；同一来源标识如果对应不同股票、日期、方向、股数、价格或手续费，返回 409 冲突而不是静默复用。交易响应中的 `TradePrincipalAmount` 只表示成交本金，手续费单独由 `TransactionFeeAmount` 表示。`PortfolioTradeCashLedger` 是交易现金影响的唯一 Domain 策略：买入本金是 outflow，卖出本金是 inflow，手续费始终是独立的 fee/outflow。每次新交易与策略生成的现金流水在同一个 UoW 中提交，自动流水来源标识使用 `portfolio_trade:{trade_id}:principal` 或 `portfolio_trade:{trade_id}:fee`；`BudgetAppService` 只负责用户实际现金事实的录入和摘要查询，不能再次推导交易现金影响。

## 9. FTShare MCP Adapter

```text
IStockDataProvider
        ▲
        │ satisfies
FtShareStockDataProvider
        │
        ▼
IFtShareMcpToolInvoker
        ▲
        │ satisfies
FtShareMcpToolInvoker ──> official MCP Client ──> FTShare MCP
```

`IFtShareMcpToolInvoker` 是 Infrastructure 内部 seam，位于 `Infrastructure/Contracts/`。`FtShareMcpToolInvoker` 使用命名的 `IHttpClientFactory` client 创建 Streamable HTTP transport；连接池、handler 生命周期、HTTP 状态码分类、`Retry-After`、jitter、指数退避和每次 HTTP attempt 的超时由 `Microsoft.Extensions.Http.Resilience` 标准管线负责。由于一个 MCP exchange 可能包含 session 初始化、工具调用和流结束等多次 HTTP 往返，adapter 另外通过 DI 注册的 exchange resilience pipeline 保留一个覆盖完整 exchange（包括 response stream 读取）的有限 operation deadline，并把调用方取消与 operation timeout 分开处理。`FtShareResponseStreamHandler` 将响应流读取阶段的 `IOException`/`HttpRequestException` 转换为专用的 transport failure，使其只按明确的网络失败分类重试；MCP 工具业务错误不会进入任何 resilience 重试。FTShare 工具只读，因此 MCP 的 POST 请求可安全按暂态 HTTP 失败重试；如果未来接入有副作用的工具，必须使用独立 client 并重新声明幂等性契约。`FtShareStockDataProvider` 只负责调用和编排，`FtShareWireModels` 通过 `System.Text.Json` source-generated context 反序列化协议 payload，`FtSharePayloadReader` 统一处理字符串化 JSON 与兼容 envelope，`FtSharePayloadNormalizer` 集中处理别名、日期/数字兼容、身份校验和数据质量默认值，再输出 `StockData`、`StockMarketData`、`StockDividendData` 和 `StockFinancialData`。这样协议噪声不会泄漏到 Application DTO，且四类数据都有 fixture 覆盖。它只接受 A 股和 CNY 股票资料，并拒绝缺少关键字段或股票身份不匹配的结果。

MCP 地址、工具名、股票代码参数名、交易所参数名、每次 HTTP attempt 超时、最大重试次数和重试间隔通过运行时配置注入。当前默认最多重试 2 次，使用 250ms 起步的指数退避并启用 jitter；标准 HTTP resilience 管线同时尊重上游 `Retry-After`，而 exchange pipeline 只处理被 `FtShareResponseStreamHandler` 明确分类的响应流中断。完整 MCP exchange 的 operation deadline 按每次 attempt 超时和重试次数计算，退避时间消耗同一 hard cap，不会无限延长调用。FTShare key 不进入代码、DTO、日志、镜像前端资源或 Git。

FTShare 连接、协议、配置和超时失败先由 Adapter 转换为 Infrastructure 的 `FtShareProviderException`，该异常实现 Application Contracts 中的 `IStockDataProviderFailure` 标记接口；Application 再把它转换为 `stock_data_provider_unavailable` 或具体数据类型的业务错误。这样 Infrastructure 不直接依赖 Application 的业务异常，Application 也不依赖具体 Adapter 类型。

## 10. 公共运行能力

### 10.1 Swagger 与 OpenAPI

Host 使用 .NET 原生的 `Microsoft.AspNetCore.OpenApi`（`AddOpenApi`/`MapOpenApi`）生成 OpenAPI 文档，并通过 `Asp.Versioning.OpenApi` 与 API Versioning 集成，按版本生成独立文档；`Swashbuckle.AspNetCore.SwaggerUI` 只承担交互式 UI 渲染，不再负责文档生成。当前业务接口统一使用 URL path 版本 `/api/v1/...`，未带版本号的业务 URL 不会隐式映射到默认版本；`/swagger` 用于浏览和调用 Controller 接口，`/openapi/v1.json` 用于获取 v1 机器可读的 OpenAPI 文档。`app.MapOpenApi().WithDocumentPerVersion()` 按 `IApiVersionDescriptionProvider` 动态生成版本文档，未来增加 v2 时新增对应的 `[ApiVersion(2.0)]` Controller/Action，不需要修改文档生成逻辑，也不修改既有 v1 合约。OpenAPI 与 Swagger UI 的服务注册集中在 `HostServiceCollectionExtensions`，middleware 集中在 `WebApplicationExtensions`，不在 `Program.cs` 或 Controller 中重复配置。

所有公开 Controller、Action 和 API DTO 都必须提供 .NET XML 文档注释（至少包含类型/操作 `summary`，公开参数提供 `param`）。Application 与 Domain 项目开启 `GenerateDocumentationFile`，Host 通过 `XmlDocumentationOperationTransformer` 和 `XmlDocumentationSchemaTransformer` 从相邻程序集的 XML 文件读取摘要，并把操作、schema 及属性说明写入同一份原生 OpenAPI 文档。构建期生成的 `src/Portwise.Web/openapi/portwise_v1.json` 必须包含非空操作摘要和 DTO schema 描述；Swagger UI 直接消费该文档，因此不需要 Swashbuckle 的 XML 配置或第三方文档生成器。

HTTP 合约的持久化副本由 Host 的 build-time document generation 生成，而不是由运行中的 `/openapi/{version}.json` 端点手工复制。`Portwise.csproj` 将文档写入前端 `openapi/` 目录；`Program.cs` 在 `GetDocument.Insider` 设计时入口使用 `CreateEmptyBuilder`，只注册 Controller、API Versioning 和 OpenAPI 元数据，避免生成合约时启动 SpaProxy 或外部基础设施。新增 v2 时生成新的 `portwise_v2.json` 与对应 TypeScript 文件，v1 仍保持独立。

### 10.2 Serilog

Host 使用 Serilog 接管 ASP.NET Core 和应用的 `ILogger<T>` 日志，配置来源为 `appsettings.json`、`appsettings.Development.json`/`appsettings.Production.json`。`WriteTo` 同时配置 `Console` 和 `File` 两个 sink：`Console` 输出到标准输出（本地终端或容器标准输出，供实时观察和容器日志采集）；`File` 按天滚动写入进程工作目录下的 `logs/portwise-{Date}.log`（`retainedFileCountLimit: 31`，只保留最近 31 天），供本地排查历史问题和无容器日志采集设施时兜底查阅。`logs/` 目录已在 `.gitignore` 中排除，不提交任何运行日志文件；容器部署时 `logs/` 通过 Docker `VOLUME` 声明持久化到宿主机，与 `data/` 卷同一约定。`UseSerilogRequestLogging` 记录 HTTP 请求摘要，业务日志使用结构化属性；请求体、Authorization、FTShare 凭据和原始外部响应不得写入日志（对 Console 和 File 两个 sink 同样生效）。请求、后台同步和 FTShare 调用通过统一诊断上下文写入受控的关联字段。

`Program.cs` 采用 Serilog 官方推荐的两阶段初始化模式，避免"Host 尚未构建完成前发生的致命错误没有任何日志"的问题：

- 第一阶段：在 `WebApplication.CreateBuilder` 之前，用 `new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger()` 赋值给静态的 `Log.Logger`。这个 Bootstrap Logger 只写控制台，不依赖 `appsettings.json` 或 DI 容器，能捕获配置加载、DI 注册等 Host 构建阶段本身的异常。
- 第二阶段：`HostServiceCollectionExtensions.AddPortwiseHost` 中的 `services.AddSerilog((serviceProvider, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(configuration).ReadFrom.Services(serviceProvider)...)` 在 Host 构建完成、DI 容器可用后，读取完整的 `appsettings.*.json` 配置和已注册服务，替换掉 `Log.Logger` 为完整配置的 Logger（默认 `preserveStaticLogger: false`）。
- `Program.cs` 的 `WebApplication.CreateBuilder` 到 `app.RunPortwiseAsync()` 整体包裹在 `try/catch/finally` 中：`catch` 分支排除 EF Core 设计时工具触发的 `HostAbortedException`（正常控制流，不应记为致命错误），其余异常统一 `Log.Fatal` 记录后返回非零退出码；`finally` 分支调用 `await Log.CloseAndFlushAsync()`，保证进程退出前把所有已缓冲的日志事件写出，不因为控制台缓冲或异步 sink 未刷新而丢失最后一批日志。
- 新增或修改 Host 启动流程时，不得绕过这个 try/catch/finally 结构直接调用 `app.RunAsync()`，否则会重新引入“启动失败但看不到任何日志”的问题。

### 10.3 Mapperly

Application 使用 Riok.Mapperly 生成编译期映射代码，映射声明分别位于 `Application/Stocks/StocksMapper.cs`、`Application/Portfolio/PortfolioMapper.cs` 和 `Application/Recommendations/RecommendationsMapper.cs`。Mapper 只负责 Domain Model、外部规范化 DTO 和响应 DTO 之间的数据形状转换；业务规则、数据查询、事务和派生建议仍由 Application/Domain 负责。

### 10.4 API Versioning

- 使用 `Asp.Versioning.Mvc`、`Asp.Versioning.Mvc.ApiExplorer` 和 `Asp.Versioning.OpenApi` 为 Controller API 提供版本元数据、OpenAPI 分组与按版本生成的原生 OpenAPI 文档。
- 当前业务 API 为 v1，Controller 使用 `[ApiVersion(1.0)]`，路由使用 `api/v{version:apiVersion}/...`；调用方必须显式携带 `/api/v1/...`。
- 当前前端已接入 v1 Controller API；前端构建产物写入 Host 的 `wwwroot`，由同一个 ASP.NET Core Host 通过 `UseDefaultFiles`、`UseStaticFiles` 和 `MapFallbackToFile("index.html")` 提供。此次接入属于 v1 合约冻结前的实现，不是对已发布 v1 合约的静默变更；之后面向调用方的破坏性修改必须进入 v2。
- 本地开发使用 .NET 官方标准的 `Microsoft.AspNetCore.SpaProxy` 集成前端 dev server（详见 10.6 节），取代早期手工维护 Vite proxy + 双终端的方式；发布产物不受影响，仍然是纯静态文件。
- Host 使用 `UrlSegmentApiVersionReader`，不假设缺失版本时自动使用默认版本，并通过 `ReportApiVersions` 返回支持/弃用版本信息。
- 健康检查、Swagger UI 和 `/openapi/{version}.json` 使用自身的基础路径，不强行套用业务 API 版本。
- 新增破坏性合约时增加 v2 版本元数据和对应文档分组；v1 只在明确的弃用策略下变更，不能静默改变响应语义。

### 10.5 项目构建、包管理与代码风格

- 根目录 `Directory.Build.props` 统一收敛所有项目共用的编译属性（`TargetFramework`、`ImplicitUsings`、`Nullable`、`TreatWarningsAsErrors` 等）；各 `.csproj` 不得重复声明这些属性，新增项目直接继承根配置，只保留项目专属设置（如 `OutputType`、专属 `PackageReference`）。
- 根目录 `Directory.Packages.props` 启用 Central Package Management（`ManagePackageVersionsCentrally=true`），是全仓库第三方包版本的唯一来源；各 `.csproj` 的 `PackageReference` 不得携带 `Version` 属性。新增或升级依赖时只修改 `Directory.Packages.props` 中的 `PackageVersion`，禁止在单个项目里覆盖版本，避免同一个包在不同项目中出现版本漂移。
- 根目录 `.editorconfig` 是 C# 代码风格的唯一声明来源，覆盖缩进、file-scoped namespace、Allman 括号风格、`var` 使用偏好、可空引用类型诊断级别以及接口 `I` 前缀、PascalCase/camelCase 等命名规则；新增项目和文件默认继承该配置，不得在项目内新增局部 `.editorconfig` 覆盖风格规则。当前未开启 `EnforceCodeStyleInBuild`，规则只在 IDE 内联提示和 `dotnet format` 中生效，不会让 `dotnet build` 失败；如果后续需要把风格规则升级为构建期强制检查，必须先确认存量代码全部符合规则再开启该选项，并同步更新本节说明。

### 10.6 本地启动与前端构建集成

- `src/Portwise/Properties/launchSettings.json` 是 Host 本地启动的唯一 profile 来源，提供 `http` profile（`applicationUrl=http://localhost:5276`，`ASPNETCORE_ENVIRONMENT=Development`），供 `dotnet run --launch-profile http` 和 IDE 运行配置下拉框使用；该 profile 同时设置 `DOTNET_USE_POLLING_FILE_WATCHER=1`，兼容 macOS 上当前 .NET 10.0.0 的 `FileSystemWatcher` 启动递归问题，避免 Host 卡在 `WebApplication.CreateBuilder` 阶段；这是 .NET Web 项目模板的标准文件，缺失会导致手动 `dotnet run` 在没有显式设置环境变量时以 `Production` 环境启动，从而绕过开发期配置和异常页面。
- `appsettings.Development.json` 是 `Development` 环境的配置覆盖（当前只覆盖 Serilog 最低日志级别为 `Debug`），与 `appsettings.json`、`appsettings.Production.json` 一起构成完整的三段式环境配置；新增只在本地开发环境需要的配置项时优先放入这个文件，不要污染 `appsettings.json` 的默认值。
- 本地开发使用 .NET 官方标准的 `Microsoft.AspNetCore.SpaProxy` 集成前端：Host 项目仅在 `Debug` 配置下引用 `Microsoft.AspNetCore.SpaProxy` 包，并声明 `SpaRoot`（`../Portwise.Web/`）、`SpaProxyServerUrl`（`http://localhost:4173`）、`SpaProxyLaunchCommand`（`pnpm run dev`）；同时通过 `portwise.esproj`（`Microsoft.VisualStudio.JavaScript.Sdk`）以 `ReferenceOutputAssembly=false` 挂入 Host，使 Rider 识别标准 `.NET Launch Settings Profile` 与 SPA 启动项目。`Properties/launchSettings.json` 的 `http` profile 设置了 `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=Microsoft.AspNetCore.SpaProxy`，Host 地址为 `http://localhost:5276`。执行 `dotnet run --launch-profile http` 或 IDE 的 `http` 配置时，SpaProxy 通过 `IHostingStartup` 自动注入的 `IStartupFilter` 检测 Vite dev server 是否就绪，未就绪则自动执行 `SpaProxyLaunchCommand` 拉起，并把非 API 请求反向代理到 dev server；开发者只需要一条命令即可完成前后端联调。还原、构建、运行（CI、Dockerfile、`BuildFrontend` Target、SpaProxy dev server）全部统一使用 pnpm，不引入 npm。`WebApplicationExtensions` 的中间件管道不需要为此做任何区分（Development/Production 都是同一套 `UseDefaultFiles`/`UseStaticFiles`/`MapFallbackToFile`），SpaProxy 的转发逻辑完全由 `IStartupFilter` 在管道最前面完成。Release/Publish 不引用 SpaProxy 包，发布产物仍是纯静态文件。
- 前端（`src/Portwise.Web`）与 Host 项目在构建上保持独立：CI（`build-and-test.yml`）和 Dockerfile 分别用 `pnpm install`/`pnpm build` 显式构建前端后再构建/发布 Host，这是为了避免在只安装 .NET SDK、没有 Node.js/pnpm 的构建环境（例如 Docker 后端构建阶段镜像）上因为隐式触发前端构建而失败。`Portwise.csproj` 额外提供一个默认关闭的 `BuildFrontend` MSBuild Target（`BeforeTargets="Build;Publish"`），只有显式传入 `-p:BuildFrontend=true` 执行 `dotnet build`/`dotnet publish` 时才会自动执行 `pnpm install`/`pnpm build` 并把产物写入 Host 的 `wwwroot`，用于本地一次性生成前后端产物；CI 和 Docker 流程不依赖也不触发这个 Target。</replace>

- Vite dev server 在 `vite.config.ts` 中使用 `server.host=true` 监听所有网络接口，`SpaProxyServerUrl` 使用 `http://localhost:4173` 作为本机探测地址；这样既兼容 SpaProxy，也允许通过其他本机 IP 或局域网地址访问开发服务器。Vite 的 API、健康检查和就绪检查代理目标统一使用 `http://localhost:5276`。

## 11. Exception 设计

Application 自定义异常统一位于 `Application/Exceptions/`，但不再为每个业务错误码创建一个异常类。异常模型收窄为：

- `ApplicationExceptionBase`：统一承载稳定 `ErrorCode`、结构化参数和仅供内部诊断的 `InnerException`。
- `ApplicationErrorException`：承载状态、冲突、未配置、外部资料不可用等非验证错误。
- `ApplicationValidationException`：承载由 FluentValidation 或 Domain 不变量转换而来的验证诊断。
- `ApplicationErrorCodes`：稳定错误码的唯一代码清单；`ApplicationErrors` 按参数形状提供集中式工厂。

错误码仍然按业务语义细分，例如 `stock_market_data_unavailable` 和 `portfolio_trade_conflict`，但错误码与异常类型解耦。这样既保留 `locales/{culture}/{domain}.json`、HTTP 状态和参数占位符的精确语义，也避免大量只重复错误码和字典参数的类。目录加载时校验 `ApplicationErrorCodes.All` 与语言 JSON 的完整集合，而不是反射扫描异常类属性。

Host 的 `ApplicationExceptionHandler` 只负责识别 Application 异常、调用 `IApplicationErrorLocalizer` 生成语言和参数已解析的结果、记录安全的结构化日志，并直接调用 ASP.NET Core 注册的 `IProblemDetailsService` 写入响应。这样保留框架的 `IExceptionHandler` 与 `IProblemDetailsService` seam，删除只有一个 implementation 的 `IHttpErrorRenderer`/`ProblemDetailsErrorRenderer` 中间层；响应返回目录解析出的 `status`、`title`、`detail`、稳定 `error_code`、`locale` 和 `trace_id`。验证错误映射为 400、状态冲突映射为 409、未配置股票映射为 404、外部资料不可用映射为 503。异常的 `InnerException`、FTShare 原始响应和凭据不会进入公共详情；调用方主动取消的 `OperationCanceledException` 不转换，继续传播。

新增错误时，只需在 `ApplicationErrorCodes` 添加稳定码、在每个受支持语言的对应领域 JSON 中定义它、通过 `ApplicationErrors` 选择结构化参数工厂并补充 Application 单元测试。缺少错误码定义、重复定义、跨语言状态码/占位符不一致、非法状态码或空文本应在目录加载时直接失败，而不是运行到请求时才产生隐性回退。

### 11.1 基于 Activity 的隐私感知诊断上下文

`IDiagnosticContext` 位于 `Application/Contracts/`，Host 使用基于 `System.Diagnostics.Activity`（W3C Trace Context 官方标准）的 `ActivityDiagnosticContext` 实现，并在 `HostServiceCollectionExtensions` 中注册。`PortwiseActivitySource`（`Diagnostics/PortwiseActivitySource.cs`）是 Host 唯一的 `ActivitySource`，静态构造函数注册一个基础 `ActivityListener`（`ActivitySamplingResult.AllDataAndRecorded`），使 Activity 在没有接入完整 OpenTelemetry SDK/导出器时也能被创建和记录；这样系统既能立即获得标准化的分布式追踪基础设施，也不需要为当前的单进程 Host 引入额外的 Exporter 依赖，未来接入 OpenTelemetry Collector/导出器时只需要替换或追加 `ActivityListener`/`TracerProvider`，不需要改动业务代码里 `IDiagnosticContext` 的调用方式。

`ActivityDiagnosticContext.BeginScope` 为每个 `DiagnosticScope` 启动一个 `Activity`（`ActivitySource.StartActivity`），仍然只允许写入固定的安全字段，并同时作为 Activity Tag 和 Serilog `LogContext` 属性写入：

- `diagnostic_operation`、`correlation_id`、`run_id`、`error_code`、`severity`；其中操作只允许 `http_request`、`http_error`、`daily_stock_data_sync`、`stock_data_sync` 和 `ftshare_mcp`。
- A 股 `security_code`、`exchange_code` 和有限集合的 `data_kind`（`profile`、`market`、`dividend`、`financial`）。

Serilog 通过 `Serilog.Enrichers.Span` 的 `Enrich.WithSpan()` 自动把当前 `Activity` 的 `TraceId`/`SpanId`/`ParentId` 注入结构化日志，不需要手工维护 correlation id 的生成和传播；ASP.NET Core 自身也会为每个 HTTP 请求创建 Activity，`HttpContext.TraceIdentifier` 因此天然是 W3C 格式的 trace id。`WebApplicationExtensions` 仍然为每个 HTTP 请求开启一个 `http_request` scope 并返回 `X-Correlation-Id` 响应头；Application 异常响应同时返回 `trace_id`。`StockDataSyncRunner` 为每次实际同步创建独立 run scope；`FtShareStockDataProvider` 为每次资料、行情、股息和财务 MCP 调用追加股票引用和数据类型；这些嵌套 scope 产生的 Activity 会按照 `Activity.Current` 自动形成父子关系，构成一次请求内的完整调用链。诊断上下文仍然不接受任意字典，超过长度、包含不允许字符或不在有限集合中的值会被丢弃；内部日志最多记录异常类型和 cause type，不记录异常消息，避免把请求正文、持仓数量、认证信息、FTShare key、原始响应和内部异常文本带入日志、Activity Tag 或 ProblemDetails。

## 12. 测试策略

测试项目位于 `tests/`，使用 xUnit + Moq；需要时间控制时使用微软提供的 `FakeTimeProvider`：

- Domain 测试领域不变量。
- Application 测试通过 `IRepository<TEntity>`、`IUow` 和数据提供 Adapter 的 Interface mock 验证用例行为。
- Application 测试不创建真实 DbContext，也不依赖真实 PostgreSQL 或 FTShare 网络连接。
- Infrastructure 测试只验证 FTShare Options 等非数据库行为；数据库 migration 通过生成 SQL、pending-model-changes 检查和 Compose smoke 验证。
- Host 测试验证同步执行器的并发边界、Controller 入口、Options 校验和调度时间。
- Infrastructure 的 EF Fluent 配置和 Adapter 通过编译、依赖检查及后续专门测试验证；不把 EF Core 细节泄漏到 Application 单元测试。

所有测试必须保持对 Interface 的验证，而不是依赖具体实现内部结构。

## 13. 后续扩展

新增行情、股息、财务和建议计算功能时，优先遵守以下边界：

- 外部 FTShare 数据先进入 Infrastructure Adapter，再转换为 Application DTO。
- 事实数据和建议快照按 `docs/dividend-strategy-quant-model.md` 的 canonical 字段建模。
- 新增持久化能力先增加对应 Domain Model `class` 和 Fluent Configuration，再通过 `IUow.Get<TEntity>()` 获取通用 Repository；不要从 Host 或 AppService 直接使用 DbContext。
- 新增业务状态代码时使用显式枚举或 `*_code` 约定；字符串代码放入 `Codes/`，真正的枚举放入所属项目的 `Enums/` 文件夹。

## 14. 2026-09-07 后端架构审查处理记录

本节记录 HTML 架构审查报告的处理结果，避免把“建议探索项”误解为已经批准的重写任务。每个已修复项目保持独立提交；保留项目只有在出现明确的正确性、性能或部署指标后才重新评估。

| 报告项 | 处理结果 | 提交/触发条件 |
| --- | --- | --- |
| 01 EF Core migrations | 已修复：启动统一执行 `MigrateAsync`，设计时工厂和初始 migration 已纳入 Infrastructure | `0e74ad0` |
| 02 同步执行 seam | 已修复：HTTP、Setup 和定时任务统一经过 `IStockDataSyncRunner` | `62d81b3` |
| 03 Options 与时间 | 已修复：Options `ValidateOnStart`，调度/重试/超时使用注入的 `TimeProvider` | `7776ddb` |
| 04 启动退出码 | 已修复：启动异常刷新日志后返回非零退出码 | `3ea1ab7` |
| 05 行为型持久化 seam | 保留 `IUow` + 通用 Repository；当前没有足够的重复查询或性能证据 | 出现具体查询/性能问题后再引入窄接口 |
| 06 HTTP 本地化 | 已修复：ASP.NET Core `RequestLocalizationMiddleware` 负责 HTTP culture；JSON catalog 保留 | `c202011` |
| 07 Activity 采样 | 保留本地 listener，保证无 Collector 的单进程部署仍可诊断 | 接入 OpenTelemetry 或有采样成本指标后再调整 |
| 08 组合建议读模型 | 暂不引入快照；先以真实组合规模、查询次数和延迟建立基线 | 指标确认 N+1/延迟热点后再批量 projection |
| 09 单组合不变量 | 已修复：`portfolio_scope` 唯一索引和并发冲突映射 | `44439cd` |
| 10 股票事实 AppService | 保留独立事实类型契约；当前重复主要是入口校验与转发 | 出现共享事务、幂等或真实 adapter 复用需求后再提取内部 module |

截至本记录，后端完整测试共 141 个通过；EF Core `has-pending-model-changes` 检查通过。

## 15. 2026-09-08 架构深化处理记录

本节记录本轮 HTML 审查中已逐项落地的深模块处理。每个项目都保持独立提交，并在提交前完成对应层的构建、Lint 或测试验证。

| 审查候选项 | 处理结果 | 提交 |
| --- | --- | --- |
| 01 前端推荐展示 module | 已修复：`recommendation-display.ts` 统一 recommendation/price-zone 的展示语义，feature 只消费归一化结果 | `58b1465` |
| 02 后端 Recommendation module | 已修复：Domain `RecommendationModule` 集中单股分析与组合分配规则，Application AppService 变为薄 Adapter | `196a4ff` |
| 03 HTTP contract 单一事实源 | 已修复：Host build-time 生成版本化 OpenAPI，前端从 `openapi-typescript` 生成 transport contract，`api-contract.ts` 是唯一 wire-to-UI Adapter | `e97c070` |
| 04 导航与 Setup gate module | 已修复：`navigation.ts` 集中路由、history、查询参数和 Setup gate seam，修复 query 与旧 session 选择冲突 | `e4ff8f3` |
| 05 Application 领域 module locality | 已修复：Setup、Stocks、Portfolio、Recommendations 各自共置 contract/DTO/validator/实现/测试，根目录只保留跨 module 共享项 | `aa2dae8` |

## 16. 2026-09-09 架构复审候选项处理记录

本节记录修改后复审中的剩余候选。每项完成后保持独立提交；实现与本记录一起验证。提交哈希以 Git 历史为准，避免在提交内容中制造自引用。

| 复审候选项 | 处理结果 | 验证 |
| --- | --- | --- |
| 01 HTTP contract publication module | 已完成：集中运行时与 build-time OpenAPI 注册，前端请求消费生成路径约束，增加 wire numeric 运行时归一化与 contract drift 检查；CI/Docker 先生成并校验 contract，再构建前端 | `dotnet test`、`pnpm api:check`、`pnpm build`；提交 `refactor: close HTTP contract publication loop` |
| 02 Recommendation 事实装配 module | 已完成：`StockAnalysisAppService` 增加关注列表批量事实读取，单股与组合入口共享一次计算时间和分析结果；组合分配拒绝混合计算时间，快照复用组合结果时间 | `dotnet test`（82 Application tests）；提交 `refactor: batch recommendation fact assembly` |
| 03 Frontend application shell module | 已完成：路由选择、Setup gate、浏览器 session 读取和 feature 装载集中到 `application-shell.tsx`；feature 页面按路由懒加载；navigation 纯策略增加 Node 原生测试与 `pnpm test` 脚本 | `pnpm test`（4 tests）、`pnpm build`、`pnpm lint`；提交 `refactor: deepen frontend application shell` |
| 04 Application module ownership executable | 已完成：module-specific Contracts/DTOs/Validators 使用 module namespace，Mapperly 映射拆分为 `StocksMapper`、`PortfolioMapper`、`RecommendationsMapper`；新增架构测试防止类型泄漏回根技术桶 | `dotnet test`（84 Application tests）、`dotnet build Portwise.slnx`；提交 `refactor: enforce application module ownership` |
| 05 CONTEXT/ADR authority | 已完成：修正根上下文产品名与文档权威声明，新增 `docs/adr/` 索引及单组合、单 Host、HTTP 合约、Application module locality 四项 Accepted ADR，并清理实现地图中的过时路径 | `git diff --check`、全文旧路径扫描；提交 `docs: establish architecture decision authority` |
| 06 Backend localization and native XML documentation | 已完成：移除后端源码中的中文诊断硬编码，为全部 Controller/DTO 增加 XML 注释；由 .NET 原生 OpenAPI transformer 读取各项目 XML 文档并写入操作、schema 和属性描述 | `dotnet build`、`dotnet test`、OpenAPI 摘要/schema 检查、中文源码扫描；本次提交 |

## 17. 2026-09-10 FTShare HTTP transport 复审处理记录

| 复审候选项 | 处理结果 | 验证 |
| --- | --- | --- |
| 03 托管 FTShare HTTP transport | 已完成：使用命名 `IHttpClientFactory` client 创建 MCP transport，连接池和 handler 生命周期由工厂管理；`Microsoft.Extensions.Http.Resilience` 负责 HTTP 状态码重试、`Retry-After`、jitter、指数退避和每次 attempt 超时；DI 注册的 exchange pipeline 只重试明确分类的响应流中断，adapter 保留覆盖完整 MCP exchange（含流读取）的 hard deadline，删除手写重试循环；增加命名 client 的 POST 暂态响应重试、非暂态响应不重试、响应流异常分类和 exchange pipeline 重试测试 | Infrastructure 定向测试、完整 `dotnet test`、`git diff --check`；本次提交 |
| 04 深化 FTShare payload 解析 | 已完成：将 800+ 行 `JsonElement` tree walk 拆为 source-generated wire models、统一 envelope reader 和单一 normalizer；保留原有兼容别名、字符串化 JSON、数字/日期/布尔值兼容与身份校验，Application 仍只接收规范化 DTO；增加 profile、market、dividend、financial 四类 fixture 测试 | Infrastructure 定向测试、编译、`git diff --check`；本次提交 |
