# OpenPeer — Agent Instructions

## Project status

**M6 completed — supporting data upload/management, AI LaTeX generation.** M4 (categories CRUD, profile center, admin panel, avatar upload) and M3 (rating + comment system) finished earlier. Registration/login/JWT + paper upload/list/detail/search/delete + rating + comment all working. M6 adds: `SupportingData` entity/table, multi-file upload (≤20MB per file, images/docs/csv/json/zip), `UserAiConfig` entity/table with AES-256 encrypted API key storage, AI config API (GET/PUT `/api/users/me/ai-config`), LaTeX generation via `POST /api/papers/generate` (calls OpenAI/DeepSeek/Anthropic compatible APIs), `AiConfigView` frontend page, supporting data section in `PaperDetailView`. Rate limiting, response compression, health checks, EF Core retry configured. Unit tests for `RatingService` (5 tests running via `dotnet test`).

## Project identity

OpenPeer is a reader-evaluation-driven academic paper platform. Authors publish papers directly; registered readers rate them 1–5 stars and comment. Final paper scores are weighted-average reader ratings (no traditional peer review). Full docs in `doc/`.

## Tech stack (fixed, non-negotiable)

| Layer | Choice |
|-------|--------|
| Backend | ASP.NET Core 10 Web API (C# 13) |
| Frontend | Vue 3 + Vite + TypeScript |
| Database | PostgreSQL 16 |
| ORM | EF Core 10 + Npgsql |
| Auth | JWT + ASP.NET Core Identity |
| Validation | FluentValidation |
| UI | Element Plus |
| State | Pinia |
| HTTP | Axios |
| Mapping | Mapster |
| Logging | Serilog |

## Architecture

**Backend layers** (Clean Architecture — dependencies flow inward):
```
Api → Application → Domain ← Infrastructure
```
- `Domain` has zero external dependencies
- `Infrastructure` implements `Application` interfaces
- `Api` is the composition root (Program.cs, DI, Controllers)

**Frontend** (`src/OpenPeer.Web/`): standard Vue 3 SPA with `<script setup lang="ts">`, Pinia stores in `stores/`, typed API clients in `api/`.

## Key commands

```bash
# Backend (run from Api project)
dotnet restore
dotnet build
dotnet test                        # runs src/OpenPeer.Tests
dotnet ef migrations add <Name> --project src/OpenPeer.Infrastructure --startup-project src/OpenPeer.Api
dotnet ef database update    --project src/OpenPeer.Infrastructure --startup-project src/OpenPeer.Api

# Frontend (run from src/OpenPeer.Web)
npm install
npm run dev         # Vite dev server (port 5173)
npm run build       # production build

# Docker (root)
docker compose up -d --build   # start all services (db, api, web)
docker compose down -v         # teardown with volumes
```

Service ports: API `:5000`, Web `:80`, DB `:5432` (local dev) / `:5433` (Docker), API docs `http://localhost:5000/scalar/v1`, Health `http://localhost:5000/health`.

## Important conventions

- **API responses**: every endpoint returns `{ code, message, data }` — never raw data
- **C# naming**: PascalCase public, `_camelCase` private fields, camelCase locals/params
- **Vue naming**: PascalCase filenames for components, Composition API only, `<script setup lang="ts">`
- **Primary keys**: UUID (not auto-increment ints) — uses PostgreSQL `gen_random_uuid()`
- **Soft deletes**: papers and comments use soft-delete (`IsDeleted` flag), ratings are physically deleted
- **Rating**: one rating per user per paper (unique constraint), score recalculation in transaction after every write. `CreateRatingRequest` has a single `score` field (1–5)
- **Avatar upload**: `POST /api/users/me/avatar` (multipart), stored at `Uploads/Avatars/{userId}.ext`, served at `GET /api/files/avatars/{fileName}`
- **User role**: stored in `User.Role` enum column, NOT in Identity's `AspNetUserRoles` table. `UserService.UpdateUserRoleAsync` updates `User.Role` directly via `_userManager.UpdateAsync()` — do NOT use `AddToRoleAsync`/`RemoveFromRolesAsync`
- **Rate limiting**: Login (5/min), Register (3/min), Upload (10/min), Default (100/min) via `[EnableRateLimiting("...")]`
- **File uploads**: paper PDF ≤ 10 MB via `IFileStorageService` (stored at `Uploads/Papers/`); avatar images ≤ 2 MB (stored at `Uploads/Avatars/`)
- **nginx**: `client_max_body_size 25M` in the `/api/` location block is required. API controllers use `[DisableRequestSizeLimit]` — all upload size enforcement happens at nginx level, not in ASP.NET Core
- **Git commits**: format `<type>(<scope>): <subject>` — types: feat, fix, docs, refactor, test, chore

## Repository structure
```
├── doc/                    # architecture, requirements, db-design, api-design
├── src/
│   ├── OpenPeer.Api/
│   ├── OpenPeer.Application/
│   ├── OpenPeer.Domain/
│   ├── OpenPeer.Infrastructure/
│   ├── OpenPeer.Tests/          # xUnit unit tests (Moq + FluentAssertions)
│   └── OpenPeer.Web/            # Vue 3 frontend
├── docker-compose.yml
└── AGENTS.md
```

## Session discipline

Every development session must end with a documentation review:

- **Update AGENTS.md** — sync project status, add newly discovered conventions or traps, remove stale claims. Every line should answer: "would a future agent make a mistake without reading this?" — if not, delete it.
- **Update README.md** — keep it in sync with the actual project state: milestones, tech stack versions, port numbers, quick-start steps. It is the public face of the project.
- **Update doc/ files** — if code changes deviate from design docs (`architecture.md`, `api-design.md`, `database-design.md`, `requirements.md`), update them. The `doc/` directory must remain the trusted single source of truth.

## Traps to avoid

- Don't add NuGet packages beyond what's listed here without discussion
- Don't mix architectures — this is Clean Architecture, not N-tier or vertical slices
- Don't expose domain entities in API responses — always map to DTOs
- Don't store raw SQL — use EF Core parameterized queries (full-text search is the only exception via `tsvector`)
- Don't use cookie auth — this is a stateless JWT API
- Don't make `ReputationScore` a required feature — it's a P2 reserved field, may not be implemented yet
- Don't install Vue 2 plugins or Options API code — Vue 3 + Composition API strictly
- Don't use Identity role APIs (`GetRolesAsync`/`AddToRoleAsync`) — the app stores roles in `User.Role` column directly
- Don't rely on `Login` response's `user.paperCount` — it reflects the value at registration time; use `GET /api/users/me` for accurate counts
- Don't call `fetchPapers()`/`fetchRatings()`/`fetchComments()` on ProfileView mount — use lazy loading via `watch(activeTab)` with `loadedTabs` dedup set
- Don't enable `AddRoles<IdentityRole>()` — this project uses `User.Role` column, not the Identity roles table
