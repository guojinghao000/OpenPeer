# OpenPeer — API 设计文档

## 1. 约定

### 1.1 基础 URL

| 环境 | 地址 |
|------|------|
| 开发 | `http://localhost:5000/api` |
| 生产 | `https://<domain>/api` |

### 1.2 统一响应格式

**成功:**

```json
{
  "code": 200,
  "message": "操作成功",
  "data": { ... }
}
```

**失败:**

```json
{
  "code": 400,
  "message": "请求参数有误",
  "errors": [
    { "field": "Title", "message": "标题不能为空" }
  ]
}
```

### 1.3 HTTP 状态码

| 码 | 含义 | 触发场景 |
|----|------|----------|
| `200` | OK | GET, PUT 成功 |
| `201` | Created | POST 创建资源成功 |
| `204` | No Content | DELETE 成功 |
| `400` | Bad Request | 参数校验失败 |
| `401` | Unauthorized | 未携带 Token 或 Token 过期 |
| `403` | Forbidden | 无权操作该资源 |
| `404` | Not Found | 资源不存在 |
| `409` | Conflict | 资源冲突 (如重复评分) |
| `413` | Payload Too Large | 上传文件超限 |
| `429` | Too Many Requests | 触发限流 |
| `500` | Internal Server Error | 未处理异常 |

### 1.4 认证

需认证的请求携带 Header:

```
Authorization: Bearer <access_token>
```

### 1.5 分页

**请求:**

```
GET /api/papers?page=1&pageSize=20&sortBy=rating&order=desc
```

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `page` | int | 1 | 页码 (从 1 开始) |
| `pageSize` | int | 20 | 每页数量 (最大 50) |
| `sortBy` | string | "publishedAt" | 排序字段 |
| `order` | string | "desc" | asc / desc |

**响应:**

```json
{
  "code": 200,
  "message": "操作成功",
  "data": {
    "items": [ ... ],
    "page": 1,
    "pageSize": 20,
    "total": 156,
    "totalPages": 8
  }
}
```

---

## 2. 认证模块 `POST /api/auth`

### 2.1 注册

```
POST /api/auth/register
```

**请求:**

```json
{
  "userName": "alice",
  "email": "alice@example.com",
  "password": "Secure1234",
  "confirmPassword": "Secure1234"
}
```

| 字段 | 约束 |
|------|------|
| `userName` | 必填, 3~20 字符, 字母/数字/下划线 |
| `email` | 必填, 合法邮箱格式 |
| `password` | 必填, 8~100 字符, 必须含字母和数字 |
| `confirmPassword` | 必填, 与 password 一致 |

**成功 (201):**

```json
{
  "code": 201,
  "message": "注册成功",
  "data": {
    "userId": "a1b2c3d4-...",
    "userName": "alice",
    "email": "alice@example.com"
  }
}
```

**可能错误:** 400 (校验失败), 409 (用户名或邮箱已存在)

---

### 2.2 登录

```
POST /api/auth/login
```

**请求:**

```json
{
  "email": "alice@example.com",
  "password": "Secure1234"
}
```

**成功 (200):**

```json
{
  "code": 200,
  "message": "登录成功",
  "data": {
    "accessToken": "eyJhbGciOi...",
    "refreshToken": "dGhpcyBpcyBh...",
    "expiresIn": 3600,
    "user": {
      "id": "a1b2c3d4-...",
      "userName": "alice",
      "email": "alice@example.com",
      "avatarPath": null,
      "role": "Reader"
    }
  }
}
```

**可能错误:** 400 (参数缺失), 401 (邮箱或密码错误)

---

### 2.3 刷新 Token

```
POST /api/auth/refresh
```

**请求:**

```json
{
  "refreshToken": "dGhpcyBpcyBh..."
}
```

**成功 (200):**

