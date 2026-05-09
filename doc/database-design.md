# OpenPeer — 数据库设计文档

## 1. 概述

- **数据库**: PostgreSQL 16
- **ORM**: Entity Framework Core 8
- **迁移**: EF Core Code-First Migrations
- **主键策略**: UUID (避免自增主键的安全和迁移问题)
- **字符集**: UTF-8

---

## 2. ER 图

```
 ┌──────────────┐          ┌──────────────┐          ┌──────────────┐
 │    Users     │          │   Ratings    │          │   Papers     │
 │──────────────│          │──────────────│          │──────────────│
 │ PK Id (UUID) │──┐       │ PK Id (UUID) │          │ PK Id (UUID) │
 │ UserName     │  │       │ FK PaperId ──│─────────►│ Title        │
 │ Email        │  │       │ FK UserId  ──│──┐       │ Abstract     │
 │ PasswordHash │  │       │ Score (1-5)  │  │       │ FilePath     │
 │ Reputation   │  │       │ CreatedAt    │  │       │ FileSize     │
 │ Role         │  │       │ UpdatedAt    │  │    ┌──│ FK AuthorId──│──┐
 │ Bio          │  │       └──────────────┘  │    │  │ Status       │  │
 │ AvatarPath   │  │                         │    │  │ AvgRating    │  │
 │ CreatedAt    │  │       ┌──────────────┐  │    │  │ RatingCount  │  │
 │ UpdatedAt    │  │       │  Comments    │  │    │  │ ViewCount    │  │
 └──────────────┘  │       │──────────────│  │    │  │ PublishedAt  │  │
                   │       │ PK Id (UUID) │  │    │  │ UpdatedAt    │  │
                   │       │ FK PaperId ──│──│────┘  └──────────────┘  │
                   │       │ FK UserId  ──│──┤                          │
                   │       │ ParentId    │  │    ┌───────────────────┐  │
                   ├──────►│ Content     │  │    │  PaperCategories  │  │
                   │       │ IsDeleted   │  │    │───────────────────│  │
                   │       │ CreatedAt   │  │    │ FK PaperId  (PK) ─│──┤
                   │       │ UpdatedAt   │  │    │ FK CategoryId(PK) │  │
                   │       └──────────────┘  │    └────────┬──────────┘  │
                   │                         │             │              │
                   │                         │    ┌────────▼──────────┐  │
                   │                         │    │    Categories     │  │
                   │                         │    │───────────────────│  │
                   │                         │    │ PK Id (UUID)      │  │
                   │                         │    │ Name              │  │
                   │                         │    │ Description       │  │
                   │                         │    │ CreatedAt         │  │
                   │                         │    └───────────────────┘  │
                   │                         │                           │
                   └─────────────────────────┴───────────────────────────┘
```

**关系描述:**
- User (1) ── (N) Paper: 一个用户可发布多篇论文
- User (1) ── (N) Rating: 一个用户可对多篇论文评分
- User (1) ── (N) Comment: 一个用户可发表多条评论
- Paper (1) ── (N) Rating: 一篇论文可被多个用户评分
- Paper (1) ── (N) Comment: 一篇论文可有多条评论
- Paper (N) ── (N) Category: 论文与分类多对多 (通过 PaperCategories)
- Comment (1) ── (N) Comment: 评论自引用 (支持回复)

---

## 3. 表结构

### 3.1 Users

| 列名 | 类型 | 约束 | 默认值 | 说明 |
|------|------|------|--------|------|
| `Id` | `uuid` | PK | `gen_random_uuid()` | 用户唯一标识 |
| `UserName` | `varchar(50)` | NOT NULL, UNIQUE | — | 用户名 |
| `NormalizedUserName` | `varchar(50)` | NOT NULL | — | Identity 规范化用户名 |
| `Email` | `varchar(256)` | NOT NULL, UNIQUE | — | 邮箱地址 |
| `NormalizedEmail` | `varchar(256)` | NOT NULL | — | Identity 规范化邮箱 |
| `EmailConfirmed` | `boolean` | NOT NULL | `false` | 邮箱是否确认 (预留) |
| `PasswordHash` | `text` | NOT NULL | — | PBKDF2 密码哈希 |
| `SecurityStamp` | `text` | NOT NULL | — | Identity 安全戳 |
| `ConcurrencyStamp` | `text` | NOT NULL | — | 乐观并发控制 |
| `Bio` | `varchar(500)` | NULL | — | 个人简介 |
| `AvatarPath` | `varchar(500)` | NULL | — | 头像文件路径 |
| `ReputationScore` | `real` | NOT NULL | `1.0` | 信誉分 (1.0 为基础值) |
| `Role` | `varchar(50)` | NOT NULL | `'Reader'` | 角色枚举 (Reader/Author/Admin) |
| `CreatedAt` | `timestamptz` | NOT NULL | `NOW()` | 注册时间 |
| `UpdatedAt` | `timestamptz` | NULL | — | 最后更新时间 |

