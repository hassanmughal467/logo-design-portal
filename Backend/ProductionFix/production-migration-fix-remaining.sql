-- =============================================================================
-- Production Migration - REMAINING COLUMNS ONLY
-- =============================================================================
-- Run this AFTER production-migration-fix-simple.sql failed on "Duplicate column".
-- This script SKIPS CurrencyCode (already exists) and adds the rest.
-- Safe to run even if some columns already exist - just ignore those errors.
-- =============================================================================

USE LogoDesignPortalDb;

-- Add ClientPrice (skip if you get "Duplicate column" error)
ALTER TABLE LogoOrders ADD COLUMN ClientPrice DECIMAL(18,2) NULL;

-- Add StandardPrice
ALTER TABLE LogoOrders ADD COLUMN StandardPrice DECIMAL(18,2) NULL;

-- Add ClientBasePrice
ALTER TABLE LogoOrders ADD COLUMN ClientBasePrice DECIMAL(18,2) NOT NULL DEFAULT 0;

-- Add ClientChargePrice
ALTER TABLE LogoOrders ADD COLUMN ClientChargePrice DECIMAL(18,2) NOT NULL DEFAULT 0;

-- Add DesignerProposedPrice
ALTER TABLE LogoOrders ADD COLUMN DesignerProposedPrice DECIMAL(18,2) NULL;

-- Add DesignerApprovedPrice
ALTER TABLE LogoOrders ADD COLUMN DesignerApprovedPrice DECIMAL(18,2) NULL;

-- Fix CurrencyCode: backfill and make NOT NULL (safe if already NOT NULL)
UPDATE LogoOrders SET CurrencyCode = 'USD' WHERE CurrencyCode IS NULL;
ALTER TABLE LogoOrders MODIFY COLUMN CurrencyCode VARCHAR(3) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'USD';

-- Backfill pricing
UPDATE LogoOrders SET ClientBasePrice = COALESCE(ClientPrice, Price, 0), ClientChargePrice = COALESCE(ClientPrice, Price, 0) WHERE ClientBasePrice = 0 AND ClientChargePrice = 0;

-- Backfill designer prices (skip if ProposedPrice/ApprovedPrice don't exist)
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

SELECT 'Remaining migration completed.' AS Status;