```json
{
  "code": 200,
  "message": "Token 刷新成功",
  "data": {
    "accessToken": "eyJhbGciOi...",
    "refreshToken": "bmV3IHJlZnJl...",
    "expiresIn": 3600
  }
}
```

**可能错误:** 401 (Refresh Token 无效或过期)

---

### 2.4 登出

```
POST /api/auth/logout
Authorization: Bearer <access_token>
```

**请求:**

```json
{
  "refreshToken": "dGhpcyBpcyBh..."
}
```

**成功 (204):** 无响应体

---

## 3. 用户模块 `/api/users`

### 3.1 获取当前用户信息

```
GET /api/users/me
Authorization: Bearer <access_token>
```

**成功 (200):**

```json
{
  "code": 200,
  "message": "操作成功",
  "data": {
    "id": "a1b2c3d4-...",
    "userName": "alice",
    "email": "alice@example.com",
    "bio": "计算机科学研究者",
    "avatarPath": "/uploads/avatars/abc.jpg",
    "reputationScore": 1.0,
    "role": "Reader",
    "paperCount": 3,
    "ratingCount": 12,
    "commentCount": 8,
    "createdAt": "2026-01-15T08:30:00Z"
  }
}
```

---

### 3.2 更新当前用户信息

```
PUT /api/users/me
Authorization: Bearer <access_token>
```

**请求:**

```json
{
  "bio": "更新后的个人简介"
}
```

> 头像上传建议使用独立端点 (multipart/form-data)。

---

### 3.3 获取用户公开信息

```
GET /api/users/{id}
```

**说明:** 无需认证, 返回用户公开信息和论文列表

---

### 3.4 修改密码

```
POST /api/users/me/change-password
Authorization: Bearer <access_token>
```

**请求:**

```json
{
  "currentPassword": "OldPass1234",
  "newPassword": "NewPass5678",
  "confirmNewPassword": "NewPass5678"
}
```

---

## 4. 论文模块 `/api/papers`

### 4.1 获取论文列表

```
GET /api/papers?page=1&pageSize=20&sortBy=publishedAt&order=desc&categoryId=xxx&keyword=ml
```

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `page` | int | 1 | 页码 |
| `pageSize` | int | 20 | 每页条数 (max 50) |
| `sortBy` | string | `publishedAt` | `publishedAt` / `averageRating` / `commentCount` |
| `order` | string | `desc` | `asc` / `desc` |
| `categoryId` | uuid? | null | 按分类筛选 |
| `keyword` | string? | null | 全文搜索关键词 |
| `status` | string? | `Published` | 论文状态筛选 (仅管理员可查非 Published) |

**成功 (200):**

```json
{
  "code": 200,
  "message": "操作成功",
  "data": {
    "items": [
      {
        "id": "p1-uuid-...",
        "title": "基于 Transformer 的时序预测研究",
        "abstract": "本文提出了一种...",
        "author": {
          "id": "u1-uuid-...",
          "userName": "alice"
        },
        "categories": [
          { "id": "c1-...", "name": "人工智能" }
        ],
        "averageRating": 4.2,
        "ratingCount": 23,
        "commentCount": 8,
        "status": "Published",
        "publishedAt": "2026-04-01T10:00:00Z"
      }
    ],
    "page": 1,
    "pageSize": 20,
    "total": 156,
    "totalPages": 8
  }
}
```

---

### 4.2 获取论文详情

```
GET /api/papers/{id}
```

**成功 (200):**

```json
{
  "code": 200,
  "message": "操作成功",
  "data": {
    "id": "p1-uuid-...",
    "title": "基于 Transformer 的时序预测研究",
    "abstract": "本文提出了一种...全文摘要...",
    "fileUrl": "/api/files/papers/p1-uuid.pdf",
    "fileSize": 2048576,
    "author": {
      "id": "u1-uuid-...",
      "userName": "alice",
      "avatarPath": null
    },
    "categories": [
      { "id": "c1-...", "name": "人工智能" },
      { "id": "c2-...", "name": "数据科学" }
    ],
    "averageRating": 4.2,
    "ratingCount": 23,
    "ratingDistribution": {
      "star1": 1,
      "star2": 2,
      "star3": 3,
      "star4": 8,
      "star5": 9
    },
    "currentUserRating": 4,
    "commentCount": 8,
    "viewCount": 356,
    "status": "Published",
    "publishedAt": "2026-04-01T10:00:00Z",
    "updatedAt": null
  }
}
```