**索引:**

| 索引名 | 列 | 类型 |
|--------|----|------|
| `PK_Users` | `Id` | PRIMARY KEY |
| `IX_Users_UserName` | `UserName` | UNIQUE |
| `IX_Users_Email` | `Email` | UNIQUE |
| `IX_Users_NormalizedUserName` | `NormalizedUserName` | UNIQUE |
| `IX_Users_NormalizedEmail` | `NormalizedEmail` | INDEX |

---

### 3.2 Papers

| 列名 | 类型 | 约束 | 默认值 | 说明 |
|------|------|------|--------|------|
| `Id` | `uuid` | PK | `gen_random_uuid()` | 论文唯一标识 |
| `Title` | `varchar(200)` | NOT NULL | — | 论文标题 |
| `Abstract` | `text` | NOT NULL | — | 论文摘要 |
| `FilePath` | `varchar(500)` | NOT NULL | — | PDF 文件相对路径 |
| `FileSize` | `bigint` | NOT NULL | — | 文件大小 (字节) |
| `AuthorId` | `uuid` | FK → Users, NOT NULL | — | 作者外键 |
| `Status` | `varchar(20)` | NOT NULL | `'Published'` | 状态: Published / Draft / Retracted |
| `AverageRating` | `real` | NOT NULL | `0` | 缓存: 加权平均分 |
| `RatingCount` | `integer` | NOT NULL | `0` | 缓存: 评分总数 |
| `ViewCount` | `integer` | NOT NULL | `0` | 浏览计数 (预留) |
| `PublishedAt` | `timestamptz` | NOT NULL | `NOW()` | 发布时间 |
| `UpdatedAt` | `timestamptz` | NULL | — | 最后更新时间 |

**索引:**

| 索引名 | 列 | 用途 |
|--------|----|------|
| `PK_Papers` | `Id` | PRIMARY KEY |
| `IX_Papers_AuthorId` | `AuthorId` | 按作者查询 |
| `IX_Papers_Status` | `Status` | 按状态过滤 |
| `IX_Papers_PublishedAt` | `PublishedAt DESC` | 按时间排序 (最新) |
| `IX_Papers_AverageRating` | `AverageRating DESC` | 按评分排序 (最热) |
| `IX_Papers_SearchVector` | `to_tsvector('english', Title || ' ' || Abstract)` (GIN) | 全文搜索 |

**全文搜索索引创建 SQL:**
```sql
CREATE INDEX "IX_Papers_SearchVector"
ON "Papers"
USING GIN (to_tsvector('english', "Title" || ' ' || "Abstract"));
```

---

### 3.3 Ratings

| 列名 | 类型 | 约束 | 默认值 | 说明 |
|------|------|------|--------|------|
| `Id` | `uuid` | PK | `gen_random_uuid()` | 评分唯一标识 |
| `PaperId` | `uuid` | FK → Papers, NOT NULL | — | 被评分论文 |
| `UserId` | `uuid` | FK → Users, NOT NULL | — | 评分用户 |
| `Score` | `smallint` | NOT NULL, CHECK (1 ≤ Score ≤ 5) | — | 评分 1~5 星 |
| `CreatedAt` | `timestamptz` | NOT NULL | `NOW()` | 评分时间 |
| `UpdatedAt` | `timestamptz` | NULL | — | 修改时间 |

**约束与索引:**

| 名称 | 列 | 类型 |
|------|----|------|
| `PK_Ratings` | `Id` | PRIMARY KEY |
| `UQ_Ratings_PaperId_UserId` | (`PaperId`, `UserId`) | UNIQUE — 每用户每论文仅一个评分 |
| `IX_Ratings_PaperId` | `PaperId` | INDEX — 查询论文的所有评分 |
| `IX_Ratings_UserId` | `UserId` | INDEX — 查询用户的评分记录 |

