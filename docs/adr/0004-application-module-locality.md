# ADR-0004: Application 类型和映射归属 module

- Status: Accepted
- Date: 2026-09-09

## Context

Application 的 Setup、Stocks、Portfolio 和 Recommendations 已按业务能力分层，但全局 `Contracts`、`Dtos`、`Validators` 和 `ApplicationMapper` 会重新形成技术桶。调用一个用例需要跨越多个浅层目录，类型归属也只能靠约定保持。

## Decision

每个 module 在自己的 `Contracts/`、`Dtos/` 和 `Validators/` namespace 中拥有 public 类型，并在 module 根目录拥有自己的 Mapperly mapper。根目录只保留错误、本地化、诊断、共享持仓 DTO 和通用 A 股规则。`ModuleNamespaceArchitectureTests` 作为可执行 guard，禁止新的 public 类型泄漏回根技术桶。

## Consequences

- module 的 interface、实现、validator、DTO 和 mapper 形成更深的 locality，修改和验证集中在同一 seam。
- 跨 module 引用必须显式导入对方 namespace，依赖关系更容易被代码审查发现。
- 共享类型只有在确实跨 module 时才提升；否则应留在拥有它的 module，避免为了方便制造新的汇聚点。
