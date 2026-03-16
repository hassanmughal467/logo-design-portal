# Reset Production Database from Local

Use this to wipe production and replace it with an exact copy of your local database. **Production will match local exactly.**

---

## ⚠️ WARNING

- **All production data will be deleted** (orders, invoices, users, etc.)
- Take a backup of production first if you might need it
- Ensure your local database has the correct schema and any test data you want

---

## Step 1: Export Local Database (on your dev machine)

Open Command Prompt or PowerShell and run:

```cmd
cd "C:\Users\MuhammadHassan\Desktop\Web Portal\Backend\ProductionFix"

mysqldump -h 127.0.0.1 -u root -p --single-transaction --routines --triggers LogoDesignPortalDb > local_full_backup.sql
```

Enter your local MySQL password when prompted. This creates `local_full_backup.sql` with schema + data.

---

## Step 2: Copy the Backup to Production Server

Copy `local_full_backup.sql` to the production server (e.g. via RDP, shared folder, or SCP).

Place it in a folder like `C:\Users\Administrator\Desktop\Publish\ProductionFix\`

---

## Step 3: Reset Production and Import (on production server)

**Option A: Using MySQL Workbench / HeidiSQL**

1. Connect to the production MySQL server
2. **Drop the database:**
   ```sql
   DROP DATABASE IF EXISTS LogoDesignPortalDb;
   CREATE DATABASE LogoDesignPortalDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
   ```
3. **File → Run SQL file** and select `local_full_backup.sql`

**Option B: Using Command Line**

```cmd
cd C:\Users\Administrator\Desktop\Publish\ProductionFix

REM Drop and recreate database
mysql -h localhost -u root -p -e "DROP DATABASE IF EXISTS LogoDesignPortalDb; CREATE DATABASE LogoDesignPortalDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"

REM Import local backup
mysql -h localhost -u root -p LogoDesignPortalDb < local_full_backup.sql
```

---

## Step 4: Restart the API

1. Open **IIS Manager** → **Application Pools**
2. Right-click **LogoDesignPortal-API-Pool** → **Recycle**

---

## Step 5: Verify

- Log in to the production site
- Check orders, invoices, analytics – they should match local

---

## Quick Reference

| Step | Where | Action |
|------|-------|--------|
| 1 | Local PC | `mysqldump ... LogoDesignPortalDb > local_full_backup.sql` |
| 2 | - | Copy `local_full_backup.sql` to production server |
| 3 | Production | Drop DB, recreate, import backup |
| 4 | Production | Recycle IIS Application Pool |
