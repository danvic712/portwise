# Portwise

Portwise 是一个面向个人 A 股长期投资者的私人工具。它把股票资料、策略所需的市场事实、收盘行情、持仓和预算放在同一个组合上下文中，用规则透明的方式给出“现在是否适合观察、分批买入或减仓”的参考。当前版本内置股息策略，后续可以扩展更多策略。

项目地址：[github.com/danvic712/portwise](https://github.com/danvic712/portwise)

> 本项目只提供可解释的研究和模拟记录，不预测股价、不自动下单、不构成投资建议。

## 项目定位

这个项目优先解决三个问题：

1. 让不熟悉量化模型的用户知道“当前发生了什么”和“下一步可以做什么”；
2. 用当前的股息策略，以 TTM 实际股息、股息可靠性和价格区域生成每只股票的分批交易参考；
3. 保存个人的持仓、交易和现金流水，让建议能结合自己的实际情况，而不是只看一个股息率。

当前版本只支持中国 A 股、CNY 和单用户单组合场景，不包含账户体系、多人协作、自动交易或云端托管。

## 已实现能力

- 首次设置时一次添加多只 A 股，可录入已有持仓、核心仓、目标股数和平均成本；
- 建账先保存股票和组合，FTShare 资料同步进入后台任务，不阻塞用户进入系统；
- 通过 FTShare MCP Adapter 获取股票基础资料、行情、股息事件和财务快照；
- 按交易日自动同步全部关注股票，也可以在股票资料页手动触发同步；
- 使用 TTM 实际每股股息计算股息率，并结合可靠性检查、价格区域、预算和核心仓生成参考；
- 支持多只股票独立配置模型参数，同时在组合层汇总预算、持仓和建议；
- 提供今日决策、股票资料、资金预算、交易记录和设置页面；
- 提供中英文切换、日间/夜间/跟随系统主题、加载骨架屏、404 页面和全局错误页面；
- 提供 Swagger、Serilog、API Versioning、原生健康检查和 PostgreSQL 17+ 持久化。

## 运行架构

前端和后端构建为一个 ASP.NET Core 镜像，由同一个 Host 提供静态页面和 API：

```text
Browser
  │
  ▼
ASP.NET Core Host
  ├── React + shadcn/ui 风格前端（构建到 wwwroot）
  ├── Controllers / API v1
  ├── Application AppService + FluentValidation
  ├── Domain 规则与股息交易模型
  └── Infrastructure
       ├── EF Core + PostgreSQL 17（public schema）
       └── FTShare MCP Adapter
```

数据访问只经过 `IUow`，数据库实体位于 Domain，EF Core Fluent API 配置位于 Infrastructure。控制器只负责 HTTP 参数绑定、调用应用服务和返回响应，不包含业务逻辑。

## 快速运行：Docker

### 构建镜像

在项目根目录执行：

```bash
docker build -t danvic712/portwise .
```

镜像采用多阶段构建：先构建 React 前端，再发布 ASP.NET Core 后端。根目录的 `locales/` 会同时纳入前端构建和后端嵌入式资源；其中前端 UI 文案和后端应用错误定义共用同一套中英文语言文件。

### Docker Compose 本地运行

Compose 会启动 PostgreSQL 17 和 Portwise Host，Host 只在数据库健康检查通过后启动，并在首次启动时自动应用 fresh migration：

```bash
cp .env.example .env
docker compose up --build -d
```

启动后访问 `http://127.0.0.1:8080/`；`/healthz` 是存活检查，`/readyz` 是包含数据库的就绪检查。PostgreSQL 数据保存在 named volume `portwise-postgres` 中，执行 `docker compose down` 不会删除它；需要重新建立空 schema 时再执行 `docker compose down -v`。

如果只想由 Compose 启动数据库、再从 Rider/Visual Studio 或命令行启动 Host，可以执行：

```bash
docker compose up -d postgres
dotnet run --project src/Portwise/Portwise.csproj --launch-profile http
```

IDE 运行 Host 时请使用本地 `ConnectionStrings__Default`（`Host=localhost`）；Compose 内部 Host 使用服务名 `postgres`，两者不会混用。

### 启动实例（外部 PostgreSQL）

应用镜像不内置数据库；生产或独立运行时请注入 PostgreSQL 17+ 的连接字符串：

```bash
docker run --name portwise \
  --rm \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e 'ConnectionStrings__Default=Host=<postgres-host>;Port=5432;Database=portwise;Username=<postgres-user>;Password=<postgres-password>' \
  -e FtShare__McpEndpoint="https://<ftshare-mcp-endpoint>/mcp" \
  danvic712/portwise
```

启动后访问：

- 应用：http://127.0.0.1:8080/
- Swagger：http://127.0.0.1:8080/swagger
- 存活检查：http://127.0.0.1:8080/healthz
- 就绪检查：http://127.0.0.1:8080/readyz

项目适合部署在私有网络中。数据库数据由外部 PostgreSQL 服务负责持久化和备份；应用镜像只保留可选的 `/app/logs` 文件日志目录，不保存关系数据。

## GitHub Actions

仓库包含两个独立的 GitHub Actions workflow：

- [`build-and-test.yml`](.github/workflows/build-and-test.yml)：在分支推送和 Pull Request 时运行，使用 pnpm/NuGet 缓存，依次构建前端、还原并构建 .NET 解决方案，然后执行全部后端测试；
- [`docker-build-and-push.yml`](.github/workflows/docker-build-and-push.yml)：只在推送 Git tag 时运行，构建同时支持 `linux/amd64` 和 `linux/arm64` 的镜像，并同时推送到 Docker Hub 和 GitHub Container Registry。

Docker 镜像 workflow 需要在 GitHub 仓库的 Settings → Secrets and variables → Actions 中配置：

| Secret | 用途 |
| --- | --- |
| `DOCKERHUB_USERNAME` | Docker Hub 用户名 |
| `DOCKERHUB_TOKEN` | Docker Hub Access Token |

GitHub Container Registry 使用 workflow 自动提供的 `GITHUB_TOKEN`，不需要额外创建 Secret；仓库权限必须允许 Actions 写入 Packages。

发布镜像时推送一个 tag 即可：

```bash
git tag v0.1.0
git push origin v0.1.0
```

该 tag 会发布为以下镜像，并同时更新各自的 `latest`：

```text
danvic712/portwise:v0.1.0
ghcr.io/danvic712/portwise:v0.1.0
```

### Azure App Service 配置

Azure App Service 只运行 Portwise Host，关系数据使用 Azure Database for PostgreSQL Flexible Server 或其他可达的 PostgreSQL 17+ 服务；不要把数据库文件写入容器 `/home`。通过应用设置注入连接字符串：

| App Service 应用设置 | 值 |
| --- | --- |
| `WEBSITES_PORT` | `8080` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__Default` | `Host=<postgres-host>;Port=5432;Database=portwise;Username=<postgres-user>;Password=<postgres-password>` |

也可以使用 Azure CLI 配置：

```bash
az webapp config appsettings set \
  --resource-group <resource-group> \
  --name <app-name> \
  --settings \
    WEBSITES_PORT=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    'ConnectionStrings__Default=Host=<postgres-host>;Port=5432;Database=portwise;Username=<postgres-user>;Password=<postgres-password>'
```

`WEBSITES_PORT=8080` 对应镜像的 `EXPOSE 8080`。PostgreSQL 的网络访问、TLS、备份、连接池和防火墙策略由托管数据库和 Azure 网络配置负责；应用不依赖 App Service 文件持久化。[Azure 自定义容器文档](https://learn.microsoft.com/zh-cn/azure/app-service/configure-custom-container)

## 本地开发

### 环境要求

- .NET SDK 10；
- Node.js `24.16.x`；
- pnpm `12.3.4`；
- Docker（仅在使用容器运行时需要）。

### 运行后端

项目自带 `Properties/launchSettings.json`，可以直接在 Rider/Visual Studio 的运行配置下拉框中选择 `http` profile 启动（默认监听 `http://localhost:5276`，`ASPNETCORE_ENVIRONMENT=Development`）。该 profile 同时启用 .NET 的 polling file watcher，兼容 macOS 上当前 .NET 10.0.0 的 `FileSystemWatcher` 启动递归问题；使用这个 profile 时不会卡在 Host 创建阶段。命令行等价写法：

```bash
dotnet run --project src/Portwise/Portwise.csproj --launch-profile http
```

后端启动时会自动应用待执行的 EF Core migration，并在空 PostgreSQL 17+ 数据库的 `public` schema 中创建完整结构；migration history 使用 `public.ef_migrations`。本地 Compose 使用 `postgres` 服务名，IDE/命令行使用 `localhost`，生产部署通过 `ConnectionStrings__Default` 注入外部 PostgreSQL 连接。配置文件只保存非敏感默认值。

修改 EF Core 模型后，先还原仓库固定版本的工具，再从 Infrastructure 项目生成 migration：

```bash
dotnet tool restore
dotnet ef migrations add <MigrationName> \
  --project src/Portwise.Infrastructure/Portwise.Infrastructure.csproj \
  --startup-project src/Portwise.Infrastructure/Portwise.Infrastructure.csproj \
  --output-dir Migrations
```

需要一次性生成前后端产物（不经过 Docker）时，可以在 `dotnet build`/`dotnet publish` 时显式开启前端构建：

```bash
dotnet build src/Portwise/Portwise.csproj -p:BuildFrontend=true
# 或
dotnet publish src/Portwise/Portwise.csproj -c Release -p:BuildFrontend=true
```

该开关默认关闭，避免在没有 Node.js/pnpm 的机器（例如 Docker 后端构建阶段、只安装了 .NET SDK 的 CI 步骤）上因缺少前端工具链而构建失败；CI 和 Docker 镜像目前各自独立执行前端构建（见下文）。

### 前端开发联调（SpaProxy）

本地开发使用 .NET 官方标准的 [`Microsoft.AspNetCore.SpaProxy`](https://learn.microsoft.com/aspnet/core/client-side/spa/intro) 集成前端，取代早期“手动开两个终端 + Vite 自带 proxy”的方式：`Properties/launchSettings.json` 的 `http` profile 已经设置 `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=Microsoft.AspNetCore.SpaProxy`，Host 项目在 `Debug` 配置下引用了 `Microsoft.AspNetCore.SpaProxy` 包并声明了 `SpaRoot`/`SpaProxyServerUrl`/`SpaProxyLaunchCommand`；前端同时通过 JavaScript SDK 工程挂入 Host，保证 Rider 能识别标准的 `.NET Launch Settings Profile` 与 SPA 启动项目。

只需要一条命令即可同时启动后端和前端 dev server：

```bash
dotnet run --project src/Portwise/Portwise.csproj --launch-profile http
```

首次启动时后端会检测到 `http://localhost:4173` 未就绪，自动在 `src/Portwise.Web` 下执行 `pnpm run dev` 拉起 Vite dev server（需要提前执行一次 `corepack enable && pnpm install --frozen-lockfile` 安装依赖），随后把非 API 请求反向代理到 Vite；浏览器只需访问后端地址 `http://localhost:5276` 即可看到前端页面并联调 `/api`。

如果只想单独运行前端 dev server（不联调后端），仍可以手动执行：

```bash
cd src/Portwise.Web
corepack enable
pnpm install --frozen-lockfile
pnpm dev
```

此时开发服务器使用 `server.host=true` 监听本机所有网络接口，通常可通过 `http://localhost:4173` 或其他本机 IP 访问，并将 `/api`、`/healthz` 和 `/readyz` 代理到 `http://localhost:5276`（`vite.config.ts` 中的 `server.proxy` 配置）。这与 .NET SpaProxy 的探测地址兼容，同时允许局域网设备访问开发服务器。前端生产构建会输出到 `src/Portwise/wwwroot`，Docker 构建会自动执行这一步；`Debug` 配置下的 SpaProxy 只用于开发期反代，不影响生产发布产物。

如果 Rider 启动时提示 `Couldn't start the SPA development server with command 'pnpm run dev'`，请在 Rider 的运行配置中将 Node.js 环境指定为 `24.16.x`，并确保该配置的 PATH 包含对应 Node 目录；不要依赖从 Dock 启动 Rider 时自动读取 shell 配置。项目已将 `packageManager` 固定为 `pnpm@12.3.4`，且启动 profile 禁止 Corepack 在无交互界面中等待下载确认。首次使用前请在 `src/Portwise.Web` 执行 `corepack install --global pnpm@12.3.4` 和 `pnpm install --frozen-lockfile`。

## 配置

ASP.NET Core 按默认规则加载 `appsettings.json` 和当前环境对应的 `appsettings.{Environment}.json`，环境变量会覆盖文件中的值。生产配置文件只保存非敏感默认值。

常用配置：

| 环境变量 | 用途 | 默认值 |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | ASP.NET Core 环境 | `Production`（容器中建议显式设置） |
| `ConnectionStrings__Default` | PostgreSQL 连接字符串 | `Host=localhost;Port=5432;Database=portwise;Username=portwise;Password=portwise` |
| `FtShare__McpEndpoint` | FTShare MCP Streamable HTTP 地址 | `https://market.ft.tech/gateway/mcp` |
| `FtShare__RequestTimeoutSeconds` | 每次 HTTP attempt 超时；完整 MCP exchange deadline 会按重试次数扩展 | `30` |
| `FtShare__MaxRetryCount` | MCP 请求最大重试次数 | `2` |
| `FtShare__RetryDelayMilliseconds` | 重试间隔 | `250` |
| `DailySync__Enabled` | 是否启用每日同步 | `true` |
| `DailySync__LocalTime` | 每日同步时间 | `18:00` |
| `DailySync__TimeZoneId` | 每日同步时区 | `Asia/Shanghai` |

`FtShare` 和 `DailySync` 配置会在 Host 启动时校验。地址、时间格式、时区、超时和重试范围无效时，应用会直接报告配置错误，不会等到首次同步请求才失败。

FTShare 通过命名的 `IHttpClientFactory` client 复用连接池；标准 resilience 管线负责暂态 HTTP 响应、`Retry-After`、jitter、指数退避和每次 attempt 超时。响应流中断会在 transport 边界被明确分类，并由 exchange resilience pipeline 重试；完整 MCP exchange 仍有一个有界 deadline，以覆盖 session 初始化和流式响应读取。当前 FTShare 工具为只读契约，若接入有副作用的 MCP 工具，必须使用独立 client 并重新声明幂等性。

FTShare 工具名称和参数名也可以通过 `FtShare__*` 配置覆盖：

```text
FtShare__McpEndpoint=https://<ftshare-mcp-endpoint>/mcp
FtShare__StockProfileToolName=get_stock_profile
FtShare__StockMarketDataToolName=get_stock_market_data
FtShare__StockDividendEventsToolName=get_stock_dividend_events
FtShare__StockFinancialSnapshotsToolName=get_stock_financial_snapshots
FtShare__SecurityCodeArgumentName=security_code
FtShare__ExchangeCodeArgumentName=exchange_code
```

不要把 FTShare key、MCP 凭证或任何个人数据写入源代码、`appsettings*.json`、Dockerfile、镜像层或 Git。若 MCP 部署需要认证，应在 MCP 网关或运行环境的密钥管理中注入，应用仓库只保存 endpoint 和非敏感默认配置。

未配置或暂时无法连接 FTShare 时，首次建账仍然可以完成；股票资料同步会在后台记录失败并等待下一次手动或交易日同步，不应阻塞用户建立组合。

## 页面与路由

前端使用轻卡通理财手帐风格，面向不熟悉投资术语的小白用户。页面路由由前端应用统一处理：

| 路由 | 用途 |
| --- | --- |
| `/onboarding` | 首次保存工作区基础配置 |
| `/overview` | 今日决策、组合状态和下一步提示 |
| `/stocks` | 股票资料、行情、股息、财务和模型结果 |
| `/budget` | 组合预算与现金流水 |
| `/portfolio` | 持仓摘要和模拟交易记录 |
| `/settings` | 系统偏好、股票数据 Provider 与 AI Provider 配置 |
| `/strategy` | 每只股票独立的策略参数 |
| `/404` | 未知前端路由 |
| `/error` | 全局初始化或 API 读取失败 |

根路径 `/` 会进入 `/overview`。首次运行且尚未完成初始化时会进入 `/onboarding`；初始化完成后即使部分 Provider 未配置，也允许进入系统并显示受限能力。未知路径和全局错误分别归一到 `/404` 与 `/error`，恢复成功后返回 `/overview`。

## API 入口

API 使用 URL Segment 版本号，当前版本为 `v1`，完整接口和请求模型以 Swagger 为准。

| API | 用途 |
| --- | --- |
| `GET /api/v1/initialization` | 查询初始化完成标记、偏好和能力 readiness |
| `POST /api/v1/initialization/complete` | 以一个数据库事务保存初始化基础配置和可选 Provider/Route |
| `GET /api/v1/stocks` | 获取关注股票和持仓摘要 |
| `POST /api/v1/stocks/sync` | 手动触发全部股票资料同步 |
| `GET /api/v1/stocks/{securityCode}/{exchangeCode}/analysis` | 获取单只股票分析与交易参考 |
| `GET /api/v1/stocks/{securityCode}/{exchangeCode}/model-parameters` | 获取当前生效模型参数 |
| `POST /api/v1/stocks/model-parameters` | 保存单只股票模型参数 |
| `POST /api/v1/stocks/{securityCode}/{exchangeCode}/price-observations/sync` | 同步单只股票行情 |
| `POST /api/v1/stocks/{securityCode}/{exchangeCode}/dividend-events/sync` | 同步单只股票股息事件 |
| `POST /api/v1/stocks/{securityCode}/{exchangeCode}/financial-snapshots/sync` | 同步单只股票财务快照 |
| `GET /api/v1/budgets/summary` | 获取预算摘要 |
| `POST /api/v1/budgets/entries` | 记录现金流水 |
| `POST /api/v1/portfolio/trades` | 记录模拟交易并更新持仓 |
| `GET /api/v1/recommendations` | 获取组合级建议 |
| `POST /api/v1/recommendations/snapshots` | 保存一次建议快照 |

## 数据与模型口径

- 股票代码以文本保存，支持 `000001` 等带前导零的代码；对外使用 `security_code + exchange_code`；
- 默认模型股息为最近十二个月已经实施的常规现金股息，即 TTM DPS；
- 价格区域使用每只股票独立配置的收益率阈值；连续两个有效交易日确认后，才驱动操作建议；
- `available_budget_amount` 是扣除现金储备后的可用预算，不等于现金流水的实际余额；
- `core_shares` 是希望长期保留的核心仓，卫星仓用于阶段性分批操作；
- 数据不足、来源失败、股息取消或可靠性检查未通过时，系统应降低建议强度或停止生成买入股数。

完整模型规则、字段口径和验收用例见：

- [股息策略收益与分批交易参考模型](docs/dividend-strategy-quant-model.md)
- [股息策略研究摘要](docs/dividend-strategy-research.md)
- [应用架构设计](docs/architecture-design.md)
- [架构决策记录](docs/adr/README.md)

## 项目结构

```text
Portwise/
├── src/
│   ├── Portwise/                 # ASP.NET Core Host、Controller、配置、健康检查
│   ├── Portwise.Application/     # AppService、Contracts、DTO、验证、异常与本地化
│   ├── Portwise.Domain/          # Entity、领域规则、量化模型和领域代码
│   ├── Portwise.Infrastructure/  # EF Core、Uow/Repository、PostgreSQL、FTShare Adapter
│   └── Portwise.Web/             # React、shadcn/ui 风格组件、页面 feature 和 API 调用
├── tests/
│   ├── Portwise.Domain.Tests/         # xUnit 领域单元测试
│   └── Portwise.Application.Tests/    # xUnit + Moq 应用层单元测试
├── locales/                              # zh-CN / en-US 多语言资源
├── docs/                                 # 架构、模型和研究文档
├── Dockerfile                            # 前后端单镜像构建
└── Portwise.slnx
```

前端 feature 按页面组织自己的页面、API、样式和骨架屏；公共 Header、Footer、页面框架和 shadcn/ui 基础组件位于 `src/Portwise.Web/src/components`。后端按 Domain、Application、Infrastructure、Host 分层，接口统一放在各层的 `Contracts` 文件夹。

## 测试与质量检查

运行后端测试：

```bash
dotnet test Portwise.slnx
```

运行前端检查：

```bash
cd src/Portwise.Web
pnpm run typecheck
pnpm run lint
pnpm run build
```

后端测试覆盖 Domain、Application、Host 和 Infrastructure，统一使用 xUnit；Application 测试使用 Moq 模拟 Uow、Repository、Provider 和调度器，Infrastructure 不创建数据库测试容器，迁移通过编译、pending-model-changes、生成 SQL 和 PostgreSQL Compose smoke 验证，Host 测试覆盖同步执行、Options 校验和调度时间。UI 页面验收还应在浏览器中检查目标分辨率、加载态、空态、错误态、夜间主题和中英文切换，构建成功不等于视觉验收完成。

## 许可

本项目使用 [MIT License](LICENSE)。
