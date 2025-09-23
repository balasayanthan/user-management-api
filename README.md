# User Management API (.NET)

A clean, CQRS-style .NET API for managing users, groups, and access rules. The solution uses MediatR, EF Core (SQL Server), AutoMapper, FluentValidation, API Versioning, Serilog, and Health Checks—set up with a thin controller layer over application handlers.

> **Auth note:** Only the **Reports** endpoints currently require authorization. Authorization uses a **static Bearer token** configured in `appsettings.Development.json` for easy local testing.

---

## Table of Contents

* [Features](#features)
* [Tech Stack](#tech-stack)
* [Solution Structure](#solution-structure)
* [Prerequisites](#prerequisites)
* [Quick Start](#quick-start)

  * [1) Clone](#1-clone)
  * [2) Configure](#2-configure)
  * [3) Database](#3-database)
  * [4) Run](#4-run)
* [Configuration](#configuration)

  * [Authorization (Static Bearer)](#authorization-static-bearer)
  * [CORS](#cors)
  * [Database Migrations & Seed](#database-migrations--seed)
* [API Surface (v1)](#api-surface-v1)
* [Health Checks](#health-checks)
* [Swagger](#swagger)
* [Troubleshooting](#troubleshooting)
* [Useful Commands](#useful-commands)
* [Notes for Production](#notes-for-production)

---

## Features

* ✅ Clean layering: **Api → Application (CQRS) → Infrastructure (EF Core) → Domain**
* ✅ DTOs, validators (FluentValidation), AutoMapper projections (`ProjectTo`)
* ✅ SQL Server via EF Core, migrations, optional seeding
* ✅ Serilog request logging, correlation-id header
* ✅ API Versioning (`/api/v1/...`)
* ✅ **Health checks** (`/health`, `/health/ready`)
* ✅ **Swagger UI** with Bearer auth support
* ✅ **Authorization on Reports** (static Bearer tokens for local dev)

---

## Tech Stack

* .NET 9 / ASP.NET Core
* Entity Framework Core (SQL Server)
* MediatR (CQRS)
* AutoMapper
* FluentValidation
* Serilog
* HealthChecks
* Swashbuckle (Swagger)

---

## Solution Structure

```
access-mgmt/
  src/
    Api/                # Controllers, middleware, Program.cs, Swagger, Auth (static bearer)
    Application/        # CQRS handlers, DTOs, validators, pipeline behaviors
    Infrastructure/     # EF Core DbContext, Migrations, Seed
    Domain/             # Entities, value objects
  tests/
    UnitTests/
    IntegrationTests/
```

---

## Prerequisites

* .NET SDK **9.x**
* SQL Server (LocalDB / Express / Docker)
* EF Core Tools (optional): `dotnet tool update -g dotnet-ef`

---

## Quick Start

### 1) Clone

```bash
git clone <your-repo-url>
cd user-management-api/access-mgmt
```

### 2) Configure

**Check these before running:**

* **Connection string** – `src/Api/appsettings.json` under `ConnectionStrings:Default`.

  Examples:

  ```json
  // LocalDB (Windows)
  "Server=(localdb)\\MSSQLLocalDB;Database=AccessMgmt;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"

  // SQL Express
  "Server=.\\SQLEXPRESS;Database=AccessMgmt;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"

  // Docker SQL (localhost:1433)
  "Server=localhost,1433;Database=AccessMgmt;User Id=sa;Password=YourStrong!Passw0rd;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True"
  ```

* **CORS** – in `src/Api/appsettings.json`:

  ```json
  "Cors": {
    "AllowedOrigins": [ "http://localhost:4200", "http://127.0.0.1:4200" ],
    "AllowCredentials": false
  }
  ```

* **Auth toggle & static tokens** – in `src/Api/appsettings.Development.json` (dev only):

  ```json
  "Auth": {
    "Enabled": true,
    "StaticBearer": {
      "Tokens": [
        {
          "token": "admin-token-123",
          "name": "Admin Demo",
          "claims": [ "perm:CanViewReports", "perm:CanManageUsers" ]
        },
        {
          "token": "staff-token-123",
          "name": "Staff Demo",
          "claims": [ "perm:CanViewReports" ]
        }
      ]
    }
  }
  ```

  > With `"Auth:Enabled": true`, **Reports** endpoints require a valid token. Other controllers are currently open.

* **Migrate/Seed toggles** (optional) – `src/Api/appsettings.Development.json`:

  ```json
  "Database": { "MigrateOnStartup": true, "SeedOnStartup": true }
  ```

### 3) Database

If applying migrations manually:

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

> If `"Database:MigrateOnStartup": true` is enabled, the API will migrate automatically at startup (recommended for dev only).

### 4) Run

```bash
dotnet run --project src/Api
```

Default Kestrel ports (unless overridden):

* HTTP: `http://localhost:5000`
* HTTPS: `https://localhost:5001`

Open Swagger: `http://localhost:5000/swagger`

---

## Configuration

### Authorization (Static Bearer)

* Toggle via `Auth.Enabled` in `appsettings.Development.json`.
* Tokens are configured under `Auth:StaticBearer:Tokens`.
* **Protection scope:** Only **ReportsController** is decorated with `[Authorize(Policy = "CanViewReports")]`.
  All other controllers are currently unauthenticated for convenience during development/demo.

**Using tokens in Swagger:**

1. Run the API and open Swagger.
2. Click **Authorize** (top-right).
3. In **Bearer** field, paste **only** the token (e.g., `staff-token-123`).
4. Call `GET /api/v1/reports/user-names-by-permission`.

> Later, you can swap this static handler for real **JWT Bearer** without changing policies.

---

### CORS

Configured in `src/Api/appsettings.json`:

```json
"Cors": {
  "AllowedOrigins": [ "http://localhost:4200" ],
  "AllowCredentials": false
}
```

* Add your frontend origins here.
* Set `AllowCredentials` to `true` if you need cookie/credential support.

---

### Database Migrations & Seed

**Option A: Startup (dev)**

```json
// appsettings.Development.json
"Database": { "MigrateOnStartup": true, "SeedOnStartup": true }
```

On boot, the API will `Migrate()` and seed a small dataset (groups, rules, users) if the DB is empty.

**Option B: CLI**

```bash
# Add migration when model changes
dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/Api

# Apply migrations
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

---

## API Surface (v1)

Base path: `/api/v1`

* **Users**

  * `GET /users` — list with paging/filter/sort
  * `GET /users/{id}`
  * `POST /users`
  * `PUT /users/{id}`
  * `DELETE /users/{id}`

* **Groups**

  * `GET /groups` — list with paging
  * `GET /groups/{id}`
  * `POST /groups`
  * `PUT /groups/{id}`
  * `DELETE /groups/{id}`

* **Access Rules**

  * `GET /groups/{groupId}/rules`
  * `POST /groups/{groupId}/rules`
  * `DELETE /groups/{groupId}/rules/{ruleId}`

* **Reports** *(requires authorization: `CanViewReports`)*

  * `GET /reports/user-names-by-permission?permission=true&ruleName=CanViewReports`

> Errors are returned as **RFC 7807 Problem Details** with a `traceId` for correlation.

---

## Health Checks

* `GET /health` – liveness

Includes an EF Core DbContext check (`AddDbContextCheck<AppDbContext>`).

---

## Swagger

* **UI:** `http://localhost:5000/swagger`
* **Auth:** Click **Authorize** → paste the token (no `Bearer ` prefix needed—Swagger adds it).
* **Versioning:** The UI shows a document for **v1** (API is versioned via `IApiVersionDescriptionProvider`).

---

## Troubleshooting

* **DB connection/“Instance failure”**

  * Ensure SQL Server is running and your connection string matches the instance.
  * LocalDB: `sqllocaldb info` / `sqllocaldb start MSSQLLocalDB`
  * Express: start **SQL Server (SQLEXPRESS)** service
  * Docker: run with `-p 1433:1433` and correct SA password

* **EF Tools warning (9.0.8 vs 9.0.9)**

  * Optional to align: `dotnet tool update -g dotnet-ef --version 9.0.9`

* **CORS blocked**

  * Add your frontend origin to `Cors:AllowedOrigins` in `appsettings.json`.

* **401/403 in Swagger**

  * Click **Authorize**; use a token from `appsettings.Development.json`:

    * `staff-token-123` → can view reports
    * `admin-token-123` → can view reports + (future) manage endpoints if you secure them

* **Ports**

  * Use the URLs printed by Kestrel on startup if different from defaults.

---

## Useful Commands

```bash
# Build
dotnet build

# Run API
dotnet run --project src/Api

# Add migration
dotnet ef migrations add Initial --project src/Infrastructure --startup-project src/Api

# Update database
dotnet ef database update --project src/Infrastructure --startup-project src/Api

# Run tests
dotnet test
```