---

### 3.4 Comments

| 列名 | 类型 | 约束 | 默认值 | 说明 |
|------|------|------|--------|------|
| `Id` | `uuid` | PK | `gen_random_uuid()` | 评论唯一标识 |
| `PaperId` | `uuid` | FK → Papers, NOT NULL | — | 所属论文 |
| `UserId` | `uuid` | FK → Users, NOT NULL | — | 评论者 |
| `ParentId` | `uuid` | FK → Comments, NULL | NULL | 父评论 ID (NULL 表示顶级评论) |
| `Content` | `text` | NOT NULL | — | 评论内容 (≤ 5000 字符) |
| `IsDeleted` | `boolean` | NOT NULL | `false` | 软删除标记 |
| `CreatedAt` | `timestamptz` | NOT NULL | `NOW()` | 发表时间 |
| `UpdatedAt` | `timestamptz` | NULL | — | 编辑时间 |

**索引:**

| 索引名 | 列 | 用途 |
|--------|----|------|
| `PK_Comments` | `Id` | PRIMARY KEY |
| `IX_Comments_PaperId` | (`PaperId`, `CreatedAt` DESC) | 论文评论列表 (含排序) |
| `IX_Comments_UserId` | `UserId` | 用户评论记录 |
| `IX_Comments_ParentId` | `ParentId` | 查询子回复 |

---

### 3.5 Categories

| 列名 | 类型 | 约束 | 默认值 | 说明 |
|------|------|------|--------|------|
| `Id` | `uuid` | PK | `gen_random_uuid()` | 分类唯一标识 |
| `Name` | `varchar(100)` | NOT NULL, UNIQUE | — | 分类名称 (如 "计算机科学") |
| `Description` | `varchar(500)` | NULL | — | 分类描述 |
| `CreatedAt` | `timestamptz` | NOT NULL | `NOW()` | 创建时间 |

**索引:**

| 索引名 | 列 | 类型 |
|--------|----|------|
| `PK_Categories` | `Id` | PRIMARY KEY |
| `IX_Categories_Name` | `Name` | UNIQUE |

---

### 3.6 PaperCategories (关联表)

| 列名 | 类型 | 约束 | 说明 |
|------|------|------|------|
| `PaperId` | `uuid` | FK → Papers, PK | 论文 ID |
| `CategoryId` | `uuid` | FK → Categories, PK | 分类 ID |

**约束与索引:**

| 名称 | 列 | 类型 |
|------|----|------|
| `PK_PaperCategories` | (`PaperId`, `CategoryId`) | COMPOSITE PRIMARY KEY |
| `IX_PaperCategories_CategoryId` | `CategoryId` | INDEX — 按分类查论文 |

---

## 4. EF Core 实体关系映射

```csharp
// User → Papers (Author)
builder.Entity<User>()
    .HasMany(u => u.Papers)
    .WithOne(p => p.Author)
    .HasForeignKey(p => p.AuthorId)
    .OnDelete(DeleteBehavior.Restrict);

// User → Ratings
builder.Entity<User>()
    .HasMany(u => u.Ratings)
    .WithOne(r => r.User)
    .HasForeignKey(r => r.UserId)
    .OnDelete(DeleteBehavior.Cascade);

// User → Comments
builder.Entity<User>()
    .HasMany(u => u.Comments)
    .WithOne(c => c.User)
    .HasForeignKey(c => c.UserId)
    .OnDelete(DeleteBehavior.Cascade);

// Paper → Ratings
builder.Entity<Paper>()
    .HasMany(p => p.Ratings)
    .WithOne(r => r.Paper)
    .HasForeignKey(r => r.PaperId)
    .OnDelete(DeleteBehavior.Cascade);

// Paper → Comments
builder.Entity<Paper>()
    .HasMany(p => p.Comments)
    .WithOne(c => c.Paper)
    .HasForeignKey(c => c.PaperId)
    .OnDelete(DeleteBehavior.Cascade);

// Paper ←→ Category (多对多)
builder.Entity<Paper>()
    .HasMany(p => p.PaperCategories)
    .WithOne(pc => pc.Paper)
    .HasForeignKey(pc => pc.PaperId);

builder.Entity<Category>()
    .HasMany(c => c.PaperCategories)
    .WithOne(pc => pc.Category)
    .HasForeignKey(pc => pc.CategoryId);

// Comment → Comment (自引用回复)
builder.Entity<Comment>()
    .HasOne(c => c.Parent)
    .WithMany(c => c.Replies)
    .HasForeignKey(c => c.ParentId)
    .OnDelete(DeleteBehavior.Restrict);
```

