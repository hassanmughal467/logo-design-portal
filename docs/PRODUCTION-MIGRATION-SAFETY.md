# Production Migration Safety Guide

How to keep production data safe when applying schema changes and migrations.

---

## 1. Always Backup Before Migrations

**Never run migrations on production without a backup.**

### MySQL Backup Commands

```bash
# Full database backup (run before ANY migration)
mysqldump -h YOUR_DB_HOST -u YOUR_USER -p LogoDesignPortalDb > backup_$(date +%Y%m%d_%H%M%S).sql

# Windows PowerShell:
mysqldump -h YOUR_DB_HOST -u YOUR_USER -p LogoDesignPortalDb > "backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"
```

### Restore if Something Goes Wrong

```bash
mysql -h YOUR_DB_HOST -u YOUR_USER -p LogoDesignPortalDb < backup_20260311_120000.sql
```

**Tip:** Keep backups for at least 7 days. Store them off-server (e.g. cloud storage).

---

## 2. Use Staging First

Before applying migrations to production:

1. **Restore production backup to staging** (or a copy of prod DB)
2. **Run the migration on staging**
3. **Verify** the app works with the new schema
4. **Then** apply to production

This catches schema issues before they affect live users.

---

## 3. Migration Workflow (Recommended)

### Step 1: Generate SQL Script from EF Migrations

Instead of running `dotnet ef database update` directly on production, generate a script and review it:

```bash
cd Backend/src/LogoDesignPortal.API

# Generate script for pending migrations only
dotnet ef migrations script --project ../LogoDesignPortal.Infrastructure --output ../../scripts/NewMigration.sql

# Or: script from last applied migration to latest
dotnet ef migrations script --project ../LogoDesignPortal.Infrastructure --idempotent --output ../../scripts/NewMigration.sql
```

### Step 2: Review the Script

- Look for `DROP TABLE`, `DROP COLUMN`, `ALTER TABLE ... DROP` — these can cause data loss
- Check for `UPDATE` / `INSERT` that might overwrite data
- Ensure new columns are nullable or have defaults if the table has existing rows

### Step 3: Test on Staging

```bash
mysql -h STAGING_DB_HOST -u USER -p LogoDesignPortalDb < NewMigration.sql
```

### Step 4: Backup Production, Then Apply

```bash
mysqldump -h PROD_HOST -u USER -p LogoDesignPortalDb > backup_before_migration.sql
mysql -h PROD_HOST -u USER -p LogoDesignPortalDb < NewMigration.sql
```

---

## 4. Safe Migration Patterns

### Additive Changes (Safe)

- `CREATE TABLE` — new tables
- `ALTER TABLE ADD COLUMN` — new columns (use `NULL` or `DEFAULT` for existing rows)
- `CREATE INDEX` — new indexes (can be slow on large tables; consider `ONLINE` if MySQL supports it)

### Risky Changes (Require Care)

| Change | Risk | Mitigation |
|--------|------|------------|
| `DROP COLUMN` | Data loss | Export data first; or add new column, migrate data, drop old in a later migration |
| `DROP TABLE` | Data loss | Never drop tables with data; archive first |
| `ALTER COLUMN` (type change) | Data truncation / conversion errors | Test on staging; backup; consider multi-step migration |
| `RENAME COLUMN` | Code expects old name | Deploy code that supports both, then rename, then remove old support |

### Expand–Contract Pattern (Zero Downtime)

For breaking changes (e.g. renaming a column):

1. **Expand:** Add new column, keep old one. Deploy code that writes to both.
2. **Migrate:** Backfill new column from old.
3. **Contract:** Deploy code that uses only new column. Drop old column in next migration.

---

## 5. Deployment Order

**Migrations before app deployment** (recommended):

1. Backup production DB
2. Apply migration SQL
3. Deploy new app version

This works when migrations are **additive** (new tables, new nullable columns). The old app ignores new columns; the new app works with the updated schema.

**App before migrations** (only when necessary):

Use when the new app **requires** the new schema. In that case:
- Plan for a short maintenance window, or
- Use expand–contract so the new app works with both old and new schema during the transition

---

## 6. Your Existing Scripts

You already have:

- **`ApplyAllMigrations.sql`** — Idempotent; skips already-applied migrations. Safe to run multiple times.
- **`ApplyDesignerLogoPricing.sql`** — Incremental; adds DesignerLogoPricing table only.

**When adding new migrations:**

1. Run `dotnet ef migrations add YourMigrationName`
2. Generate script: `dotnet ef migrations script --idempotent --output scripts/YourMigration.sql`
3. Review the script
4. Test on staging
5. Backup production
6. Run the script on production

---

## 7. Quick Checklist Before Production Migration

- [ ] Backup production database
- [ ] Migration tested on staging (or prod copy)
- [ ] Script reviewed for `DROP` / destructive changes
- [ ] New columns nullable or have defaults (if table has data)
- [ ] Deployment order decided (migration first vs app first)
- [ ] Rollback plan documented (restore from backup if needed)

---

## 8. Rollback Plan

If a migration causes issues:

1. **Restore from backup** (fastest):
   ```bash
   mysql -h PROD_HOST -u USER -p LogoDesignPortalDb < backup_before_migration.sql
   ```

2. **Revert app** to previous version (if app was deployed)

3. **Manual fix** — Only if you know exactly what went wrong and can fix it safely. Prefer restore when in doubt.

---

## 9. Avoid Export/Import for Schema Updates

You mentioned exporting SQL and importing into production. That approach is fine for **initial setup** or **full restore**, but for **ongoing schema updates**:

- **Do not** overwrite production with a full export from dev — that would replace all production data with dev data.
- **Do** use migration scripts (EF-generated or your idempotent scripts) to apply only the **schema changes**.
- **Do** keep production data; migrations add/alter structure without replacing data.

**Summary:** Use **migration scripts** for schema changes; use **backup/restore** only for recovery.