**说明:**
- `currentUserRating` 为当前登录用户的评分，未评分时返回 `null`
- `ratingDistribution` 统计各星级评分数

---

### 4.3 上传论文

```
POST /api/papers
Authorization: Bearer <access_token>
Content-Type: multipart/form-data
```

**请求 (multipart/form-data):**

| 字段 | 类型 | 约束 |
|------|------|------|
| `title` | string | 必填, 5~200 字符 |
| `abstract` | string | 必填, 20~2000 字符 |
| `categoryIds` | string[] | 可选, UUID 数组 |
| `file` | file | 必填, PDF 格式, ≤ 10MB |

**成功 (201):**

```json
{
  "code": 201,
  "message": "论文发布成功",
  "data": {
    "id": "p1-uuid-...",
    "title": "基于 Transformer 的时序预测研究",
    "abstract": "本文提出了一种...",
    "fileUrl": "/api/files/papers/p1-uuid.pdf",
    "status": "Published",
    "publishedAt": "2026-04-01T10:00:00Z"
  }
}
```

**可能错误:** 400 (校验失败), 401, 413 (文件过大), 429 (上传频率过高)

---

### 4.4 更新论文元数据

```
PUT /api/papers/{id}
Authorization: Bearer <access_token>
```

**请求:**

```json
{
  "title": "更新后的标题",
  "abstract": "更新后的摘要",
  "categoryIds": ["c1-...", "c3-..."]
}
```

> 不可更换已上传的 PDF 文件。

---

### 4.5 删除论文

```
DELETE /api/papers/{id}
Authorization: Bearer <access_token>
```

**成功 (204):** 无响应体

**约束:** 仅作者本人可删除, 软删除 (不物理删除文件和记录)

---

### 4.6 撤回论文

```
POST /api/papers/{id}/retract
Authorization: Bearer <access_token>
```

**请求:**

```json
{
  "reason": "发现数据计算错误，正在修正"
}
```

---

### 4.7 下载/预览论文文件

```
GET /api/files/papers/{fileName}
```

返回 PDF 文件流 (`Content-Type: application/pdf`)。

---

## 5. 评分模块 `/api/papers/{paperId}/ratings`

### 5.1 提交或更新评分

```
POST /api/papers/{paperId}/ratings
Authorization: Bearer <access_token>
```

**请求:**

```json
{
  "score": 4
}
```

| 字段 | 约束 |
|------|------|
| `score` | 必填, 整数 1~5 |

**成功 (200/201):** 返回更新后的评分信息

**说明:** 若已有评分则更新 (幂等)，无则创建。每次评分后自动重算论文均分。

### 5.2 获取论文评分列表

```
GET /api/papers/{paperId}/ratings?page=1&pageSize=20
```

**成功 (200):**

```json
{
  "code": 200,
  "message": "操作成功",
  "data": {
    "items": [
      {
        "id": "r1-uuid-...",
        "user": {
          "id": "u1-...",
          "userName": "bob",
          "avatarPath": null
        },
        "score": 5,
        "createdAt": "2026-04-02T12:00:00Z"
      }
    ],
    "page": 1,
    "pageSize": 20,
    "total": 23,
    "totalPages": 2,
    "distribution": {
      "star1": 1,
      "star2": 2,
      "star3": 3,
      "star4": 8,
      "star5": 9
    }
  }
}
```

### 5.3 删除自己的评分

```
DELETE /api/papers/{paperId}/ratings
Authorization: Bearer <access_token>
```

