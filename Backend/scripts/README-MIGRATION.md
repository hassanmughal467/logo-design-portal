# Migration Scripts for Production (IIS)

Run these SQL scripts on your **production MySQL database** before deploying new application versions.

---

## ApplyAllMigrations.sql (recommended for 500 errors)

**Complete idempotent script** — applies all migrations from scratch. Safe for:
- Production DB with missing migrations (500 errors on Invoices, Analytics, Orders, etc.)
- Fresh database setup
- Any database state — skips migrations already applied

**When to run:** When you see 500 errors across multiple endpoints after deployment. Run this to bring the database fully up to date.

---

## ApplyDesignerLogoPricing.sql (incremental only)

**Adds:** `DesignerLogoPricings` table (Designer Logo Pricing feature)

**When to run:** When production is already up to date and you only need the Designer Pricing table.

**Prerequisites:** Migration `20260310192426_AddPriceUpdatedTrackingFields` must already be applied.

---

## How to Run on the Server

### Option A: MySQL Command Line

```bash
# For complete migration (recommended when seeing 500 errors):
mysql -h YOUR_DB_HOST -u YOUR_USER -p LogoDesignPortalDb < ApplyAllMigrations.sql

# For incremental (Designer Pricing only):
mysql -h YOUR_DB_HOST -u YOUR_USER -p LogoDesignPortalDb < ApplyDesignerLogoPricing.sql
```

Replace `YOUR_DB_HOST`, `YOUR_USER` with your production values.

**Windows batch file:** Edit `run-migration.bat` (set `MYSQL_HOST`, `MYSQL_USER`, `MYSQL_DATABASE`), then run it from the `scripts` folder.

When prompted, enter the database password.

### Option B: MySQL Workbench / HeidiSQL / phpMyAdmin

**Note:** These scripts use `DELIMITER` and stored procedures. Some GUI clients (e.g. MySQL Workbench) may not handle them correctly and can show Error 1064. **Use the command line (Option A) instead** for reliable execution.

If you must use a GUI:
1. Connect to your production MySQL server.
2. Select the `LogoDesignPortalDb` database.
3. Use **File → Run SQL Script** (or equivalent) and select the script — this often works better than pasting into a query window.

### Option C: PowerShell (if MySQL client is installed)

```powershell
Get-Content ".\ApplyAllMigrations.sql" -Raw | mysql -h YOUR_DB_HOST -u YOUR_USER -p LogoDesignPortalDb
```

---

## Verify After Running

1. Check migrations were applied:
   ```sql
   SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId;
   ```
   Should include `20260310222229_AddDesignerLogoPricing` and all prior migrations.

2. Check critical tables exist:
   ```sql
   SHOW TABLES LIKE 'Invoices';
   SHOW TABLES LIKE 'ClientLogoPricings';
   SHOW TABLES LIKE 'DesignerLogoPricings';
   ```

3. Call the health endpoint after deploying the app:
   ```
   https://api.hawkmerchandising.com/api/system/health
   ```
   Should return `"database": "ok"`.

---

## Idempotent

The script is **idempotent** — safe to run multiple times. It checks `__EFMigrationsHistory` before applying changes and skips if already applied.

---

## Troubleshooting: Error 1064 (SQL syntax)

If you see **Error Code: 1064** when running in MySQL Workbench or another GUI:

1. **Use the command line instead** — GUI clients often mishandle `DELIMITER` and stored procedures:
   ```bash
   mysql -h YOUR_HOST -u YOUR_USER -p LogoDesignPortalDb < ApplyAllMigrations.sql
   ```

2. **Or use the batch file** — Edit `run-migration.bat` with your DB host/user, then double-click or run from Command Prompt.

3. **Ensure MySQL client is in PATH** — The `mysql` command must be available (install MySQL Client or full MySQL if needed).
