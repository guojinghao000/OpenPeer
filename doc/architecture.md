# OpenPeer — 项目架构文档

## 1. 项目概述

**OpenPeer** 是一个读者评价驱动的学术论文平台。作者直接发布论文，注册读者对论文进行 1~5 星评分并发表评论。论文最终得分由读者评分加权平均动态计算，可融入读者信誉机制。平台无传统审稿人角色。

### 1.1 核心目标

- 降低学术成果发布门槛，加速知识传播
- 以社区评价替代传统同行评审
- 通过信誉机制激励高质量评价
- 提供透明、开放的学术交流环境

---

## 2. 技术栈

### 2.1 后端

| 组件 | 技术选型 | 版本 | 说明 |
|------|----------|------|------|
| 运行时 | ASP.NET Core Web API | .NET 8 | 跨平台高性能 REST API |
| 语言 | C# | 12 | 最新语言特性 |
| ORM | Entity Framework Core | 8.x | 数据访问抽象 |
| 数据库驱动 | Npgsql | 8.x | PostgreSQL 驱动 |
| 认证 | JWT + Identity | 8.x | 无状态令牌认证 |
| 参数校验 | FluentValidation | 11.x | 声明式校验规则 |
| 对象映射 | Mapster | 7.x | 零反射高性能映射 |
| 日志 | Serilog | 8.x | 结构化日志 |
| API 文档 | Swashbuckle | 6.x | OpenAPI / Swagger UI |

### 2.2 前端

| 组件 | 技术选型 | 版本 | 说明 |
|------|----------|------|------|
| 框架 | Vue 3 | 3.4+ | Composition API |
| 构建工具 | Vite | 5.x | 极速 HMR 与构建 |
| 语言 | TypeScript | 5.x | 类型安全 |
| UI 组件库 | Element Plus | 2.x | Vue 3 生态最成熟 |
| 状态管理 | Pinia | 2.x | 官方状态管理 |
| 路由 | Vue Router | 4.x | SPA 路由 |
| HTTP 客户端 | Axios | 1.x | 请求封装与拦截 |
| 样式 | SCSS + Element Plus 主题 | — | 可定制化 |

### 2.3 基础设施

| 组件 | 技术选型 |
|------|----------|
| 数据库 | PostgreSQL 16 |
| 容器化 | Docker + Docker Compose |
| 反向代理 | Nginx (生产环境) |
| 文件存储 | 本地文件系统 (`Uploads/`) |

---

## 3. 后端架构

### 3.1 分层设计 (Clean Architecture)

```
┌────────────────────────────────────────────────┐
│                 OpenPeer.Api                    │
│      Controllers, Middleware, DI, Config        │
│     Presentation Layer — 仅处理 HTTP 请求       │
├────────────────────────────────────────────────┤
│              OpenPeer.Application               │
│   Services, DTOs, Interfaces, Validators,      │
│   Application Layer — 业务逻辑编排              │
├────────────────────────────────────────────────┤
│              OpenPeer.Domain                    │
│   Entities, ValueObjects, Enums, DomainEvents  │
│   Domain Layer — 核心业务规则                   │
├────────────────────────────────────────────────┤
│              OpenPeer.Infrastructure            │
│  DbContext, Repositories, JWT, FileStorage...  │
│  Infrastructure Layer — 技术实现                │
└────────────────────────────────────────────────┘
```

**依赖方向**: `Api → Application → Domain ← Infrastructure`

- `Domain` 不依赖任何外部层，是系统的内核
- `Application` 依赖 `Domain`，定义接口契约
- `Infrastructure` 实现 `Application` 定义的接口，依赖 `Domain`
- `Api` 作为宿主，负责组装依赖注入

### 3.2 项目目录结构

