-- =============================================================================
-- Production Database Migration Script - Logo Design Portal
-- =============================================================================
-- Fixes 500 errors on /api/orders and /api/admin/analytics/overview caused by
-- missing columns: ClientBasePrice, ClientChargePrice, DesignerProposedPrice,
-- DesignerApprovedPrice, CurrencyCode, StandardPrice
--
-- USAGE:
--   1. BACKUP your database first: mysqldump -u root -p LogoDesignPortalDb > backup.sql
--   2. Connect to MySQL: mysql -u root -p LogoDesignPortalDb
--   3. Run this script: source production-migration-fix.sql
--      Or: mysql -u root -p LogoDesignPortalDb < production-migration-fix.sql
--
-- Safe to run multiple times (idempotent - skips if already applied)
-- =============================================================================

-- Ensure we're in the right database (run: mysql -h HOST -u root -p LogoDesignPortalDb < production-migration-fix.sql)
SELECT DATABASE() AS 'CurrentDatabase';

-- -----------------------------------------------------------------------------
-- 1. Add CurrencyCode if missing (from AddClientLogoPricingAndOrderFields)
-- -----------------------------------------------------------------------------
SET @col_exists = (SELECT COUNT(*) FROM information_schema.columns 
  WHERE table_schema = DATABASE() AND table_name = 'LogoOrders' AND column_name = 'CurrencyCode');
