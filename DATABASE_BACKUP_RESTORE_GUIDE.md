# Database Backup & Restore Guide (VPS / No IDE)

Use these commands on any machine with MySQL client installed (including your VPS via SSH).

---

## Option 1: Fresh VPS Setup (No Existing Data)

If this is a new deployment, you **don't need a backup file**. The app creates the schema automatically on first run.

### On your VPS (via SSH):

```bash
# 1. Create the database
mysql -u root -p -e "CREATE DATABASE LogoDesignPortalDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"

# 2. Deploy and run the app - migrations run automatically on startup
```

---

## Option 2: Backup Existing Database (Create Backup File)

### From your local machine or any server with the data:

```bash
# Full backup (schema + data)
mysqldump -u root -p LogoDesignPortalDb > LogoDesignPortalDb_backup.sql

# Or with explicit host (for remote backup)
mysqldump -h localhost -u root -p LogoDesignPortalDb > LogoDesignPortalDb_backup.sql
```

### Copy the backup file to your VPS:

```bash
# Using SCP (from your local machine)
scp LogoDesignPortalDb_backup.sql user@your-vps-ip:/home/user/
```

---

## Option 3: Restore Backup on VPS

### On your VPS (via SSH):

```bash
# 1. Create empty database (if not exists)
mysql -u root -p -e "CREATE DATABASE IF NOT EXISTS LogoDesignPortalDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"

# 2. Restore from backup file
mysql -u root -p LogoDesignPortalDb < LogoDesignPortalDb_backup.sql

# Or if backup file is in a different location:
mysql -u root -p LogoDesignPortalDb < /path/to/LogoDesignPortalDb_backup.sql
```

---

## Option 4: Schema-Only Backup (No Data)

Use when you only need the table structure:

```bash
mysqldump -u root -p --no-data LogoDesignPortalDb > LogoDesignPortalDb_schema_only.sql
```

---

## Quick Reference

| Task | Command |
|------|---------|
| **Create DB** | `mysql -u root -p -e "CREATE DATABASE LogoDesignPortalDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"` |
| **Backup (full)** | `mysqldump -u root -p LogoDesignPortalDb > backup.sql` |
| **Backup (schema only)** | `mysqldump -u root -p --no-data LogoDesignPortalDb > schema.sql` |
| **Restore** | `mysql -u root -p LogoDesignPortalDb < backup.sql` |

---

## Notes

- Replace `root` with your MySQL username if different
- Use `-h your-mysql-host` if MySQL is on a different server
- The backup file is plain text SQL – you can edit it if needed
- For large databases, add `--single-transaction` to mysqldump for consistent backup without locking tables
