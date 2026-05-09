# OpenPeer — Agent Instructions

## Project status

**Pre-implementation (M0).** Source code is not yet scaffolded. The `doc/` directory is the source of truth: `requirements.md`, `architecture.md`, `database-design.md`, `api-design.md`. First milestone (M1): scaffold backend/frontend projects, Docker Compose, database schema, and auth system per those docs.

## Project identity

OpenPeer is a reader-evaluation-driven academic paper platform. Authors publish papers directly; registered readers rate them 1–5 stars and comment. Final paper scores are weighted-average reader ratings (no traditional peer review). Full docs in `doc/`.

## Tech stack (fixed, non-negotiable)

| Layer | Choice |
|-------|--------|
| Backend | ASP.NET Core 8 Web API (C# 12) |
| Frontend | Vue 3 + Vite + TypeScript |
| Database | PostgreSQL 16 |
| ORM | EF Core 8 + Npgsql |
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

> All commands require the project to be scaffolded first — no `.sln`, `.csproj`, or `package.json` exist yet.

```bash
# Backend (run from solution root or Api project)
dotnet restore
dotnet build
dotnet test
dotnet ef migrations add <Name> --project src/OpenPeer.Infrastructure --startup-project src/OpenPeer.Api
dotnet ef database update    --project src/OpenPeer.Infrastructure --startup-project src/OpenPeer.Api

# Frontend (run from src/OpenPeer.Web)
npm install
npm run dev         # Vite dev server
npm run build       # production build
npm run typecheck   # if configured
npm run test        # Vitest

# Docker (root)
docker compose up -d --build   # start all services (db, api, web)
docker compose down -v         # teardown with volumes
```

Service ports: API `:5000`, Web `:80`, DB `:5432`, Swagger `http://localhost:5000/swagger`.

## Important conventions

- **API responses**: every endpoint returns `{ code, message, data }` — never raw data
- **C# naming**: PascalCase public, `_camelCase` private fields, camelCase locals/params
- **Vue naming**: PascalCase filenames for components, Composition API only, `<script setup lang="ts">`
- **Primary keys**: UUID (not auto-increment ints) — uses PostgreSQL `gen_random_uuid()`
- **Soft deletes**: papers and comments use soft-delete (`IsDeleted` flag), not physical delete
- **Rating**: one rating per user per paper (unique constraint), score recalculation in transaction after every write
- **File uploads**: PDF only, max 10 MB, stored locally (`Uploads/`) via `IFileStorageService`
- **Git commits**: format `<type>(<scope>): <subject>` — types: feat, fix, docs, refactor, test, chore

## Repository structure
```
├── doc/                    # architecture, requirements, db-design, api-design
├── src/
│   ├── OpenPeer.Api/
│   ├── OpenPeer.Application/
│   ├── OpenPeer.Domain/
│   ├── OpenPeer.Infrastructure/
│   └── OpenPeer.Web/           # Vue 3 frontend
├── docker-compose.yml
└── AGENTS.md
```

## Traps to avoid

- Don't add NuGet packages beyond what's listed here without discussion
- Don't mix architectures — this is Clean Architecture, not N-tier or vertical slices
- Don't expose domain entities in API responses — always map to DTOs
- Don't store raw SQL — use EF Core parameterized queries (full-text search is the only exception via `tsvector`)
- Don't use cookie auth — this is a stateless JWT API
- Don't make `ReputationScore` a required feature — it's a P2 reserved field, may not be implemented yet
- Don't install Vue 2 plugins or Options API code — Vue 3 + Composition API strictly