SET @sql = IF(@col_exists = 0, 
  'ALTER TABLE LogoOrders ADD COLUMN CurrencyCode VARCHAR(3) CHARACTER SET utf8mb4 NULL', 
  'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- -----------------------------------------------------------------------------
-- 2. Add ClientPrice if missing (from AddClientLogoPricingAndOrderFields)
-- -----------------------------------------------------------------------------
SET @col_exists = (SELECT COUNT(*) FROM information_schema.columns 
  WHERE table_schema = DATABASE() AND table_name = 'LogoOrders' AND column_name = 'ClientPrice');
SET @sql = IF(@col_exists = 0, 
  'ALTER TABLE LogoOrders ADD COLUMN ClientPrice DECIMAL(18,2) NULL', 
  'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- -----------------------------------------------------------------------------
-- 3. Add StandardPrice if missing (from AddStandardPriceAndDesignerInvoiceAdjustment)
-- -----------------------------------------------------------------------------
SET @col_exists = (SELECT COUNT(*) FROM information_schema.columns 
  WHERE table_schema = DATABASE() AND table_name = 'LogoOrders' AND column_name = 'StandardPrice');
SET @sql = IF(@col_exists = 0, 
  'ALTER TABLE LogoOrders ADD COLUMN StandardPrice DECIMAL(18,2) NULL', 
  'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- -----------------------------------------------------------------------------
-- 4. Add ClientBasePrice if missing (from AddPricingWorkflowFields)
-- -----------------------------------------------------------------------------
SET @col_exists = (SELECT COUNT(*) FROM information_schema.columns 
  WHERE table_schema = DATABASE() AND table_name = 'LogoOrders' AND column_name = 'ClientBasePrice');
SET @sql = IF(@col_exists = 0, 
  'ALTER TABLE LogoOrders ADD COLUMN ClientBasePrice DECIMAL(18,2) NOT NULL DEFAULT 0', 
  'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- -----------------------------------------------------------------------------
-- 5. Add ClientChargePrice if missing (from AddPricingWorkflowFields)
-- -----------------------------------------------------------------------------
SET @col_exists = (SELECT COUNT(*) FROM information_schema.columns 
  WHERE table_schema = DATABASE() AND table_name = 'LogoOrders' AND column_name = 'ClientChargePrice');
SET @sql = IF(@col_exists = 0, 
  'ALTER TABLE LogoOrders ADD COLUMN ClientChargePrice DECIMAL(18,2) NOT NULL DEFAULT 0', 
  'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- -----------------------------------------------------------------------------
-- 6. Add DesignerProposedPrice if missing (from AddPricingWorkflowFields)
-- -----------------------------------------------------------------------------
SET @col_exists = (SELECT COUNT(*) FROM information_schema.columns 
  WHERE table_schema = DATABASE() AND table_name = 'LogoOrders' AND column_name = 'DesignerProposedPrice');
SET @sql = IF(@col_exists = 0, 
  'ALTER TABLE LogoOrders ADD COLUMN DesignerProposedPrice DECIMAL(18,2) NULL', 
  'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- -----------------------------------------------------------------------------
-- 7. Add DesignerApprovedPrice if missing (from AddPricingWorkflowFields)
-- -----------------------------------------------------------------------------
SET @col_exists = (SELECT COUNT(*) FROM information_schema.columns 
  WHERE table_schema = DATABASE() AND table_name = 'LogoOrders' AND column_name = 'DesignerApprovedPrice');
SET @sql = IF(@col_exists = 0, 
  'ALTER TABLE LogoOrders ADD COLUMN DesignerApprovedPrice DECIMAL(18,2) NULL', 
  'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- -----------------------------------------------------------------------------
-- 8. Fix CurrencyCode: set NULLs to 'USD', then alter to NOT NULL
-- -----------------------------------------------------------------------------
UPDATE LogoOrders SET CurrencyCode = 'USD' WHERE CurrencyCode IS NULL;

-- Alter CurrencyCode to NOT NULL (only if still nullable)
SET @col_nullable = (SELECT IS_NULLABLE FROM information_schema.columns 
  WHERE table_schema = DATABASE() AND table_name = 'LogoOrders' AND column_name = 'CurrencyCode');
SET @sql = IF(@col_nullable = 'YES', 
  'ALTER TABLE LogoOrders MODIFY COLUMN CurrencyCode VARCHAR(3) CHARACTER SET utf8mb4 NOT NULL DEFAULT ''USD''', 
  'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- -----------------------------------------------------------------------------
-- 9. Backfill ClientBasePrice, ClientChargePrice from ClientPrice or Price
--    (Only update rows where ClientBasePrice = 0 or ClientChargePrice = 0)
-- -----------------------------------------------------------------------------
UPDATE LogoOrders
SET ClientBasePrice = COALESCE(ClientPrice, Price, 0),
    ClientChargePrice = COALESCE(ClientPrice, Price, 0)
WHERE ClientBasePrice = 0 AND ClientChargePrice = 0;

-- -----------------------------------------------------------------------------
-- 10. Backfill DesignerProposedPrice, DesignerApprovedPrice from ProposedPrice, ApprovedPrice
--     (Only if those columns exist - check first)
-- -----------------------------------------------------------------------------
SET @proposed_exists = (SELECT COUNT(*) FROM information_schema.columns 
  WHERE table_schema = DATABASE() AND table_name = 'LogoOrders' AND column_name = 'ProposedPrice');
SET @approved_exists = (SELECT COUNT(*) FROM information_schema.columns 
  WHERE table_schema = DATABASE() AND table_name = 'LogoOrders' AND column_name = 'ApprovedPrice');

-- Run backfill only if ProposedPrice/ApprovedPrice exist
SET @sql = IF(@proposed_exists > 0 AND @approved_exists > 0,
  'UPDATE LogoOrders SET DesignerProposedPrice = ProposedPrice, DesignerApprovedPrice = ApprovedPrice WHERE DesignerProposedPrice IS NULL AND DesignerApprovedPrice IS NULL',
  'SELECT 1');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- -----------------------------------------------------------------------------
-- 11. Create DesignPricings table if missing (from AddDesignPricingTable)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS DesignPricings (
    Id char(36) COLLATE ascii_general_ci NOT NULL,
    DesignCategory int NOT NULL,
    DesignType int NOT NULL,
    DefaultPrice decimal(18,2) NOT NULL,
    IsActive tinyint(1) NOT NULL DEFAULT 1,
    CreatedAt datetime(6) NOT NULL,
    UpdatedAt datetime(6) NULL,
    CreatedBy char(36) COLLATE ascii_general_ci NULL,
    UpdatedBy char(36) COLLATE ascii_general_ci NULL,
    IsDeleted tinyint(1) NOT NULL,
    DeletedAt datetime(6) NULL,
    DeletedBy char(36) COLLATE ascii_general_ci NULL,
    PRIMARY KEY (Id),
    UNIQUE KEY IX_DesignPricings_DesignCategory_DesignType (DesignCategory, DesignType)
) CHARACTER SET=utf8mb4;

INSERT IGNORE INTO DesignPricings (Id, DesignCategory, DesignType, DefaultPrice, IsActive, CreatedAt, IsDeleted)
VALUES
('a1000001-0000-0000-0000-000000000001', 1, 1, 350.00, 1, '2026-03-09 01:01:05', 0),
('a1000001-0000-0000-0000-000000000002', 1, 2, 700.00, 1, '2026-03-09 01:01:05', 0),
('a1000001-0000-0000-0000-000000000003', 2, 3, 350.00, 1, '2026-03-09 01:01:05', 0),
('a1000001-0000-0000-0000-000000000004', 2, 4, 0.00, 1, '2026-03-09 01:01:05', 0);

-- -----------------------------------------------------------------------------
-- 12. Record migrations in __EFMigrationsHistory (skip if already recorded)
-- -----------------------------------------------------------------------------
INSERT IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment', '8.0.0');
INSERT IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20260309010106_AddDesignPricingTable', '8.0.0');
INSERT IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20260309181731_AddClientLogoPricingAndOrderFields', '8.0.0');
INSERT IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20260310003349_AddPricingWorkflowFields', '8.0.0');

-- -----------------------------------------------------------------------------
-- Done. Verify:
-- -----------------------------------------------------------------------------
SELECT 'Migration script completed.' AS Status;
SELECT COLUMN_NAME FROM information_schema.columns 
  WHERE table_schema = DATABASE() AND table_name = 'LogoOrders' 
  AND COLUMN_NAME IN ('ClientBasePrice', 'ClientChargePrice', 'DesignerProposedPrice', 'DesignerApprovedPrice', 'CurrencyCode', 'StandardPrice')
  ORDER BY COLUMN_NAME;
