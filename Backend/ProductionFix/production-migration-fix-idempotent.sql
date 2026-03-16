-- =============================================================================
-- Production Migration - IDEMPOTENT (adds only missing columns)
-- =============================================================================
-- Uses stored procedure to add each column only if it doesn't exist.
-- No "Duplicate column" errors - safe to run multiple times.
-- =============================================================================

USE LogoDesignPortalDb;

DELIMITER $$

DROP PROCEDURE IF EXISTS AddColumnIfNotExists$$
CREATE PROCEDURE AddColumnIfNotExists()
BEGIN
    DECLARE col_count INT;
    DECLARE db_name VARCHAR(64);
    SET db_name = DATABASE();

    -- CurrencyCode
    SELECT COUNT(*) INTO col_count FROM information_schema.columns 
    WHERE table_schema = db_name AND table_name = 'LogoOrders' AND column_name = 'CurrencyCode';
    IF col_count = 0 THEN
        ALTER TABLE LogoOrders ADD COLUMN CurrencyCode VARCHAR(3) CHARACTER SET utf8mb4 NULL;
    END IF;

    -- ClientPrice
    SELECT COUNT(*) INTO col_count FROM information_schema.columns 
    WHERE table_schema = db_name AND table_name = 'LogoOrders' AND column_name = 'ClientPrice';
    IF col_count = 0 THEN
        ALTER TABLE LogoOrders ADD COLUMN ClientPrice DECIMAL(18,2) NULL;
    END IF;

    -- StandardPrice
    SELECT COUNT(*) INTO col_count FROM information_schema.columns 
    WHERE table_schema = db_name AND table_name = 'LogoOrders' AND column_name = 'StandardPrice';
    IF col_count = 0 THEN
        ALTER TABLE LogoOrders ADD COLUMN StandardPrice DECIMAL(18,2) NULL;
    END IF;

    -- ClientBasePrice
    SELECT COUNT(*) INTO col_count FROM information_schema.columns 
    WHERE table_schema = db_name AND table_name = 'LogoOrders' AND column_name = 'ClientBasePrice';
    IF col_count = 0 THEN
        ALTER TABLE LogoOrders ADD COLUMN ClientBasePrice DECIMAL(18,2) NOT NULL DEFAULT 0;
    END IF;

    -- ClientChargePrice
    SELECT COUNT(*) INTO col_count FROM information_schema.columns 
    WHERE table_schema = db_name AND table_name = 'LogoOrders' AND column_name = 'ClientChargePrice';
    IF col_count = 0 THEN
        ALTER TABLE LogoOrders ADD COLUMN ClientChargePrice DECIMAL(18,2) NOT NULL DEFAULT 0;
    END IF;

    -- DesignerProposedPrice
    SELECT COUNT(*) INTO col_count FROM information_schema.columns 
    WHERE table_schema = db_name AND table_name = 'LogoOrders' AND column_name = 'DesignerProposedPrice';
    IF col_count = 0 THEN
        ALTER TABLE LogoOrders ADD COLUMN DesignerProposedPrice DECIMAL(18,2) NULL;
    END IF;

    -- DesignerApprovedPrice
    SELECT COUNT(*) INTO col_count FROM information_schema.columns 
    WHERE table_schema = db_name AND table_name = 'LogoOrders' AND column_name = 'DesignerApprovedPrice';
    IF col_count = 0 THEN
        ALTER TABLE LogoOrders ADD COLUMN DesignerApprovedPrice DECIMAL(18,2) NULL;
    END IF;
END$$

DELIMITER ;

CALL AddColumnIfNotExists();
DROP PROCEDURE IF EXISTS AddColumnIfNotExists;

-- Fix CurrencyCode
UPDATE LogoOrders SET CurrencyCode = 'USD' WHERE CurrencyCode IS NULL;
ALTER TABLE LogoOrders MODIFY COLUMN CurrencyCode VARCHAR(3) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'USD';

-- Backfill pricing
UPDATE LogoOrders SET ClientBasePrice = COALESCE(ClientPrice, Price, 0), ClientChargePrice = COALESCE(ClientPrice, Price, 0) WHERE ClientBasePrice = 0 AND ClientChargePrice = 0;

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
