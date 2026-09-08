# ADR-0002: 使用单一 ASP.NET Core Host 镜像

- Status: Accepted
- Date: 2026-09-09

## Context

Portwise 需要一个个人可部署的运行单元。前端是静态资源，后端是 ASP.NET Core Controllers；拆成多个运行时镜像会增加反向代理、跨进程配置和本地持久化协调成本。当前产品也不依赖 Cloudflare 或其他平台专属运行时能力。

## Decision

前端和后端在构建期分别编译，最终合并到一个 ASP.NET Core Host 镜像。Host 同时提供 `/api`、静态前端和 SPA fallback；用户数据写入容器外的 `/app/data` volume。镜像不保存凭据或用户数据，部署只需要运行一个 HTTP 入口。

## Consequences

- 构建 pipeline 必须保留前端 contract/build 与 .NET publish 的明确顺序。
- Host 的 HTTP seam 是唯一对外入口，健康检查和日志也由同一进程统一编排。
- 若未来需要独立扩展前端或后端，必须先提供容量、隔离或故障域证据，再重新评估本 ADR。
