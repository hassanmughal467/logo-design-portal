-- Client Logo Pricing migration
-- Run this script to add ClientLogoPricings table and LogoOrder columns

-- 1. Add columns to LogoOrders
ALTER TABLE LogoOrders ADD COLUMN IF NOT EXISTS ClientPrice DECIMAL(18,2) NULL;
ALTER TABLE LogoOrders ADD COLUMN IF NOT EXISTS CurrencyCode VARCHAR(3) NULL;

-- 2. Create ClientLogoPricings table (MySQL)
CREATE TABLE IF NOT EXISTS ClientLogoPricings (
    Id CHAR(36) NOT NULL COLLATE ascii_general_ci,
    ClientId CHAR(36) NOT NULL,
    DesignCategory INT NOT NULL,
    DesignType INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    CurrencyCode VARCHAR(3) NOT NULL DEFAULT 'USD',
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6) NULL,
    CreatedBy CHAR(36) NULL COLLATE ascii_general_ci,
    UpdatedBy CHAR(36) NULL COLLATE ascii_general_ci,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    DeletedAt DATETIME(6) NULL,
    DeletedBy CHAR(36) NULL COLLATE ascii_general_ci,
    PRIMARY KEY (Id),
    UNIQUE KEY IX_ClientLogoPricings_ClientId_DesignCategory_DesignType (ClientId, DesignCategory, DesignType),
    CONSTRAINT FK_ClientLogoPricings_ClientProfiles_ClientId FOREIGN KEY (ClientId) REFERENCES ClientProfiles(Id) ON DELETE RESTRICT
) CHARACTER SET utf8mb4;
