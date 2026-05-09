---
name: docker-setup
description: Set up Docker Compose for OpenPeer — PostgreSQL 16, ASP.NET Core API, Nginx-hosted Vue SPA
---

# Docker Compose Setup for OpenPeer

## Prerequisites
- Docker Desktop or Docker Engine installed
- `docker compose` command available (v2+)

## docker-compose.yml template

```yaml
services:
  openpeer-db:
    image: postgres:16-alpine
    container_name: openpeer-db
    restart: unless-stopped
    environment:
      POSTGRES_DB: openpeer
      POSTGRES_USER: openpeer
      POSTGRES_PASSWORD: ${DB_PASSWORD:-devpassword}
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U openpeer"]
      interval: 5s
      timeout: 5s
      retries: 5

  openpeer-api:
    build:
      context: ./src/OpenPeer.Api
      dockerfile: Dockerfile
    container_name: openpeer-api
    restart: unless-stopped
    environment:
      ConnectionStrings__Default: "Host=openpeer-db;Database=openpeer;Username=openpeer;Password=${DB_PASSWORD:-devpassword}"
      Jwt__Secret: ${JWT_SECRET:-change-me-in-production-min-256bit}
      Jwt__Issuer: "OpenPeer"
      Jwt__Audience: "OpenPeer"
      ASPNETCORE_ENVIRONMENT: "Development"
    ports:
      - "5000:8080"
    volumes:
      - uploads:/app/Uploads
    depends_on:
      openpeer-db:
        condition: service_healthy

  openpeer-web:
    build:
      context: ./src/OpenPeer.Web
      dockerfile: Dockerfile
    container_name: openpeer-web
    restart: unless-stopped
    ports:
      - "80:80"
    depends_on:
      - openpeer-api

volumes:
  pgdata:
  uploads:
```

## API Dockerfile (`src/OpenPeer.Api/Dockerfile`)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["OpenPeer.Api.csproj", "OpenPeer.Api/"]
COPY ["../OpenPeer.Application/OpenPeer.Application.csproj", "OpenPeer.Application/"]
COPY ["../OpenPeer.Domain/OpenPeer.Domain.csproj", "OpenPeer.Domain/"]
COPY ["../OpenPeer.Infrastructure/OpenPeer.Infrastructure.csproj", "OpenPeer.Infrastructure/"]
RUN dotnet restore "OpenPeer.Api/OpenPeer.Api.csproj"
COPY . .
WORKDIR "/src/OpenPeer.Api"
RUN dotnet build -c Release -o /app/build
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Auto-run migrations on startup
ENTRYPOINT ["sh", "-c", "dotnet OpenPeer.Api.dll"]
```

## Web Dockerfile (`src/OpenPeer.Web/Dockerfile`)

```dockerfile
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

## Key commands

```bash
docker compose up -d --build   # Build images and start all services
docker compose up -d           # Start without rebuilding
docker compose down            # Stop all services
docker compose down -v         # Stop + remove volumes (reset DB)
docker compose logs -f api     # Tail API logs
docker compose ps              # Check service status
```

## Ports
| Service | Host Port | Container Port |
|---------|-----------|---------------|
| PostgreSQL | 5432 | 5432 |
| API | 5000 | 8080 |
| Web (Nginx) | 80 | 80 |

## Environment variables
Create `.env` in project root:
```
DB_PASSWORD=secure-dev-password
JWT_SECRET=your-256bit-or-longer-secret-key-goes-here
```

Never commit `.env` to Git — add to `.gitignore`.
