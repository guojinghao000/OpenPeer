---
name: ef-core
description: Entity Framework Core 8 patterns for PostgreSQL — entity configuration, migrations, full-text search, rating recalculation
---

# EF Core 8 + PostgreSQL Conventions

## Primary keys
All entities use UUID (Guid):
```csharp
builder.Property(e => e.Id)
    .HasDefaultValueSql("gen_random_uuid()");
```

## Entity configuration pattern
Place in `Infrastructure/Data/Configurations/` using `IEntityTypeConfiguration<T>`:
```csharp
public class PaperConfiguration : IEntityTypeConfiguration<Paper>
{
    public void Configure(EntityTypeBuilder<Paper> builder)
    {
        builder.Property(p => p.Title).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Status).HasMaxLength(20).HasDefaultValue("Published");
        builder.HasIndex(p => p.AuthorId);
        builder.HasIndex(p => p.PublishedAt).IsDescending();
        builder.HasIndex(p => p.AverageRating).IsDescending();
        // Soft delete filter
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
```

## Key relationships
- **User → Papers**: `DeleteBehavior.Restrict` (don't cascade delete papers when user deleted)
- **User → Ratings**: `DeleteBehavior.Cascade`
- **User → Comments**: `DeleteBehavior.Cascade`
- **Paper → Ratings**: `DeleteBehavior.Cascade`
- **Paper → Comments**: `DeleteBehavior.Cascade`
- **Paper ↔ Category**: Many-to-many via `PaperCategory` join entity
- **Comment → Comment**: Self-referencing for replies, `DeleteBehavior.Restrict`

## Unique constraints
Ratings: unique composite index on `(PaperId, UserId)` — one rating per user per paper.

## Migration commands
```bash
# From solution root
dotnet ef migrations add <MigrationName> -p src/OpenPeer.Infrastructure -s src/OpenPeer.Api
dotnet ef database update -p src/OpenPeer.Infrastructure -s src/OpenPeer.Api

# Generate SQL script for production
dotnet ef migrations script -p src/OpenPeer.Infrastructure -s src/OpenPeer.Api -o migrate.sql
```

## Full-text search (raw SQL OK here)
```sql
CREATE INDEX "IX_Papers_SearchVector"
ON "Papers"
USING GIN (to_tsvector('english', "Title" || ' ' || "Abstract"));
```

Query via `EF.Functions` or raw SQL interpolation (parameterized keyword input only).

## Rating recalculation pattern
Every rating write must run in a transaction:
1. INSERT/UPDATE the `Ratings` row
2. Recalculate `Papers.AverageRating` and `Papers.RatingCount` from aggregated `Ratings`
3. Commit transaction

Use `ExecuteSqlRawAsync` for the aggregation update query (raw SQL OK for performance).