**成功 (204):** 评分删除后自动重算论文均分

---

## 6. 评论模块

### 6.1 获取论文评论列表

```
GET /api/papers/{paperId}/comments?page=1&pageSize=20
```

**成功 (200):**

```json
{
  "code": 200,
  "message": "操作成功",
  "data": {
    "items": [
      {
        "id": "cm1-uuid-...",
        "user": {
          "id": "u2-...",
          "userName": "bob",
          "avatarPath": null
        },
        "content": "非常有价值的论文...",
        "parentId": null,
        "replies": [
          {
            "id": "cm2-uuid-...",
            "user": {
              "id": "u1-...",
              "userName": "alice"
            },
            "content": "感谢您的反馈！",
            "parentId": "cm1-uuid-...",
            "replies": [],
            "createdAt": "2026-04-03T08:30:00Z",
            "updatedAt": null
          }
        ],
        "createdAt": "2026-04-02T18:00:00Z",
        "updatedAt": null
      }
    ],
    "page": 1,
    "pageSize": 20,
    "total": 8,
    "totalPages": 1
  }
}
```

**说明:** 仅嵌套一级回复 (`replies` 数组), 更深层不再展开。

---

### 6.2 发表评论

```
POST /api/papers/{paperId}/comments
Authorization: Bearer <access_token>
```

**请求:**

```json
{
  "content": "非常有价值的论文，特别是第三章的实验设计很严谨。",
  "parentId": null
}
```

| 字段 | 约束 |
|------|------|
| `content` | 必填, 1~5000 字符 |
| `parentId` | 可选, UUID 或 null (null = 顶级评论) |

**成功 (201):**

```json
{
  "code": 201,
  "message": "评论发表成功",
  "data": {
    "id": "cm1-uuid-...",
    "content": "非常有价值的论文...",
    "parentId": null,
    "createdAt": "2026-04-02T18:00:00Z"
  }
}
```

---

### 6.3 编辑评论

```
PUT /api/comments/{id}
Authorization: Bearer <access_token>
```

**请求:**

```json
{
  "content": "修改后的评论内容"
}
```

**约束:** 仅评论作者本人可编辑。

---

### 6.4 删除评论

```
DELETE /api/comments/{id}
Authorization: Bearer <access_token>
```

**成功 (204):** 无响应体

**约束:** 仅评论作者或论文作者可删除, 软删除。

---

## 7. 分类模块 `/api/categories`

### 7.1 获取分类列表

```
GET /api/categories
```

**成功 (200):**

```json
{
  "code": 200,
  "message": "操作成功",
  "data": [
    {
      "id": "c1-uuid-...",
      "name": "人工智能",
      "description": "机器学习、深度学习、NLP 等相关研究",
      "paperCount": 42,
      "createdAt": "2026-01-01T00:00:00Z"
    }
  ]
}
```

---

### 7.2 创建分类 (管理员)

```
POST /api/categories
Authorization: Bearer <access_token> [Admin]
```

**请求:**

```json
{
  "name": "量子计算",
  "description": "量子计算相关研究"
}
```

---

### 7.3 更新分类 (管理员)

```
PUT /api/categories/{id}
Authorization: Bearer <access_token> [Admin]
```

### 7.4 删除分类 (管理员)

```
DELETE /api/categories/{id}
Authorization: Bearer <access_token> [Admin]
```

**约束:** 仅可删除无关联论文的分类。

---

## 8. 端点汇总