---

## 5. 评分计算策略

### 5.1 简单模式 (初期默认)

不启用信誉权重，直接计算算术平均：

```
AverageRating = SUM(Score) / COUNT(*)
```

每次评分写入/更新后，在事务中重新计算并更新 `Papers` 表冗余字段:

```sql
UPDATE "Papers" p
SET
  "AverageRating" = COALESCE(s.avg_score, 0),
  "RatingCount"   = COALESCE(s.rating_count, 0)
FROM (
  SELECT
    "PaperId",
    AVG("Score"::float) AS avg_score,
    COUNT(*)             AS rating_count
  FROM "Ratings"
  WHERE "PaperId" = @PaperId
  GROUP BY "PaperId"
) s
WHERE p."Id" = s."PaperId";
```

### 5.2 加权模式 (信誉机制启用后)

```
WeightedAverage = SUM(Score_i * ReputationWeight_i) / SUM(ReputationWeight_i)

其中:
  ReputationWeight_i = User_i.ReputationScore
  或 ReputationWeight_i = LN(1 + User_i.ReputationScore)  (对数平滑)
```

```sql
UPDATE "Papers" p
SET
  "AverageRating" = COALESCE(s.weighted_avg, 0),
  "RatingCount"   = COALESCE(s.rating_count, 0)
FROM (
  SELECT
    r."PaperId",
    SUM(r."Score"::float * u."ReputationScore") / NULLIF(SUM(u."ReputationScore"), 0) AS weighted_avg,
    COUNT(*) AS rating_count
  FROM "Ratings" r
  JOIN "Users" u ON u."Id" = r."UserId"
  WHERE r."PaperId" = @PaperId
  GROUP BY r."PaperId"
) s
WHERE p."Id" = s."PaperId";
```

### 5.3 更新触发点

评分变更时统一调用 `RatingService` 的以下流程 (事务包裹):

1. 写入/更新 `Ratings` 表
2. 聚合计算最新平均分
3. 更新 `Papers.AverageRating` 和 `Papers.RatingCount`
4. 提交事务

---

## 6. 全文搜索

### 6.1 索引

```sql
CREATE INDEX "IX_Papers_SearchVector"
ON "Papers"
USING GIN (to_tsvector('english', "Title" || ' ' || "Abstract"));
```

### 6.2 查询示例

```sql
SELECT "Id", "Title", "Abstract",
       ts_rank(
         to_tsvector('english', "Title" || ' ' || "Abstract"),
         plainto_tsquery('english', 'machine learning')
       ) AS rank
FROM "Papers"
WHERE to_tsvector('english', "Title" || ' ' || "Abstract")
      @@ plainto_tsquery('english', 'machine learning')
  AND "Status" = 'Published'
ORDER BY rank DESC;
```

### 6.3 中文支持

中文全文搜索需要额外扩展:
- **pg_jieba**: 结巴分词 PostgreSQL 扩展
- **zhparser**: 基于 SCWS 的中文分词

安装扩展后，将 `'english'` 替换为 `'jieba'` 或 `'zhparser'` 即可。

---

## 7. 迁移与版本管理

- 使用 EF Core CLI 或 Package Manager Console 生成迁移:

```bash
# 创建迁移
dotnet ef migrations add <MigrationName> \
  --project src/OpenPeer.Infrastructure \
  --startup-project src/OpenPeer.Api

# 更新数据库
dotnet ef database update \
  --project src/OpenPeer.Infrastructure \
  --startup-project src/OpenPeer.Api

# 生成 SQL 脚本 (生产环境)
dotnet ef migrations script \
  --project src/OpenPeer.Infrastructure \
  --startup-project src/OpenPeer.Api \
  --output migrate.sql
```

- Docker Compose 启动时, API 容器可配置 `ENTRYPOINT` 自动执行 `database update`
