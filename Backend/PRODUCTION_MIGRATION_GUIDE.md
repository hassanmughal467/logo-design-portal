# Production Migration Guide – Fix StandardPrice Error

This guide resolves the **"Unknown column 'l.StandardPrice' in 'field list'"** error on production.

## Prerequisites

1. **Database backup** – Take a full backup of the production database before running any migration.
2. **MySQL client** – `mysql` CLI, MySQL Workbench, or another MySQL client.
3. **Production connection details** – Host, database name, username, password.

---

## Option A: Run SQL Script (Recommended)

### Step 1: Verify prerequisites

Run this on production to ensure required tables exist:

```sql
-- Check if DesignerInvoices exists (required for DesignerInvoiceAdjustments)
SELECT COUNT(*) FROM information_schema.tables 
WHERE table_schema = DATABASE() AND table_name = 'DesignerInvoices';
-- Should return 1. If 0, run Option B instead.
```

### Step 2: Execute the migration script

From your project root:

```powershell
mysql -h <PRODUCTION_HOST> -u <USERNAME> -p <DATABASE_NAME> < Backend/apply-new-migration.sql
```

Or paste and run the contents of `Backend/apply-new-migration.sql` in your MySQL client.

### Step 3: Confirm migration

```sql
-- Verify StandardPrice column exists
SELECT COLUMN_NAME FROM information_schema.columns 
WHERE table_schema = DATABASE() AND table_name = 'LogoOrders' AND column_name = 'StandardPrice';
-- Should return 1 row.

-- Verify migration recorded
SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment';
-- Should return 1 row.
```

---

## Option B: EF Core Migrations

If the SQL script fails (e.g. missing `DesignerInvoices`), apply migrations via EF Core:

### Step 1: Set production connection string

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=<HOST>;Database=<DB>;User=<USER>;Password=<PASSWORD>;"
```

### Step 2: Run migrations

```powershell
cd "c:\Users\MuhammadHassan\Desktop\Web Portal\Backend\src\LogoDesignPortal.API"
dotnet ef database update --project ../LogoDesignPortal.Infrastructure --startup-project .
```

This applies all pending migrations in order.

---

## After Migration

1. Restart the production API (if it was running during the migration).
2. Test the dashboard, orders, invoices, and analytics endpoints.
3. Confirm the 500 errors are gone.

---

## Troubleshooting

| Error | Cause | Fix |
|-------|-------|-----|
| `Table 'DesignerInvoices' doesn't exist` | Earlier migrations not applied | Use Option B (EF Core) to apply all migrations in order |
| `Duplicate column name 'StandardPrice'` | Migration already applied | No action needed; verify `__EFMigrationsHistory` |
| `Access denied` | Wrong credentials | Check connection string and user permissions |
