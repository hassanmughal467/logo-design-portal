START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    ALTER TABLE `LogoOrders` ADD `StandardPrice` decimal(18,2) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    CREATE TABLE `DesignerInvoiceAdjustments` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `DesignerInvoiceId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `Amount` decimal(18,2) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_DesignerInvoiceAdjustments` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_DesignerInvoiceAdjustments_DesignerInvoices_DesignerInvoiceId` FOREIGN KEY (`DesignerInvoiceId`) REFERENCES `DesignerInvoices` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    UPDATE `Permissions` SET `CreatedAt` = '2026-03-08 23:28:17'
    WHERE `Id` = '10000000-0000-0000-0000-000000000001';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    UPDATE `Permissions` SET `CreatedAt` = '2026-03-08 23:28:17'
    WHERE `Id` = '10000000-0000-0000-0000-000000000002';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    UPDATE `Permissions` SET `CreatedAt` = '2026-03-08 23:28:17'
    WHERE `Id` = '10000000-0000-0000-0000-000000000003';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    UPDATE `Permissions` SET `CreatedAt` = '2026-03-08 23:28:17'
    WHERE `Id` = '10000000-0000-0000-0000-000000000004';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    UPDATE `Permissions` SET `CreatedAt` = '2026-03-08 23:28:17'
    WHERE `Id` = '20000000-0000-0000-0000-000000000001';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    UPDATE `Permissions` SET `CreatedAt` = '2026-03-08 23:28:17'
    WHERE `Id` = '20000000-0000-0000-0000-000000000002';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    UPDATE `Permissions` SET `CreatedAt` = '2026-03-08 23:28:17'
    WHERE `Id` = '20000000-0000-0000-0000-000000000003';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    UPDATE `Permissions` SET `CreatedAt` = '2026-03-08 23:28:17'
    WHERE `Id` = '30000000-0000-0000-0000-000000000001';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    UPDATE `Permissions` SET `CreatedAt` = '2026-03-08 23:28:17'
    WHERE `Id` = '30000000-0000-0000-0000-000000000002';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    UPDATE `Permissions` SET `CreatedAt` = '2026-03-08 23:28:17'
    WHERE `Id` = '30000000-0000-0000-0000-000000000003';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    UPDATE `Permissions` SET `CreatedAt` = '2026-03-08 23:28:17'
    WHERE `Id` = '30000000-0000-0000-0000-000000000004';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    UPDATE `Permissions` SET `CreatedAt` = '2026-03-08 23:28:17'
    WHERE `Id` = '40000000-0000-0000-0000-000000000001';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    UPDATE `Permissions` SET `CreatedAt` = '2026-03-08 23:28:17'
    WHERE `Id` = '50000000-0000-0000-0000-000000000001';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    UPDATE `Permissions` SET `CreatedAt` = '2026-03-08 23:28:17'
    WHERE `Id` = '50000000-0000-0000-0000-000000000002';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    UPDATE `Permissions` SET `CreatedAt` = '2026-03-08 23:28:17'
    WHERE `Id` = '50000000-0000-0000-0000-000000000003';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    UPDATE `Roles` SET `CreatedAt` = '2026-03-08 23:28:17'
    WHERE `Id` = '11111111-1111-1111-1111-111111111111';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    UPDATE `Roles` SET `CreatedAt` = '2026-03-08 23:28:17'
    WHERE `Id` = '22222222-2222-2222-2222-222222222222';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    UPDATE `Roles` SET `CreatedAt` = '2026-03-08 23:28:17'
    WHERE `Id` = '33333333-3333-3333-3333-333333333333';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    UPDATE `Roles` SET `CreatedAt` = '2026-03-08 23:28:17'
    WHERE `Id` = '44444444-4444-4444-4444-444444444444';
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    CREATE INDEX `IX_DesignerInvoiceAdjustments_DesignerInvoiceId` ON `DesignerInvoiceAdjustments` (`DesignerInvoiceId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

