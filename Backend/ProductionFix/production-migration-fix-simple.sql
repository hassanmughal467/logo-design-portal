-- =============================================================================
-- Production Migration - SIMPLE VERSION (no dynamic SQL)
-- =============================================================================
-- Use this if production-migration-fix.sql fails with Error 1064.
-- Run each block manually, or run the whole file - errors on "duplicate column"
-- can be ignored (means column already exists).
--
-- Run in MySQL Workbench or HeidiSQL: File -> Run SQL file
-- =============================================================================

USE LogoDesignPortalDb;

-- Add CurrencyCode (ignore error if exists)
ALTER TABLE LogoOrders ADD COLUMN CurrencyCode VARCHAR(3) CHARACTER SET utf8mb4 NULL;

-- Add ClientPrice (ignore error if exists)
ALTER TABLE LogoOrders ADD COLUMN ClientPrice DECIMAL(18,2) NULL;

-- Add StandardPrice (ignore error if exists)
ALTER TABLE LogoOrders ADD COLUMN StandardPrice DECIMAL(18,2) NULL;

-- Add ClientBasePrice (ignore error if exists)
ALTER TABLE LogoOrders ADD COLUMN ClientBasePrice DECIMAL(18,2) NOT NULL DEFAULT 0;

-- Add ClientChargePrice (ignore error if exists)
ALTER TABLE LogoOrders ADD COLUMN ClientChargePrice DECIMAL(18,2) NOT NULL DEFAULT 0;

-- Add DesignerProposedPrice (ignore error if exists)
ALTER TABLE LogoOrders ADD COLUMN DesignerProposedPrice DECIMAL(18,2) NULL;

-- Add DesignerApprovedPrice (ignore error if exists)
ALTER TABLE LogoOrders ADD COLUMN DesignerApprovedPrice DECIMAL(18,2) NULL;

-- Fix CurrencyCode: backfill and make NOT NULL
UPDATE LogoOrders SET CurrencyCode = 'USD' WHERE CurrencyCode IS NULL;
ALTER TABLE LogoOrders MODIFY COLUMN CurrencyCode VARCHAR(3) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'USD';

-- Backfill pricing from existing columns
UPDATE LogoOrders SET ClientBasePrice = COALESCE(ClientPrice, Price, 0), ClientChargePrice = COALESCE(ClientPrice, Price, 0) WHERE ClientBasePrice = 0 AND ClientChargePrice = 0;
-- Skip next line if ProposedPrice/ApprovedPrice columns don't exist:
UPDATE LogoOrders SET DesignerProposedPrice = ProposedPrice, DesignerApprovedPrice = ApprovedPrice WHERE DesignerProposedPrice IS NULL AND DesignerApprovedPrice IS NULL;

-- DesignPricings table
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

INSERT IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment', '8.0.0');
INSERT IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20260309010106_AddDesignPricingTable', '8.0.0');
INSERT IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20260309181731_AddClientLogoPricingAndOrderFields', '8.0.0');
INSERT IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20260310003349_AddPricingWorkflowFields', '8.0.0');

SELECT 'Migration completed.' AS Status;