```
src/
├── OpenPeer.Api/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── PapersController.cs
│   │   ├── RatingsController.cs
│   │   ├── CommentsController.cs
│   │   ├── CategoriesController.cs
│   │   └── UsersController.cs
│   ├── Middleware/
│   │   ├── ExceptionMiddleware.cs
│   │   └── RequestLoggingMiddleware.cs
│   ├── Extensions/
│   │   ├── ServiceCollectionExtensions.cs
│   │   └── ApplicationBuilderExtensions.cs
│   ├── Filters/
│   │   └── RateLimitFilter.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Program.cs
│   └── Dockerfile
│
├── OpenPeer.Application/
│   ├── Services/
│   │   ├── AuthService.cs
│   │   ├── PaperService.cs
│   │   ├── RatingService.cs
│   │   ├── CommentService.cs
│   │   └── UserService.cs
│   ├── DTOs/
│   │   ├── Auth/
│   │   │   ├── RegisterRequest.cs
│   │   │   ├── LoginRequest.cs
│   │   │   └── TokenResponse.cs
│   │   ├── Papers/
│   │   │   ├── CreatePaperRequest.cs
│   │   │   ├── UpdatePaperRequest.cs
│   │   │   └── PaperDto.cs
│   │   ├── Ratings/
│   │   │   └── RatingDto.cs
│   │   ├── Comments/
│   │   │   ├── CreateCommentRequest.cs
│   │   │   └── CommentDto.cs
│   │   └── Common/
│   │       ├── PagedRequest.cs
│   │       ├── PagedResponse.cs
│   │       └── ApiResponse.cs
│   ├── Validators/
│   │   ├── RegisterRequestValidator.cs
│   │   ├── CreatePaperRequestValidator.cs
│   │   └── CreateCommentRequestValidator.cs
│   ├── Interfaces/
│   │   ├── IPaperRepository.cs
│   │   ├── IRatingRepository.cs
│   │   ├── ICommentRepository.cs
│   │   ├── IUserRepository.cs
│   │   ├── ICategoryRepository.cs
│   │   └── IFileStorageService.cs
│   └── Mappings/
│       └── MapsterConfig.cs
│
├── OpenPeer.Domain/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Paper.cs
│   │   ├── Rating.cs
│   │   ├── Comment.cs
│   │   ├── Category.cs
│   │   └── PaperCategory.cs
│   ├── Enums/
│   │   ├── PaperStatus.cs
│   │   └── UserRole.cs
│   └── ValueObjects/
│       └── RatingScore.cs
│
└── OpenPeer.Infrastructure/
    ├── Data/
    │   ├── AppDbContext.cs
    │   ├── Configurations/
    │   │   ├── UserConfiguration.cs
    │   │   ├── PaperConfiguration.cs
    │   │   ├── RatingConfiguration.cs
    │   │   ├── CommentConfiguration.cs
    │   │   └── CategoryConfiguration.cs
    │   └── Migrations/
    ├── Repositories/
    │   ├── PaperRepository.cs
    │   ├── RatingRepository.cs
    │   ├── CommentRepository.cs
    │   ├── UserRepository.cs
    │   └── CategoryRepository.cs
    ├── Auth/
    │   ├── JwtService.cs
    │   └── JwtOptions.cs
    ├── Storage/
    │   └── LocalFileStorageService.cs
    └── Extensions/
        └── InfrastructureServiceExtensions.cs
```

---

## 4. 前端架构

### 4.1 项目目录结构

