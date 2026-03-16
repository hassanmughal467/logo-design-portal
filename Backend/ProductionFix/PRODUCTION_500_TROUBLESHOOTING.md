# Production 500 Errors – Troubleshooting Guide

If you ran the migration but **still get 500 errors** on `/api/orders`, `/api/admin/analytics/overview`, or `/api/invoices`, follow these steps.

---

## 1. Confirm Migration Ran on the **Production** Database

The API at `api.hawkmerchandising.com` uses the **production database**, not your local one.

**If you ran the migration on your machine only**, it updated your local MySQL. The production database (on the server where the API is hosted) was not changed.

### Action

Run the migration **on the production server**:

1. **SSH/RDP** into the server where the API is hosted (e.g. the VPS).
2. Copy `production-migration-fix.sql` to that server.
3. Run the migration there:

```powershell
# On the server (replace with your MySQL path if needed)
mysql -h localhost -u root -p LogoDesignPortalDb < production-migration-fix.sql
```

4. If the production database is on another host, use `-h`:

```powershell
mysql -h YOUR_PROD_DB_HOST -u root -p LogoDesignPortalDb < production-migration-fix.sql
```

---

## 2. Verify the Schema

Run the verification script on the **production** database:

```powershell
mysql -h localhost -u root -p LogoDesignPortalDb < verify-production-schema.sql
```

You should see **9 rows** (all required columns). If fewer, the migration did not apply fully.

---

## 3. Restart the API

After changing the database:

1. Open **IIS Manager** → **Application Pools**
2. Right-click **LogoDesignPortal-API-Pool** → **Recycle**

Or restart the app pool for your site.

---

## 4. Check Server Logs for the Real Error

500 responses hide the real exception. Check the logs:

**IIS stdout logs** (from web.config):

```
C:\inetpub\LogoDesignPortal\api\logs\stdout_*.log
```

Open the latest file and search for `Error`, `Exception`, or `Unknown column`.

Typical messages:

| Log message | Meaning |
|-------------|---------|
| `Unknown column 'ClientBasePrice' in 'field list'` | Migration not applied to production DB |
| `Access denied for user` | Wrong connection string |
| `Unable to connect to any of the specified MySQL hosts` | MySQL not running or wrong host |

---

## 5. Use EF Core Migrations (Alternative)

If the SQL script fails, use EF Core:

1. On the production server (or a machine with access to the production DB), set the connection string:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=LogoDesignPortalDb;User=root;Password=YOUR_PROD_PASSWORD;"
```

2. Run migrations:

```powershell
cd Backend\src\LogoDesignPortal.API
dotnet ef database update --project ..\LogoDesignPortal.Infrastructure --startup-project .
```

---

## 6. Quick Checklist

| Step | Action |
|------|--------|
| 1 | Run migration on **production** DB (where the API connects) |
| 2 | Run `verify-production-schema.sql` – expect 9 columns |
| 3 | Recycle the IIS Application Pool |
| 4 | Check `api\logs\stdout_*.log` for the real error |
| 5 | If still failing, check connection string in `appsettings.Production.json` (in the deployed folder) |
