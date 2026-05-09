# OpenPeer

> 去中心化学术评价平台 — 作者自由发布论文，社区读者评分评论，取代传统同行评审。

## 核心理念

- **去中心化**: 无传统审稿人，由社区驱动质量评价
- **开放获取**: 论文公开发布，降低学术壁垒
- **透明评价**: 所有评分和评论公开可见
- **信誉驱动**: 高质量评价者通过信誉分获得更大权重（预留）

## 技术栈

| 层级 | 技术 |
|------|------|
| 后端 | ASP.NET Core 8 Web API (C# 12) |
| 前端 | Vue 3 + Vite + TypeScript + Element Plus |
| 数据库 | PostgreSQL 16 |
| ORM | EF Core 8 + Npgsql |
| 认证 | JWT + ASP.NET Core Identity |
| 校验 | FluentValidation |
| 映射 | Mapster |
| 日志 | Serilog |
| 状态管理 | Pinia |
| 容器化 | Docker + Docker Compose |

## 架构

```
Api → Application → Domain ← Infrastructure
```

- **Domain** — 核心实体、值对象、枚举，零外部依赖
- **Application** — 服务、DTO、验证器、仓储接口
- **Infrastructure** — DbContext、仓储实现、JWT、文件存储
- **Api** — 控制器、中间件、依赖注入组装

前端位于 `src/OpenPeer.Web/`，标准 Vue 3 SPA。

## 项目状态

**M0 — 预实施阶段。** 设计文档已完成，源代码尚未搭建。

| 里程碑 | 目标 |
|--------|------|
| **M1** | 项目骨架、认证系统 |
| **M2** | 论文 CRUD、文件上传、搜索 |
| **M3** | 评分、评论系统 |
| **M4** | 分类管理、个人中心 |
| **M5** | 测试、优化、文档 |

## 快速开始

```bash
# 搭建项目骨架
/scaffold

# 启动所有服务
docker compose up -d --build

# 访问
# API:      http://localhost:5000
# Swagger:  http://localhost:5000/swagger
# Web:      http://localhost:80
```

## 项目结构

```
OpenPeer/
├── doc/
│   ├── requirements.md         # 需求文档（用户角色、功能、NFR）
│   ├── architecture.md         # 架构设计（分层、路由、安全）
│   ├── database-design.md      # 数据库设计（6 表、ER 图、索引）
│   └── api-design.md           # API 设计（27 端点、请求/响应）
├── src/
│   ├── OpenPeer.Api/           # Web API 入口
│   ├── OpenPeer.Application/   # 业务逻辑层
│   ├── OpenPeer.Domain/        # 领域层
│   ├── OpenPeer.Infrastructure/# 基础设施层
│   └── OpenPeer.Web/           # Vue 3 前端
├── docker-compose.yml
└── README.md
```

## 开发命令

```bash
# 后端
dotnet build
dotnet test
dotnet ef migrations add <Name> -p src/OpenPeer.Infrastructure -s src/OpenPeer.Api
dotnet ef database update -p src/OpenPeer.Infrastructure -s src/OpenPeer.Api

# 前端
npm install
npm run dev
npm run build

# Docker
docker compose up -d --build
docker compose down -v
```

## API 约定

所有端点返回统一结构：

```json
{
  "code": 200,
  "message": "操作成功",
  "data": { ... }
}
```

详细 API 文档见 [doc/api-design.md](doc/api-design.md)。

## 开发约定

- **主键**: UUID (`gen_random_uuid()`)，不使用自增整数
- **API 响应**: 统一 `{ code, message, data }` 格式
- **C# 命名**: PascalCase 公开，`_camelCase` 私有字段，file-scoped namespace
- **Vue 命名**: PascalCase 组件文件名，`<script setup lang="ts">` 独占
- **软删除**: 论文和评论使用软删除 (`IsDeleted`)
- **评分规则**: 每用户每论文仅一个评分，每次评分后事务内重算均分
- **文件上传**: 仅 PDF，≤ 10MB
- **认证**: 无状态 JWT，禁止 Cookie 认证
- **Git 提交**: `<type>(<scope>): <subject>`

## 许可证

MIT
