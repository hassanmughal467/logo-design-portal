-- Fix: Table 'logodesignportaldb.designpricings' doesn't exist
-- Creates DesignPricings table with default pricing data
-- Safe to run multiple times (skips if table exists)

CREATE TABLE IF NOT EXISTS LogoDesignPortalDb.DesignPricings (
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

-- Insert default pricing (IGNORE skips if rows already exist)
INSERT IGNORE INTO LogoDesignPortalDb.DesignPricings 
(Id, DesignCategory, DesignType, DefaultPrice, IsActive, CreatedAt, IsDeleted)
VALUES
('a1000001-0000-0000-0000-000000000001', 1, 1, 350.00, 1, '2026-03-09 01:01:05', 0),
('a1000001-0000-0000-0000-000000000002', 1, 2, 700.00, 1, '2026-03-09 01:01:05', 0),
('a1000001-0000-0000-0000-000000000003', 2, 3, 350.00, 1, '2026-03-09 01:01:05', 0),
('a1000001-0000-0000-0000-000000000004', 2, 4, 0.00, 1, '2026-03-09 01:01:05', 0);

-- Record migration
INSERT IGNORE INTO LogoDesignPortalDb.__EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20260309010106_AddDesignPricingTable', '8.0.0');
