---
description: Scaffold entire OpenPeer project — .NET solution, Clean Architecture projects, Vue 3 frontend, Docker Compose
agent: dotnet-dev
subtask: true
---

Scaffold the complete OpenPeer project from scratch following the specifications in `doc/architecture.md`.

## Tasks to complete

### 1. Backend scaffold
Use the `dotnet-scaffold` skill to create:
- `OpenPeer.sln` solution file
- 4 projects: `OpenPeer.Domain`, `OpenPeer.Application`, `OpenPeer.Infrastructure`, `OpenPeer.Api`
- Proper project references (Clean Architecture dependency flow)
- All required NuGet packages
- Remove auto-generated template files

### 2. Frontend scaffold
Use the `vue3-scaffold` skill to create `src/OpenPeer.Web/`:
- Vue 3 + Vite + TypeScript project
- Install all dependencies (vue-router, pinia, axios, element-plus, sass)
- Create directory structure per `doc/architecture.md §4.1`
- Configure vite.config.ts with proxy and path alias

### 3. Docker scaffold
Use the `docker-setup` skill to create:
- `docker-compose.yml` in project root
- `Dockerfile` for API project
- `Dockerfile` + `nginx.conf` for Web project
- `.env.example` with placeholder values
- `.gitignore` covering .NET, Node, and Docker artifacts

### 4. Verify
- Run `dotnet build` from solution root
- Run `npm install` from `src/OpenPeer.Web/`
- Confirm no build errors before reporting completion
