# Copy Local Schema to Production

Sync production database **structure** with local to fix migration/column issues. No data transfer.

---

## Step 1: Export schema on LOCAL (your dev PC)

**Option A – Batch file**
- Run `export-schema-only.bat` in the ProductionFix folder
- Creates `local_schema_only.sql` (structure only, no data)

**Option B – MySQL Workbench**
1. Connect to **local** MySQL
2. **Server → Data Export**
3. Select **LogoDesignPortalDb**
4. Check **"Dump Structure Only"** (important)
5. Export to `local_schema_only.sql`

---

## Step 2: Copy file to production server

Copy `local_schema_only.sql` to the production server (RDP, FTP, etc.).

---

## Step 3: Import on production server

**Option A – Batch file**
- Copy `import-schema-only.bat` to the same folder as `local_schema_only.sql`
- Run it (type YES when prompted)
- If MySQL uses port 3307, edit the bat file and add `-P 3307` after `-h localhost`

**Option B – MySQL Workbench**
1. Connect to **production** MySQL
2. Run: `DROP DATABASE IF EXISTS LogoDesignPortalDb; CREATE DATABASE LogoDesignPortalDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;`
3. **Server → Data Import**
4. Import from `local_schema_only.sql` into `LogoDesignPortalDb`

---

## Step 4: Seed Roles (required for login)

Run **`seed-roles-and-superadmin.sql`** in MySQL Workbench. This adds Roles so the API can create SuperAdmin on startup.

## Step 5: Recycle IIS Application Pool

---

## Result

- Production tables match local structure (all columns, indexes, etc.)
- Production tables are **empty** (no data)
- You’ll need to seed or migrate data separately if required