| 方法 | 路径 | 认证 | 说明 |
|------|------|------|------|
| `POST` | `/api/auth/register` | 否 | 用户注册 |
| `POST` | `/api/auth/login` | 否 | 用户登录 |
| `POST` | `/api/auth/refresh` | 否 | 刷新 Token |
| `POST` | `/api/auth/logout` | 是 | 登出 |
| `GET` | `/api/users/me` | 是 | 当前用户信息 |
| `PUT` | `/api/users/me` | 是 | 更新个人信息 |
| `POST` | `/api/users/me/change-password` | 是 | 修改密码 |
| `GET` | `/api/users/{id}` | 否 | 用户公开信息 |
| `GET` | `/api/papers` | 否 | 论文列表 (分页/搜索/排序) |
| `GET` | `/api/papers/{id}` | 否 | 论文详情 |
| `POST` | `/api/papers` | 是 | 上传论文 (multipart) |
| `PUT` | `/api/papers/{id}` | 是 | 编辑论文元数据 |
| `DELETE` | `/api/papers/{id}` | 是 | 删除论文 |
| `POST` | `/api/papers/{id}/retract` | 是 | 撤回论文 |
| `GET` | `/api/files/papers/{fileName}` | 否 | 下载 PDF |
| `POST` | `/api/papers/{paperId}/ratings` | 是 | 提交/更新评分 |
| `GET` | `/api/papers/{paperId}/ratings` | 否 | 评分列表 |
| `DELETE` | `/api/papers/{paperId}/ratings` | 是 | 删除评分 |
| `GET` | `/api/papers/{paperId}/comments` | 否 | 评论列表 |
| `POST` | `/api/papers/{paperId}/comments` | 是 | 发表评论 |
| `PUT` | `/api/comments/{id}` | 是 | 编辑评论 |
| `DELETE` | `/api/comments/{id}` | 是 | 删除评论 |
| `GET` | `/api/categories` | 否 | 分类列表 |
| `POST` | `/api/categories` | 是 (Admin) | 创建分类 |
| `PUT` | `/api/categories/{id}` | 是 (Admin) | 更新分类 |
| `DELETE` | `/api/categories/{id}` | 是 (Admin) | 删除分类 |
| `POST` | `/api/papers/{paperId}/data` | 是 | 上传支撑数据 |
| `GET` | `/api/papers/{paperId}/data` | 否 | 支撑数据列表 |
| `DELETE` | `/api/papers/{paperId}/data/{id}` | 是 | 删除支撑数据 |
| `GET` | `/api/files/data/{fileName}` | 否 | 下载数据文件 |
| `POST` | `/api/papers/generate` | 是 | 生成 LaTeX 论文 |
| `GET` | `/api/users/me/ai-config` | 是 | 查看 AI 配置 |
| `PUT` | `/api/users/me/ai-config` | 是 | 更新 AI 配置 |

---

## 9. 错误码

| code | 说明 |
|------|------|
| `400` | 请求参数校验失败 |
| `401` | 认证失败 (Token 缺失/无效/过期) |
| `403` | 无权限操作 |
| `404` | 资源不存在 |
| `409` | 资源冲突 |
| `429` | 请求频率超限 |
| `500` | 服务器内部错误 |

错误响应始终包含 `message` 字段，400 错误额外包含 `errors` 数组。

---

## 10. 科研数据模块 `/api/papers/{paperId}/data`

### 10.1 上传支撑数据

```
POST /api/papers/{paperId}/data
Authorization: Bearer <access_token>
Content-Type: multipart/form-data
```

**请求 (multipart/form-data):**

| 字段 | 类型 | 约束 |
|------|------|------|
| `file` | file | 必填, 图片(jpg/png/webp) / 文档(pdf/doc/docx) / 表格(csv/xlsx) / 数据(json/zip), ≤ 20MB |
| `description` | string | 可选, 文件说明 |

**成功 (201):**

```json
{
  "code": 201,
  "message": "支撑数据上传成功",
  "data": {
    "id": "sd-uuid-...",
    "fileName": "experiment-results.csv",
    "fileType": "text/csv",
    "fileSize": 1024000,
    "description": "实验原始数据",
    "createdAt": "2026-05-10T10:00:00Z"
  }
}
```

### 10.2 获取支撑数据列表

```
GET /api/papers/{paperId}/data
```

