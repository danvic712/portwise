# ADR-0005: PostgreSQL 作为唯一关系数据库

- Status: Accepted
- Date: 2026-09-09

## Context

Portwise 当前把关系数据保存在容器外的 SQLite 文件中，运行时注册、migration、类型映射、异常识别、测试和部署文档都依赖 SQLite 语义。项目不需要保留现有数据库或历史数据，因此可以用 fresh migration 完成 provider 切换；继续维护双 provider 会增加类型、错误处理和 schema 漂移风险。

## Decision

Portwise 只支持 PostgreSQL 17 及以上版本，以 PostgreSQL 17 作为 SQL 生成和本地验证的最低兼容基线；系统使用 `public` schema 和一份新的初始 migration，不迁移 SQLite 历史数据。所有表、字段、主键、外键、索引、唯一约束、检查约束和 EF migration 元数据使用小写 `snake_case`；migration 历史表为 `ef_migrations`，字段为 `migration_id` 和 `product_version`。Host 继续在启动时自动应用 migration，本地实施提供包含 PostgreSQL 17 的 Docker Compose，生产环境使用外部 PostgreSQL 17 或更高兼容版本。

## Consequences

- ASP.NET Core Host 镜像仍保持单一应用入口，但 PostgreSQL 成为独立运行时依赖。
- ADR-0002 中关于 `/app/data` SQLite volume 的持久化描述被本 ADR 替代；其余单 Host 镜像决策继续有效。
- 应用不再保留 SQLite provider、文件数据库、SQLite migration 或 SQLite 错误码处理。
- 定制 EF migration 历史字段需要依赖 Npgsql/EF Core 的内部 history repository 扩展面，每次升级 provider 都必须复核该适配和生成 SQL。
- 本次不新增数据库自动化测试；迁移通过编译、模型/SQL 审查和一次性 Docker Compose smoke verification 验证。
