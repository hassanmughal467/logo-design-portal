# EF Core Migration Strategy – Production Safety

This document defines the migration strategy for the Logo Design Portal backend to prevent data loss and ensure schema synchronization between local development and IIS staging deployment.

---

## 1. Current State Analysis

### DbContext
- **Primary**: `LogoDesignPortal.Infrastructure.Persistence.ApplicationDbContext`
- **Location**: `src/LogoDesignPortal.Infrastructure/Persistence/ApplicationDbContext.cs`
- **Entities**: Users, Roles, Permissions, LogoOrders, Invoices, DesignerInvoices, etc. (30+ entities)
- **Features**: Soft-delete query filters, transaction support, MySQL + SQLite support

### Existing Migrations
- **Count**: 35+ migrations in `src/LogoDesignPortal.Infrastructure/Migrations/`
- **Provider**: MySQL (Pomelo.EntityFrameworkCore.MySql)
- **Idempotency**: EF Core tracks applied migrations in `__EFMigrationsHistory`; `MigrateAsync()` only applies pending migrations—safe to run multiple times.

### Connection Strings
| Environment | Config File | Database |
|-------------|-------------|----------|
| Local | appsettings.json, appsettings.Development.json | LogoDesignPortalDb @ 127.0.0.1 |
| IIS Staging | appsettings.Staging.json (set ASPNETCORE_ENVIRONMENT=Staging in web.config) | LogoDesignPortalDb_Staging (or override) |
| Production | appsettings.Production.json | LogoDesignPortalDb (or env override) |

---

## 2. Rules (Non-Negotiable)

### ✅ DO
1. **Use EF Core migrations** for all schema changes.
2. **Use `Database.Migrate()` / `MigrateAsync()`** for automatic migration at startup.
3. **Run migrations before accepting requests** (blocking startup until complete).
4. **Validate connection strings** per environment (Local vs Staging).
5. **Back up production** before applying migrations.
6. **Add nullable columns** when introducing new required fields on existing tables (migrate data, then make non-nullable in a follow-up migration if needed).
7. **Use `ALTER TABLE ADD COLUMN` with `NULL`** for new columns on tables with data.

### ❌ DO NOT
1. **Never use `EnsureCreated()`** in production or staging.
2. **Never run schema changes** without a migration file.
3. **Avoid `DropTable` / `DropColumn`** unless explicitly approved and data is backed up or no longer needed.
4. **Avoid custom SQL scripts** that bypass `__EFMigrationsHistory` (e.g. `ApplyMigration`).
5. **Never run migrations in background** without blocking startup—requests may hit an incomplete schema.

---

## 3. Migration Workflow

### Creating a New Migration
```powershell
cd "c:\Users\MuhammadHassan\Desktop\Web Portal\Backend\src\LogoDesignPortal.API"
dotnet ef migrations add <MigrationName> --project ../LogoDesignPortal.Infrastructure --startup-project .
```

### Applying Migrations Locally
```powershell
dotnet ef database update --project ../LogoDesignPortal.Infrastructure --startup-project .
```

### Applying on IIS Staging
Migrations run automatically at startup via `MigrateAsync()`. Ensure:
- `ASPNETCORE_ENVIRONMENT` is set (e.g. `Production` in web.config).
- Connection string in `appsettings.Production.json` or environment variable points to staging DB.
- App pool identity has DB access.

### Manual Application (if needed)
```powershell
$env:ConnectionStrings__DefaultConnection = "Server=<HOST>;Database=<DB>;User=<USER>;Password=<PASSWORD>;"
cd "c:\Users\MuhammadHassan\Desktop\Web Portal\Backend\src\LogoDesignPortal.API"
dotnet ef database update --project ../LogoDesignPortal.Infrastructure --startup-project .
```

---

## 4. Safe Migration Patterns for MySQL

### Adding a Nullable Column (No Data Loss)
```csharp
migrationBuilder.AddColumn<decimal>(
    name: "NewAmount",
    table: "LogoOrders",
    type: "decimal(18,2)",
    nullable: true);  // Always nullable first for existing rows
```

### Adding a Required Column with Default
```csharp
migrationBuilder.AddColumn<int>(
    name: "Status",
    table: "LogoOrders",
    type: "int",
    nullable: false,
    defaultValue: 0);  // Default prevents NOT NULL violation on existing rows
```

### Renaming a Column (Avoid Data Loss)
1. Add new column.
2. Data migration: `UPDATE Table SET NewCol = OldCol`.
3. Drop old column in a separate migration.

### Modifying Column Type
- Prefer adding a new column, migrating data, then dropping the old one.
- Direct `ALTER COLUMN` can truncate or fail on incompatible types.

---

## 5. Destructive Operations (Use with Caution)

Migrations contain `DropTable`, `DropColumn`, `DropIndex` in their `Down()` methods. These are used for rollback only. **Never run `dotnet ef database update <PreviousMigration>` on production** unless you intend to roll back and accept data loss.

For production:
- Only apply forward migrations (`dotnet ef database update` with no target).
- Do not target an older migration.

---

## 6. Environment-Specific Configuration

### Local Development
- Uses `appsettings.json` + `appsettings.Development.json`.
- Database: `LogoDesignPortalDb` on `127.0.0.1`.
- Can use SQLite if connection string contains `Data Source=` or `.db`.

### IIS Staging
- Use `appsettings.Staging.json` by setting `ASPNETCORE_ENVIRONMENT=Staging` in web.config.
- Or use `appsettings.Production.json` with a different connection string override.
- Override connection string via environment variable: `ConnectionStrings__DefaultConnection`.
- Ensure staging DB is separate from production.

### Connection String Override (IIS)
In `web.config` or Application Pool advanced settings:
```xml
<environmentVariable name="ConnectionStrings__DefaultConnection" value="Server=...;Database=...;User=...;Password=...;" />
```

---

## 7. Known Technical Debt

| Item | Location | Risk |
|------|----------|------|
| `ApplyMigration` console app | `Backend/ApplyMigration/` | Bypasses EF; manual `__EFMigrationsHistory` inserts. Use only for emergency fixes. |
| Root-level `LogoDesignPortal.API` | `Backend/LogoDesignPortal.API/` | Uses `EnsureCreated()`; not in solution—dead code. |
| TESTING_GUIDE.md | References `EnsureCreated()` | Outdated; should reference migrations. |

---

## 8. Checklist Before Deploying Schema Changes

- [ ] Migration created with `dotnet ef migrations add`
- [ ] Migration tested locally (`dotnet ef database update`)
- [ ] No `EnsureCreated()` in production code path
- [ ] Migrations run synchronously at startup (before `app.Run()`)
- [ ] Production backup taken
- [ ] Connection string validated for target environment
- [ ] No unapproved `DropTable`/`DropColumn` in new migrations
