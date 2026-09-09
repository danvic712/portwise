# 部署资料

`deploy/` 集中保存与部署运行时相关的辅助文件。

- `docker/`：Docker Compose 使用的 PostgreSQL 初始化脚本；当前脚本在首次创建空数据卷时启用 pgvector 扩展。
- 根目录的 `docker-compose.yml`：本地 PostgreSQL 17（pgvector）和 Portwise Host 的统一启动入口。

初始化脚本只会在 PostgreSQL 数据卷第一次创建时执行；删除并重新创建空 volume 后才会再次运行。
