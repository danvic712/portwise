# ADR-0003: 从 Host 元数据发布 HTTP 合约

- Status: Accepted
- Date: 2026-09-09

## Context

Controller、API Versioning 和前端请求曾存在两套容易漂移的描述。手工维护 TypeScript 路径和 DTO 会让修改只在运行时才暴露，削弱 HTTP seam 的测试 surface。

## Decision

Host 的 Controller 元数据是 HTTP 合约的单一事实源。build-time 生成版本化 OpenAPI 文档，前端由 `openapi-typescript` 生成 transport types；`api-contract.ts` 是唯一 wire-to-UI Adapter，负责路径展开、请求方法约束和 wire numeric 归一化。CI 与 Docker build 在前端构建前执行 contract generation 和 drift check；运行时注册与设计时生成共用同一组 OpenAPI 扩展。

## Consequences

- 新增或修改 Controller DTO 会在生成、类型检查或 drift check 阶段尽早失败。
- feature 只依赖领域友好 alias，不需要了解生成文件的细节，调用方获得更多 leverage。
- 生成的 OpenAPI/TypeScript 文件是派生物；如果它们与 Host 元数据不一致，应重新生成而不是手工修补。
