# ADR-0001: V1 使用单一组合上下文

- Status: Accepted
- Date: 2026-09-09

## Context

Portwise 是面向单一用户的私人工具。当前建账流程只有一个投资组合，Application 的用例也不要求调用方携带 `portfolio_id`。如果把多组合字段提前扩散到每个 Interface、DTO 和查询，会让尚未存在的租户隔离复杂度进入所有 module。

## Decision

V1 保持单一组合上下文：由 Application 解析唯一组合，数据库使用 `portfolio_scope` 的唯一约束保证只有一条组合记录。并发建账失败映射为稳定的 `setup_already_completed` 错误。未来引入多组合时，必须同时扩展外部 Interface、数据隔离规则和迁移，不通过隐式默认值兼容。

## Consequences

- 组合相关 Interface 保持较小，调用方不需要重复传递当前版本无法改变的标识。
- 唯一约束是持久化 seam 的最后防线，不能只依赖 Application 的先读后写检查。
- 多组合是显式的后续设计，不会在当前 module 中留下半成品的 `portfolio_id` 参数。
