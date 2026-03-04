# IIS Deployment - Database Error Troubleshooting

When you see: **"An error occurred while processing your request. Please ensure the database is running and migrations have been applied."** on login after deploying to IIS with a restored database, follow these steps.

---

## 1. Fix the Connection String (Most Common Cause)

IIS runs with `ASPNETCORE_ENVIRONMENT=Production`, so it uses **appsettings.Production.json**.

### Update the deployed app's config

Edit the config in your **deployed** API folder (e.g. `C:\inetpub\LogoDesignPortal\api`):

```
C:\inetpub\LogoDesignPortal\api\appsettings.Production.json
```

Replace the placeholder password with your actual MySQL password:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=LogoDesignPortalDb;User=root;Password=YOUR_ACTUAL_PASSWORD;"
}
```

**Important:** If you edited the source `appsettings.Production.json` in your project, you must **republish** the app for changes to reach the deployed folder, OR edit the file directly in the deployed folder.

---

## 2. Verify MySQL is Running on the VPS

1. Open **Services** (Win+R → `services.msc`)
2. Find **MySQL** or **MySQL80** (or your MySQL service name)
3. Ensure it is **Running**
4. If not, right-click → **Start**

Or in PowerShell (Run as Administrator):

```powershell
Get-Service -Name "MySQL*"
# Start if stopped:
Start-Service -Name "MySQL80"  # Use your actual service name
```

---

## 3. Test Database Connection Manually

From Command Prompt or PowerShell on the VPS:

```cmd
mysql -u root -p -e "USE LogoDesignPortalDb; SELECT COUNT(*) FROM Users;"
```

- If this fails, the problem is MySQL or credentials, not the app.
- Use the same username and password you put in the connection string.

---

## 4. Check Migrations After Restore

When you restore a backup, the `__EFMigrationsHistory` table must exist and match the app’s migrations.

### Option A: Let the app apply migrations

The app applies pending migrations on startup. Ensure:

1. MySQL is running
2. Connection string is correct
3. The app can connect (see step 3)

Then restart the app pool in IIS:

1. Open **IIS Manager** → **Application Pools**
2. Right-click **LogoDesignPortal-API-Pool** → **Recycle**

### Option B: Apply migrations manually

From the project root (where the solution file is):

```powershell
cd Backend\src\LogoDesignPortal.API
dotnet ef database update --project ..\LogoDesignPortal.Infrastructure\LogoDesignPortal.Infrastructure.csproj
```

Use the same connection string (e.g. via `appsettings.Production.json` or environment variable).

---

## 5. View the Actual Error in Logs

The real exception is written to logs. Check:

**IIS stdout logs** (from web.config):

```
C:\inetpub\LogoDesignPortal\api\logs\stdout_*.log
```

Open the latest file and search for `Login failed` or `Error` to see the exact message (e.g. connection refused, wrong password, unknown database).

---

## Quick Checklist

| Step | Action |
|------|--------|
| 1 | Edit `appsettings.Production.json` in the **deployed** folder with correct MySQL password |
| 2 | Confirm MySQL service is running |
| 3 | Test: `mysql -u root -p -e "USE LogoDesignPortalDb;"` |
| 4 | Restart the Application Pool in IIS |
| 5 | If still failing, check `api\logs\stdout_*.log` for the real error |

---

## Common Errors in Logs

| Log message | Fix |
|-------------|-----|
| `Access denied for user 'root'@'localhost'` | Wrong MySQL password in connection string |
| `Unknown database 'LogoDesignPortalDb'` | Create database or fix name: `CREATE DATABASE LogoDesignPortalDb;` |
| `Unable to connect to any of the specified MySQL hosts` | MySQL not running or wrong host/port |
| `Table 'LogoDesignPortalDb.Users' doesn't exist` | Restore failed or migrations not applied; run `dotnet ef database update` |
