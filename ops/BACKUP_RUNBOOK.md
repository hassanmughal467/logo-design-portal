# Hawk Merchandising Portal — MySQL Backup & Restore Runbook

Automated daily backups of `LogoDesignPortalDb` to Cloudflare R2 (`hawk-backups`), with local staging on the server. Scripts live in `ops/`.

| Script | Purpose |
|--------|---------|
| `backup-mysql.ps1` | Dump, compress, upload to R2, prune old backups |
| `restore-mysql.ps1` | Decompress and restore a `.sql.gz` backup |

---

## Prerequisites

### Software

- MySQL Server 8.0 (`mysqldump.exe`, `mysql.exe`)
- [AWS CLI v2](https://aws.amazon.com/cli/) (S3-compatible API for Cloudflare R2)
- 7-Zip (optional; scripts fall back to PowerShell `GZipStream`)

### Environment variables (never hardcode passwords)

Set these as **system** or **user** environment variables on the backup server, or configure them in the Task Scheduler task action (see below).

| Variable | Used by | Description |
|----------|---------|-------------|
| `MYSQL_ROOT_PASSWORD` | backup + restore | MySQL `root` password |
| `CF_ACCOUNT_ID` | backup | Cloudflare account ID |
| `CF_R2_ACCESS_KEY` | backup | R2 API token access key ID |
| `CF_R2_SECRET_KEY` | backup | R2 API token secret access key |

Verify in an elevated PowerShell session:

```powershell
$env:MYSQL_ROOT_PASSWORD
$env:CF_ACCOUNT_ID
$env:CF_R2_ACCESS_KEY   # should not be empty
$env:CF_R2_SECRET_KEY   # should not be empty
```

### Directories (created automatically by `backup-mysql.ps1`)

```
C:\Backups\MySQL\     # local .sql.gz staging (7-day retention)
C:\Logs\backup.log    # backup job log
```

### R2 layout

```
s3://hawk-backups/daily/LogoDesignPortalDb-{yyyy-MM-dd-HHmm}.sql.gz
```

Retention: **30 daily backups** in R2 (older objects deleted automatically by `backup-mysql.ps1`).

---

## Task Scheduler — daily backup at 02:00

Run once on the server (elevated PowerShell). Adjust the script path if the repo is deployed elsewhere.

```powershell
$scriptPath = "C:\path\to\repo\ops\backup-mysql.ps1"

schtasks /Create /TN "HawkPortal MySQL Backup" /TR "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$scriptPath`"" /SC DAILY /ST 02:00 /RU SYSTEM /RL HIGHEST /F
```

> **Note:** `SYSTEM` cannot read per-user environment variables. Set `MYSQL_ROOT_PASSWORD`, `CF_ACCOUNT_ID`, `CF_R2_ACCESS_KEY`, and `CF_R2_SECRET_KEY` as **system-level** variables (System Properties → Environment Variables → System variables), or embed them in a wrapper script that sets `$env:...` before calling `backup-mysql.ps1`.

### Verify the scheduled task

```powershell
schtasks /Query /TN "HawkPortal MySQL Backup" /V /FO LIST
```

Check:

- **Status** — Ready (or Running during the 02:00 window)
- **Last Run Time** — should advance daily
- **Last Result** — `0` = success; non-zero = failure

### Run manually (smoke test)

```powershell
cd C:\path\to\repo\ops
.\backup-mysql.ps1
echo $LASTEXITCODE   # 0 = success, 1 = failure
```

---

## Verify backups are running

### 1. Task Scheduler

```powershell
schtasks /Query /TN "HawkPortal MySQL Backup" /FO LIST /V
```

Confirm **Last Run Time** is recent and **Last Result** is `0`.

### 2. Log file

```powershell
Get-Content C:\Logs\backup.log -Tail 20
```

Successful run example:

```
[2026-06-10 02:00:15] [INFO] Starting MySQL backup for database 'LogoDesignPortalDb'
[2026-06-10 02:00:45] [INFO] mysqldump complete (42.5 MB uncompressed)
[2026-06-10 02:00:52] [INFO] Compression complete (8.1 MB)
[2026-06-10 02:01:05] [INFO] Upload complete
[2026-06-10 02:01:06] [INFO] Backup succeeded: LogoDesignPortalDb-2026-06-10-0200.sql.gz
```

### 3. Local files

```powershell
Get-ChildItem C:\Backups\MySQL\LogoDesignPortalDb-*.sql.gz | Sort-Object LastWriteTime -Descending | Select-Object -First 5
```

### 4. R2 bucket

```powershell
$env:AWS_ACCESS_KEY_ID     = $env:CF_R2_ACCESS_KEY
$env:AWS_SECRET_ACCESS_KEY = $env:CF_R2_SECRET_KEY
aws s3 ls s3://hawk-backups/daily/ --endpoint-url "https://$($env:CF_ACCOUNT_ID).r2.cloudflarestorage.com"
```

Expect at least one object per day and no more than 30 objects in `daily/`.

---

## Download a backup from R2

List available backups:

```powershell
$env:AWS_ACCESS_KEY_ID     = $env:CF_R2_ACCESS_KEY
$env:AWS_SECRET_ACCESS_KEY = $env:CF_R2_SECRET_KEY
$endpoint = "https://$($env:CF_ACCOUNT_ID).r2.cloudflarestorage.com"

aws s3 ls s3://hawk-backups/daily/ --endpoint-url $endpoint
```

Download a specific file:

```powershell
$filename = "LogoDesignPortalDb-2026-06-10-0200.sql.gz"
$dest     = "C:\Backups\MySQL\$filename"

aws s3 cp "s3://hawk-backups/daily/$filename" $dest --endpoint-url $endpoint
```

Alternatively, download via the [Cloudflare dashboard](https://dash.cloudflare.com) → R2 → `hawk-backups` → `daily/`.

---

## Full restore (production)

> **Warning:** Restoring into `LogoDesignPortalDb` **overwrites** existing data. Stop the API (IIS site) before restoring to avoid active connections corrupting the import.

### Step 1 — Stop the application

```powershell
Import-Module WebAdministration
Stop-WebSite -Name "HawkPortal"
```

### Step 2 — Obtain the backup file

Use a local copy from `C:\Backups\MySQL\` or download from R2 (see above).

### Step 3 — Restore

```powershell
cd C:\path\to\repo\ops

.\restore-mysql.ps1 -BackupFile "C:\Backups\MySQL\LogoDesignPortalDb-2026-06-10-0200.sql.gz"
```

The script will:

1. Decompress the `.sql.gz`
2. Import into `LogoDesignPortalDb` (database must already exist for production restores)
3. Verify row counts in **Orders** (`LogoOrders`), **Invoices**, and **Users**
4. Print a restore summary

### Step 4 — Verify application health

```powershell
Start-WebSite -Name "HawkPortal"
.\smoke-test.ps1 -BaseUrl "https://your-api-host.example.com"
```

### Step 5 — Spot-check data

- Log in as admin and confirm recent orders/invoices are visible
- Compare row counts from the restore summary against expectations

---

## Monthly restore drill (recommended)

Perform this on the **first Monday of each month** to prove backups are restorable. Use a **separate test database** — never overwrite production during a drill.

### Procedure

1. Download the latest backup from R2 (or use the newest local file).
2. Restore to a test database:

```powershell
cd C:\path\to\repo\ops

.\restore-mysql.ps1 `
    -BackupFile "C:\Backups\MySQL\LogoDesignPortalDb-2026-06-10-0200.sql.gz" `
    -TargetDatabase "LogoDesignPortalDb_Test" `
    -CreateDatabase
```

3. Verify the summary shows non-zero row counts for Orders, Invoices, and Users.
4. Optional — run ad-hoc queries:

```powershell
$mysql = "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe"
& $mysql -u root -p"$env:MYSQL_ROOT_PASSWORD" LogoDesignPortalDb_Test -e "
  SELECT COUNT(*) AS orders   FROM LogoOrders;
  SELECT COUNT(*) AS invoices FROM Invoices;
  SELECT COUNT(*) AS users    FROM Users;
"
```

5. Record the drill in your ops log (date, backup filename, row counts, pass/fail).
6. Drop the test database when done:

```powershell
& $mysql -u root -p"$env:MYSQL_ROOT_PASSWORD" -e "DROP DATABASE IF EXISTS LogoDesignPortalDb_Test;"
```

---

## Retention policy

| Location | Retention | Enforced by |
|----------|-----------|-------------|
| `C:\Backups\MySQL\` | 7 days | `backup-mysql.ps1` (local prune) |
| `s3://hawk-backups/daily/` | 30 most recent backups | `backup-mysql.ps1` (R2 prune) |

After 30 daily backups accumulate, the oldest R2 object is deleted on each successful run.

---

## Troubleshooting

| Symptom | Likely cause | Action |
|---------|--------------|--------|
| Task Last Result ≠ 0 | Missing env vars, mysqldump failure, R2 upload error | Read `C:\Logs\backup.log` for `[ERROR]` lines |
| `mysqldump not found` | MySQL not installed or path differs | Edit `$MySqlDumpPath` at top of `backup-mysql.ps1` |
| `aws: command not found` | AWS CLI not installed or not on PATH | Install AWS CLI v2; restart Task Scheduler task |
| R2 upload `403` | Wrong access key / secret or bucket name | Verify R2 API token permissions on `hawk-backups` |
| Restore verification warnings | Backup from older schema or partial dump | Check `LogoOrders` / `Invoices` / `Users` exist in source DB version |
| Empty backup file | Disk full or mysqldump permissions | Check `C:\` free space; confirm `root` can read all tables |

---

## Related docs

- `ops/DEPLOYMENT_CHECKLIST.md` — pre-deploy backup reminder
- `docs/CLOUD_STORAGE_MIGRATION_GUIDE.md` — R2 configuration for file storage (separate from DB backups)
