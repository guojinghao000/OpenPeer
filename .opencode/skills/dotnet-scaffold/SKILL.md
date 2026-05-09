---
name: dotnet-scaffold
description: Scaffold ASP.NET Core 8 Clean Architecture projects with proper project references, NuGet packages, and solution structure
---

# .NET Clean Architecture Scaffold

## Prerequisites
- .NET 8 SDK installed (`dotnet --version` must show 8.x)
- Working directory must be the project root

## Project Structure
```
src/
├── OpenPeer.Domain/          # Class library — zero deps
├── OpenPeer.Application/     # Class library — depends on Domain
├── OpenPeer.Infrastructure/  # Class library — depends on Domain + Application
└── OpenPeer.Api/             # Web API — depends on Application + Infrastructure
```

## Step-by-step

### 1. Create solution and projects

```bash
dotnet new sln -n OpenPeer
dotnet new classlib -n OpenPeer.Domain -o src/OpenPeer.Domain --framework net8.0
dotnet new classlib -n OpenPeer.Application -o src/OpenPeer.Application --framework net8.0
dotnet new classlib -n OpenPeer.Infrastructure -o src/OpenPeer.Infrastructure --framework net8.0
dotnet new webapi -n OpenPeer.Api -o src/OpenPeer.Api --framework net8.0 --no-https
```

### 2. Add projects to solution

```bash
dotnet sln add src/OpenPeer.Domain/OpenPeer.Domain.csproj
dotnet sln add src/OpenPeer.Application/OpenPeer.Application.csproj
dotnet sln add src/OpenPeer.Infrastructure/OpenPeer.Infrastructure.csproj
dotnet sln add src/OpenPeer.Api/OpenPeer.Api.csproj
```

### 3. Set up project references (Clean Architecture dependency flow)

```bash
dotnet add src/OpenPeer.Application/OpenPeer.Application.csproj reference src/OpenPeer.Domain/OpenPeer.Domain.csproj
dotnet add src/OpenPeer.Infrastructure/OpenPeer.Infrastructure.csproj reference src/OpenPeer.Application/OpenPeer.Application.csproj
dotnet add src/OpenPeer.Api/OpenPeer.Api.csproj reference src/OpenPeer.Application/OpenPeer.Application.csproj
dotnet add src/OpenPeer.Api/OpenPeer.Api.csproj reference src/OpenPeer.Infrastructure/OpenPeer.Infrastructure.csproj
```

### 4. Add required NuGet packages

**OpenPeer.Api:**
```bash
dotnet add src/OpenPeer.Api/OpenPeer.Api.csproj package Swashbuckle.AspNetCore
dotnet add src/OpenPeer.Api/OpenPeer.Api.csproj package Serilog.AspNetCore
dotnet add src/OpenPeer.Api/OpenPeer.Api.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
```

**OpenPeer.Application:**
```bash
dotnet add src/OpenPeer.Application/OpenPeer.Application.csproj package FluentValidation
dotnet add src/OpenPeer.Application/OpenPeer.Application.csproj package Mapster
```

**OpenPeer.Infrastructure:**
```bash
dotnet add src/OpenPeer.Infrastructure/OpenPeer.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/OpenPeer.Infrastructure/OpenPeer.Infrastructure.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add src/OpenPeer.Infrastructure/OpenPeer.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
```

### 5. Delete auto-generated template files

Remove `Class1.cs` from Domain, Application, Infrastructure projects.
Remove auto-generated `WeatherForecast` controller and model from Api.

### 6. Verify build

```bash
dotnet build
```

## Important notes
- Domain layer must NOT reference any other project or NuGet package beyond .NET BCL
- All EF Core code goes in Infrastructure only
- Use file-scoped namespaces throughout
- Primary keys are UUID (Guid), handled in entity configuration via `gen_random_uuid()` default SQL
