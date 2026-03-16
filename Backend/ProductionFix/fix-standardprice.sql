-- Minimal fix for "Unknown column 'l.StandardPrice'" error
-- Safe to run multiple times (skips if already applied)
-- No need to select database - uses full table names

-- Add StandardPrice column (only if it doesn't exist)
SET @col_exists = (SELECT COUNT(*) FROM information_schema.columns 
  WHERE table_schema = 'LogoDesignPortalDb' AND table_name = 'LogoOrders' AND column_name = 'StandardPrice');

SET @sql = IF(@col_exists = 0, 
  'ALTER TABLE LogoDesignPortalDb.LogoOrders ADD COLUMN StandardPrice decimal(18,2) NULL', 
  'SELECT "StandardPrice already exists"');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Record migration (only if not already recorded)
INSERT IGNORE INTO LogoDesignPortalDb.__EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment', '8.0.0');
