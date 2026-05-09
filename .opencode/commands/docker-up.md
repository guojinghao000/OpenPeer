---
description: Start all Docker services (PostgreSQL, API, Web)
agent: dotnet-dev
---

Start the OpenPeer Docker services from project root.

1. Check if `docker-compose.yml` exists. If not, scaffold it using the `docker-setup` skill first.
2. Check if `.env` exists. If not, create it from `.env.example` or scaffold one:
   - `DB_PASSWORD=devpassword`
   - `JWT_SECRET=change-me-in-production-min-256bit-secret-key`
3. Run `docker compose up -d --build`
4. Wait for health checks and confirm:
   - PostgreSQL on `localhost:5432`
   - API on `http://localhost:5000`
   - Swagger on `http://localhost:5000/swagger`
   - Web on `http://localhost:80`
5. Report service status
