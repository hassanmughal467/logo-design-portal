START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    CREATE TABLE `DesignerLogoPricings` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `DesignerId` char(36) NOT NULL,
        `DesignCategory` int NOT NULL,
        `DesignType` int NOT NULL,
        `DefaultPrice` decimal(18,2) NOT NULL,
        `IsActive` tinyint(1) NOT NULL DEFAULT TRUE,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_DesignerLogoPricings` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_DesignerLogoPricings_DesignerProfiles_DesignerId` FOREIGN KEY (`DesignerId`) REFERENCES `DesignerProfiles` (`Id`) ON DELETE RESTRICT
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 22:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 22:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 22:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 22:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 22:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 22:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 22:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 22:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 22:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 22:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 22:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 22:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 22:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 22:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 22:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-10 22:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-10 22:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-10 22:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-10 22:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    CREATE UNIQUE INDEX `IX_DesignerLogoPricings_DesignerId_DesignCategory_DesignType` ON `DesignerLogoPricings` (`DesignerId`, `DesignCategory`, `DesignType`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310222229_AddDesignerLogoPricing') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260310222229_AddDesignerLogoPricing', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