**成功 (200):**

```json
{
  "code": 200,
  "message": "操作成功",
  "data": [
    {
      "id": "sd-uuid-...",
      "fileName": "experiment-results.csv",
      "fileType": "text/csv",
      "fileSize": 1024000,
      "description": "实验原始数据",
      "userName": "alice",
      "createdAt": "2026-05-10T10:00:00Z"
    }
  ]
}
```

### 10.3 删除支撑数据

```
DELETE /api/papers/{paperId}/data/{id}
Authorization: Bearer <access_token>
```

**成功 (204):** 无响应体

**约束:** 仅上传者或论文作者可删除。

### 10.4 下载支撑数据文件

```
GET /api/files/data/{fileName}
```

返回对应 MIME 类型的文件流。

---

## 11. AI 论文生成 `/api/papers`

### 11.1 生成 LaTeX 论文

```
POST /api/papers/generate
Authorization: Bearer <access_token>
```

**请求:**

```json
{
  "title": "基于实验数据的时序预测研究",
  "dataIds": ["sd-uuid-1", "sd-uuid-2"],
  "prompt": "请根据提供的实验数据，撰写一篇学术论文，重点分析方法的有效性。"
}
```

| 字段 | 约束 |
|------|------|
| `title` | 必填, 5~200 字符 |
| `dataIds` | 必填, 至少选择 1 个文件 |
| `prompt` | 必填, 1~2000 字符 |

**成功 (200):**

```json
{
  "code": 200,
  "message": "LaTeX 论文生成成功",
  "data": {
    "latex": "\\documentclass[12pt,a4paper]{article}\n\\usepackage[utf8]{inputenc}\n..."
  }
}
```

**可能错误:** 400 (未配置 AI API, 参数校验失败), 402 (AI API 调用失败), 413 (数据总量过大)

### 11.2 获取 LaTeX 源码 (未实现)

设计预留端点 `GET /api/papers/{id}/latex` 用于获取已保存到论文的 LaTeX 源码。当前 LaTeX 源码直接通过生成响应返回，不持久化保存。

---

## 12. AI 配置模块 `/api/users/me/ai-config`

### 12.1 查看 AI 配置

```
GET /api/users/me/ai-config
Authorization: Bearer <access_token>
```

**成功 (200):**

```json
{
  "code": 200,
  "message": "操作成功",
  "data": {
    "provider": "openai",
    "model": "gpt-4o",
    "hasApiKey": true
  }
}
```

> `hasApiKey` 为 `true` 表示已设置 API Key（不返回明文密钥）。

### 12.2 更新 AI 配置

```
PUT /api/users/me/ai-config
Authorization: Bearer <access_token>
```

**请求:**

```json
{
  "provider": "openai",
  "apiKey": "sk-xxx...",
  "model": "gpt-4o"
}
```

| 字段 | 约束 |
|------|------|
| `provider` | 必填, openai / deepseek / anthropic / custom |
| `apiKey` | 必填, 加密存储 |
| `model` | 必填, gpt-4o / deepseek-chat 等 |

**成功 (200):**

```json
{
  "code": 200,
  "message": "AI 配置已更新"
}
```

### 12.3 端点汇总

| 方法 | 路径 | 认证 | 说明 |
|------|------|------|------|
| `POST` | `/api/papers/{paperId}/data` | 是 | 上传支撑数据 |
| `GET` | `/api/papers/{paperId}/data` | 否 | 支撑数据列表 |
| `DELETE` | `/api/papers/{paperId}/data/{id}` | 是 | 删除支撑数据 |
| `GET` | `/api/files/data/{fileName}` | 否 | 下载数据文件 |
| `POST` | `/api/papers/generate` | 是 | 生成 LaTeX 论文 |
| `GET` | `/api/users/me/ai-config` | 是 | 查看 AI 配置 |
| `PUT` | `/api/users/me/ai-config` | 是 | 更新 AI 配置 |