```
src/OpenPeer.Web/
├── public/
│   └── favicon.ico
├── src/
│   ├── api/                     # HTTP 请求层
│   │   ├── client.ts            # Axios 实例 + 拦截器
│   │   ├── auth.ts              # 认证相关 API
│   │   ├── papers.ts            # 论文相关 API
│   │   ├── ratings.ts           # 评分相关 API
│   │   ├── comments.ts          # 评论相关 API
│   │   └── categories.ts        # 分类相关 API
│   ├── assets/                  # 静态资源
│   │   └── styles/
│   │       ├── variables.scss   # SCSS 变量
│   │       └── global.scss      # 全局样式
│   ├── components/              # 可复用组件
│   │   ├── common/
│   │   │   ├── AppHeader.vue
│   │   │   ├── AppFooter.vue
│   │   │   ├── AppPagination.vue
│   │   │   ├── AppLoading.vue
│   │   │   ├── AppEmpty.vue
│   │   │   └── StarRating.vue
│   │   ├── paper/
│   │   │   ├── PaperCard.vue
│   │   │   ├── PaperList.vue
│   │   │   └── PaperFilter.vue
│   │   └── comment/
│   │       ├── CommentItem.vue
│   │       ├── CommentList.vue
│   │       └── CommentForm.vue
│   ├── composables/             # 组合式函数
│   │   ├── useAuth.ts
│   │   ├── usePagination.ts
│   │   └── useDebounce.ts
│   ├── layouts/
│   │   ├── DefaultLayout.vue
│   │   └── AuthLayout.vue
│   ├── router/
│   │   └── index.ts
│   ├── stores/                  # Pinia 状态管理
│   │   ├── auth.ts
│   │   ├── papers.ts
│   │   └── ui.ts
│   ├── types/                   # TypeScript 类型定义
│   │   ├── api.ts
│   │   ├── paper.ts
│   │   ├── user.ts
│   │   ├── rating.ts
│   │   └── comment.ts
│   ├── utils/
│   │   ├── format.ts            # 日期、数字格式化
│   │   └── storage.ts           # localStorage 封装
│   ├── views/                   # 路由页面
│   │   ├── HomeView.vue
│   │   ├── LoginView.vue
│   │   ├── RegisterView.vue
│   │   ├── PaperDetailView.vue
│   │   ├── PaperUploadView.vue
│   │   ├── ProfileView.vue
│   │   └── NotFoundView.vue
│   ├── App.vue
│   └── main.ts
├── index.html
├── vite.config.ts
├── tsconfig.json
├── tsconfig.node.json
├── package.json
└── Dockerfile
```

### 4.2 路由设计

| 路径 | 页面 | 需要认证 | 说明 |
|------|------|----------|------|
| `/` | HomeView | 否 | 论文列表、搜索、排序 |
| `/papers/:id` | PaperDetailView | 否(查看) / 是(操作) | 论文详情+评分+评论 |
| `/upload` | PaperUploadView | 是 | 上传新论文 |
| `/login` | LoginView | 否(仅未登录) | 登录页 |
| `/register` | RegisterView | 否(仅未登录) | 注册页 |
| `/profile` | ProfileView | 是 | 个人中心 |
| `/:pathMatch(.*)*` | NotFoundView | 否 | 404 页面 |

### 4.3 状态管理设计 (Pinia)

**authStore**: 用户认证状态
- `user`: 当前用户信息 | null
- `accessToken`: JWT 令牌
- `isAuthenticated`: 计算属性
- `login(credentials)`, `register(data)`, `logout()`, `refreshToken()`

**papersStore**: 论文列表状态
- `papers`: 论文列表
- `total`: 总数
- `loading`: 加载状态
- `filters`: 筛选条件 (关键词、分类、排序)
- `fetchPapers()`, `fetchPaperById(id)`, `createPaper(data)`

**uiStore**: 全局 UI 状态
- `sidebarCollapsed`: 侧栏折叠
- `theme`: 主题
- `showLoading`, `showMessage`

---

## 5. 容器化与部署

### 5.1 Docker Compose 拓扑

```
                  ┌───────────────────┐
                  │   openpeer-web     │
                  │   Nginx :80        │
                  │   静态托管 Vue SPA │
                  └────────┬──────────┘
                           │ proxy /api → :5000
                  ┌────────▼──────────┐
                  │   openpeer-api    │
                  │   .NET 8 :5000    │
                  │   Web API         │
                  └────────┬──────────┘
                           │
                  ┌────────▼──────────┐
                  │   openpeer-db     │
                  │   PostgreSQL :5432│
                  │   数据持久化      │
                  └───────────────────┘
```

### 5.2 容器编排文件规划

```yaml
# docker-compose.yml
services:
  openpeer-db:
    image: postgres:16-alpine
    volumes:
      - pgdata:/var/lib/postgresql/data
    environment:
      POSTGRES_DB: openpeer
      POSTGRES_USER: openpeer
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    ports:
      - "5432:5432"

  openpeer-api:
    build: ./src/OpenPeer.Api
    volumes:
      - uploads:/app/Uploads
    environment:
      ConnectionStrings__Default: "Host=openpeer-db;Database=openpeer;Username=openpeer;Password=${DB_PASSWORD}"
      Jwt__Secret: ${JWT_SECRET}
    ports:
      - "5000:8080"
    depends_on:
      - openpeer-db

  openpeer-web:
    build: ./src/OpenPeer.Web
    ports:
      - "80:80"
    depends_on:
      - openpeer-api

volumes:
  pgdata:
  uploads:
```

