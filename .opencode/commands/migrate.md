---
description: Create and apply EF Core database migrations
agent: dotnet-dev
---

Manage EF Core database migrations.

## If $ARGUMENTS is provided
Use it as the migration name and create a new migration:
```bash
dotnet ef migrations add $ARGUMENTS -p src/OpenPeer.Infrastructure -s src/OpenPeer.Api
```

## If no arguments provided
Apply pending migrations to update the database:
```bash
dotnet ef database update -p src/OpenPeer.Infrastructure -s src/OpenPeer.Api
```

## If migration fails
Check that:
- PostgreSQL is running (use /docker-up if not)
- Connection string in `appsettings.Development.json` or environment is correct
- All entity configurations are registered in `AppDbContext.OnModelCreating`
- No duplicate migration names

Report migration status after completion.
