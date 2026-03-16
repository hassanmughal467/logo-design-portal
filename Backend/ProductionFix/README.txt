========================================
  EASY PRODUCTION FIX - Missing Tables/Columns
========================================

STEP 1: Copy this entire folder to your production server
       - Copy the "ProductionFix" folder
       - Paste it anywhere, e.g. C:\ProductionFix or Desktop

STEP 2: Open MySQL Workbench on the production server
        Connect to localhost (root + your password)

STEP 3: File -> Open SQL Script -> select fix-all-missing-tables.sql
        Click the lightning bolt (Execute) to run it

STEP 4: Open IIS Manager (Win+R, type: inetmgr)
        Application Pools -> Right-click your API pool -> Recycle

STEP 5: Refresh your website - errors should be gone!

----------------------------------------
Files in this folder:
----------------------------------------
  production-migration-fix.sql  <- RECOMMENDED: Full fix for 500 errors
  run-production-migration.bat  <- Run the above (Windows)
  fix-all-missing-tables.sql   <- StandardPrice + DesignPricings only
  fix-standardprice.sql        <- StandardPrice column only
  fix-designpricings.sql       <- DesignPricings table only

----------------------------------------
If using Command Prompt instead:
----------------------------------------
  cd C:\ProductionFix
  mysql -h localhost -u root -p LogoDesignPortalDb < fix-all-missing-tables.sql