### 5.3 开发环境启动

```bash
# 首次启动 (构建镜像)
docker compose up -d --build

# 后续启动
docker compose up -d

# 访问
# API:      http://localhost:5000
# Swagger:  http://localhost:5000/swagger
# Web:      http://localhost:80
# 数据库:   localhost:5432
```

---

## 6. 安全设计

| 安全措施 | 实现方式 |
|----------|----------|
| 密码安全 | ASP.NET Core Identity PBKDF2 哈希 |
| JWT 签名 | HMAC-SHA256, Secret 最小 256bit |
| Token 过期 | Access Token 1h, Refresh Token 7d |
| 文件上传限制 | 仅 `.pdf` 格式, 最大 10MB |
| 输入校验 | FluentValidation 服务端校验, Element Plus 前端校验 |
| SQL 注入 | EF Core 参数化查询, 禁止原始 SQL |
| XSS | Vue 3 默认转义, Content-Security-Policy |
| CORS | 白名单配置, 非开放通配符 |
| 限流 | API 限流中间件 (登录 5/min, 上传 10/min) |
| HTTPS | 生产环境强制 TLS |

---

## 7. 性能设计

| 优化点 | 策略 |
|--------|------|
| 数据库索引 | 覆盖高频查询字段 (外键、排序字段、搜索字段) |
| 分页 | 论文列表每页 20 条, 评论每页 20 条 |
| 评分缓存 | `Papers.AverageRating` / `RatingCount` 冗余字段 |
| 前端懒加载 | Vue Router 路由级别代码分割 |
| 静态资源 | Gzip/Brotli 压缩, 合理 Cache-Control |
| 全文搜索 | PostgreSQL GIN 索引 + `tsvector` |

---

## 8. 扩展性设计

### 8.1 信誉机制 (预留扩展点)

相关字段已在数据库和实体中预留:
- `User.ReputationScore` (float, 默认 1.0)
- 评分计算时可选择是否引入信誉权重
- 信誉积分规则可作为独立的 Service 实现，不影响现有评分子系统

### 8.2 文件存储可替换

通过 `IFileStorageService` 接口抽象:
- 初期: `LocalFileStorageService` (本地文件系统)
- 扩展: `S3FileStorageService`, `MinioFileStorageService`, `OssFileStorageService`
- 切换只需修改 DI 注册, 业务层零改动

### 8.3 数据库可替换

通过 Repository 模式, EF Core Provider 切换仅需修改:
1. NuGet 包替换 (Npgsql → Pomelo/SqlServer)
2. 连接字符串修改
3. Migration 重新生成

---

## 9. 开发约定

### 9.1 代码风格

- **后端**: 遵循 [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
  - PascalCase: 类名、方法名、属性名
  - camelCase: 局部变量、参数
  - `_camelCase`: 私有字段
  - 使用 `file-scoped namespace`
- **前端**: ESLint + Prettier, 遵循 [Vue 3 Style Guide](https://vuejs.org/style-guide/)
  - PascalCase: 组件文件名
  - camelCase: 函数、变量
  - 优先 Composition API (`<script setup lang="ts">`)

### 9.2 Git 分支策略

- `main`: 稳定发布版本
- `develop`: 日常开发主线
- `feature/<功能名>`: 从 `develop` 切出, 合并回 `develop`
- `fix/<问题名>`: 从 `develop` 切出, 合并回 `develop`

### 9.3 提交信息格式

```
<type>(<scope>): <subject>

可选类型: feat, fix, docs, style, refactor, test, chore, perf
可选范围: api, web, db, auth, paper, rating, comment, config

示例:
  feat(paper): add paper upload with PDF validation
  fix(auth): resolve token refresh race condition
  docs(api): document rating endpoints
```

### 9.4 统一 API 响应格式

所有 API 端点返回统一结构:

```json
{
  "code": 200,
  "message": "操作成功",
  "data": { ... }
}
```

错误响应:

```json
{
  "code": 400,
  "message": "请求参数有误",
  "errors": [
    { "field": "Title", "message": "标题不能为空" }
  ]
}
```
