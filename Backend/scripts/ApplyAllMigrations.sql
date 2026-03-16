CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    CREATE TABLE `Roles` (
        `Id` char(36) NOT NULL,
        `Name` varchar(50) NOT NULL,
        `Description` varchar(500) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_Roles` PRIMARY KEY (`Id`)
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    CREATE TABLE `Users` (
        `Id` char(36) NOT NULL,
        `Email` varchar(256) NOT NULL,
        `FirstName` varchar(100) NOT NULL,
        `LastName` varchar(100) NOT NULL,
        `PasswordHash` longtext NOT NULL,
        `IsActive` tinyint(1) NOT NULL DEFAULT TRUE,
        `RefreshToken` longtext NULL,
        `RefreshTokenExpiryTime` datetime(6) NULL,
        `RoleId` char(36) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_Users` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Users_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`Id`) ON DELETE RESTRICT
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    CREATE TABLE `ClientProfiles` (
        `Id` char(36) NOT NULL,
        `UserId` char(36) NOT NULL,
        `CompanyName` varchar(200) NOT NULL,
        `PhoneNumber` varchar(20) NULL,
        `Address` varchar(500) NULL,
        `City` varchar(100) NULL,
        `Country` varchar(100) NULL,
        `PostalCode` varchar(20) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_ClientProfiles` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ClientProfiles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    CREATE TABLE `DesignerProfiles` (
        `Id` char(36) NOT NULL,
        `UserId` char(36) NOT NULL,
        `Specialization` varchar(200) NULL,
        `Bio` varchar(1000) NULL,
        `HourlyRate` decimal(18,2) NULL,
        `IsAvailable` tinyint(1) NOT NULL DEFAULT TRUE,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_DesignerProfiles` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_DesignerProfiles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    CREATE TABLE `LogoOrders` (
        `Id` char(36) NOT NULL,
        `ClientId` char(36) NOT NULL,
        `DesignerId` char(36) NULL,
        `Title` varchar(200) NOT NULL,
        `Description` varchar(2000) NOT NULL,
        `Status` int NOT NULL DEFAULT 1,
        `Price` decimal(18,2) NOT NULL,
        `Deadline` datetime(6) NULL,
        `Requirements` varchar(2000) NULL,
        `ColorPreferences` varchar(500) NULL,
        `StylePreferences` varchar(500) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_LogoOrders` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_LogoOrders_ClientProfiles_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `ClientProfiles` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_LogoOrders_DesignerProfiles_DesignerId` FOREIGN KEY (`DesignerId`) REFERENCES `DesignerProfiles` (`Id`) ON DELETE SET NULL
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    CREATE TABLE `Invoices` (
        `Id` char(36) NOT NULL,
        `OrderId` char(36) NOT NULL,
        `InvoiceNumber` varchar(50) NOT NULL,
        `Amount` decimal(18,2) NOT NULL,
        `TaxAmount` decimal(18,2) NOT NULL,
        `TotalAmount` decimal(18,2) NOT NULL,
        `IssueDate` datetime(6) NOT NULL,
        `DueDate` datetime(6) NULL,
        `PaidDate` datetime(6) NULL,
        `IsPaid` tinyint(1) NOT NULL,
        `PaymentMethod` varchar(50) NULL,
        `Notes` varchar(1000) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_Invoices` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Invoices_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `LogoOrders` (`Id`) ON DELETE CASCADE
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    CREATE TABLE `LogoFiles` (
        `Id` char(36) NOT NULL,
        `OrderId` char(36) NOT NULL,
        `FileName` varchar(500) NOT NULL,
        `OriginalFileName` varchar(500) NOT NULL,
        `FilePath` varchar(1000) NOT NULL,
        `ContentType` varchar(100) NOT NULL,
        `FileSize` bigint NOT NULL,
        `IsFinalVersion` tinyint(1) NOT NULL,
        `Description` varchar(1000) NULL,
        `UploadedBy` char(36) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_LogoFiles` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_LogoFiles_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `LogoOrders` (`Id`) ON DELETE CASCADE
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    CREATE TABLE `OrderStatusHistories` (
        `Id` char(36) NOT NULL,
        `OrderId` char(36) NOT NULL,
        `PreviousStatus` int NOT NULL,
        `NewStatus` int NOT NULL,
        `Notes` varchar(1000) NULL,
        `ChangedBy` char(36) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_OrderStatusHistories` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_OrderStatusHistories_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `LogoOrders` (`Id`) ON DELETE CASCADE
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    INSERT INTO `Roles` (`Id`, `CreatedAt`, `CreatedBy`, `DeletedAt`, `DeletedBy`, `Description`, `IsDeleted`, `Name`, `UpdatedAt`, `UpdatedBy`)
    VALUES ('11111111-1111-1111-1111-111111111111', TIMESTAMP '2026-02-05 18:24:55.680206', NULL, NULL, NULL, 'Full system access with all permissions', False, 'SuperAdmin', NULL, NULL),
    ('22222222-2222-2222-2222-222222222222', TIMESTAMP '2026-02-05 18:24:55.680206', NULL, NULL, NULL, 'Administrative access with restricted client data access', False, 'Admin', NULL, NULL),
    ('33333333-3333-3333-3333-333333333333', TIMESTAMP '2026-02-05 18:24:55.680207', NULL, NULL, NULL, 'Designer access without client identity information', False, 'Designer', NULL, NULL),
    ('44444444-4444-4444-4444-444444444444', TIMESTAMP '2026-02-05 18:24:55.680207', NULL, NULL, NULL, 'Client access to their own data', False, 'Client', NULL, NULL);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_ClientProfiles_UserId` ON `ClientProfiles` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_DesignerProfiles_UserId` ON `DesignerProfiles` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Invoices_InvoiceNumber` ON `Invoices` (`InvoiceNumber`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Invoices_OrderId` ON `Invoices` (`OrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    CREATE INDEX `IX_LogoFiles_OrderId` ON `LogoFiles` (`OrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    CREATE INDEX `IX_LogoOrders_ClientId` ON `LogoOrders` (`ClientId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    CREATE INDEX `IX_LogoOrders_DesignerId` ON `LogoOrders` (`DesignerId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    CREATE INDEX `IX_OrderStatusHistories_OrderId` ON `OrderStatusHistories` (`OrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Roles_Name` ON `Roles` (`Name`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Users_Email` ON `Users` (`Email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    CREATE INDEX `IX_Users_RoleId` ON `Users` (`RoleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260205182456_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260205182456_InitialCreate', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260206124650_AddPermissionSystem') THEN

    CREATE TABLE `Permissions` (
        `Id` char(36) NOT NULL,
        `Name` varchar(100) NOT NULL,
        `Description` varchar(500) NOT NULL,
        `Resource` varchar(100) NOT NULL,
        `Action` varchar(50) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_Permissions` PRIMARY KEY (`Id`)
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260206124650_AddPermissionSystem') THEN

    CREATE TABLE `RolePermissions` (
        `Id` char(36) NOT NULL,
        `RoleId` char(36) NOT NULL,
        `PermissionId` char(36) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_RolePermissions` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_RolePermissions_Permissions_PermissionId` FOREIGN KEY (`PermissionId`) REFERENCES `Permissions` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_RolePermissions_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`Id`) ON DELETE CASCADE
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260206124650_AddPermissionSystem') THEN

    INSERT INTO `Permissions` (`Id`, `Action`, `CreatedAt`, `CreatedBy`, `DeletedAt`, `DeletedBy`, `Description`, `IsDeleted`, `Name`, `Resource`, `UpdatedAt`, `UpdatedBy`)
    VALUES ('10000000-0000-0000-0000-000000000001', 'Create', TIMESTAMP '2026-02-06 12:46:49.134563', NULL, NULL, NULL, 'Create new users', False, 'CreateUser', 'User', NULL, NULL),
    ('10000000-0000-0000-0000-000000000002', 'Read', TIMESTAMP '2026-02-06 12:46:49.134564', NULL, NULL, NULL, 'View all users', False, 'ViewUsers', 'User', NULL, NULL),
    ('10000000-0000-0000-0000-000000000003', 'Update', TIMESTAMP '2026-02-06 12:46:49.134565', NULL, NULL, NULL, 'Update user information', False, 'UpdateUser', 'User', NULL, NULL),
    ('10000000-0000-0000-0000-000000000004', 'Delete', TIMESTAMP '2026-02-06 12:46:49.134565', NULL, NULL, NULL, 'Delete users', False, 'DeleteUser', 'User', NULL, NULL),
    ('20000000-0000-0000-0000-000000000001', 'Create', TIMESTAMP '2026-02-06 12:46:49.134566', NULL, NULL, NULL, 'Create designer profiles', False, 'CreateDesignerProfile', 'DesignerProfile', NULL, NULL),
    ('20000000-0000-0000-0000-000000000002', 'Read', TIMESTAMP '2026-02-06 12:46:49.134566', NULL, NULL, NULL, 'View designer profiles', False, 'ViewDesignerProfiles', 'DesignerProfile', NULL, NULL),
    ('20000000-0000-0000-0000-000000000003', 'Update', TIMESTAMP '2026-02-06 12:46:49.134567', NULL, NULL, NULL, 'Update designer profiles', False, 'UpdateDesignerProfile', 'DesignerProfile', NULL, NULL),
    ('30000000-0000-0000-0000-000000000001', 'Create', TIMESTAMP '2026-02-06 12:46:49.134567', NULL, NULL, NULL, 'Create new orders', False, 'CreateOrder', 'Order', NULL, NULL),
    ('30000000-0000-0000-0000-000000000002', 'ReadAll', TIMESTAMP '2026-02-06 12:46:49.134567', NULL, NULL, NULL, 'View all orders in the system', False, 'ViewAllOrders', 'Order', NULL, NULL),
    ('30000000-0000-0000-0000-000000000003', 'Assign', TIMESTAMP '2026-02-06 12:46:49.134568', NULL, NULL, NULL, 'Assign orders to designers', False, 'AssignOrder', 'Order', NULL, NULL),
    ('30000000-0000-0000-0000-000000000004', 'UpdateStatus', TIMESTAMP '2026-02-06 12:46:49.134568', NULL, NULL, NULL, 'Update order status', False, 'UpdateOrderStatus', 'Order', NULL, NULL),
    ('40000000-0000-0000-0000-000000000001', 'Manage', TIMESTAMP '2026-02-06 12:46:49.134569', NULL, NULL, NULL, 'Manage role permissions', False, 'ManagePermissions', 'Permission', NULL, NULL),
    ('50000000-0000-0000-0000-000000000001', 'Upload', TIMESTAMP '2026-02-06 12:46:49.134575', NULL, NULL, NULL, 'Upload files', False, 'UploadFile', 'File', NULL, NULL),
    ('50000000-0000-0000-0000-000000000002', 'Download', TIMESTAMP '2026-02-06 12:46:49.134576', NULL, NULL, NULL, 'Download files', False, 'DownloadFile', 'File', NULL, NULL),
    ('50000000-0000-0000-0000-000000000003', 'Delete', TIMESTAMP '2026-02-06 12:46:49.134576', NULL, NULL, NULL, 'Delete files', False, 'DeleteFile', 'File', NULL, NULL);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260206124650_AddPermissionSystem') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-06 12:46:49.134916'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260206124650_AddPermissionSystem') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-06 12:46:49.134916'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260206124650_AddPermissionSystem') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-06 12:46:49.134917'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260206124650_AddPermissionSystem') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-06 12:46:49.134917'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260206124650_AddPermissionSystem') THEN

    CREATE UNIQUE INDEX `IX_Permissions_Name` ON `Permissions` (`Name`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260206124650_AddPermissionSystem') THEN

    CREATE INDEX `IX_RolePermissions_PermissionId` ON `RolePermissions` (`PermissionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260206124650_AddPermissionSystem') THEN

    CREATE UNIQUE INDEX `IX_RolePermissions_RoleId_PermissionId` ON `RolePermissions` (`RoleId`, `PermissionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260206124650_AddPermissionSystem') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260206124650_AddPermissionSystem', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    CREATE TABLE `Messages` (
        `Id` char(36) NOT NULL,
        `OrderId` char(36) NULL,
        `SenderId` char(36) NOT NULL,
        `RecipientId` char(36) NULL,
        `Content` varchar(2000) NOT NULL,
        `IsRead` tinyint(1) NOT NULL,
        `ReadAt` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_Messages` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Messages_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `LogoOrders` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Messages_Users_RecipientId` FOREIGN KEY (`RecipientId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Messages_Users_SenderId` FOREIGN KEY (`SenderId`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    CREATE TABLE `Reviews` (
        `Id` char(36) NOT NULL,
        `OrderId` char(36) NOT NULL,
        `ClientId` char(36) NOT NULL,
        `Rating` int NOT NULL,
        `Comment` varchar(1000) NULL,
        `IsPublished` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_Reviews` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Reviews_ClientProfiles_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `ClientProfiles` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Reviews_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `LogoOrders` (`Id`) ON DELETE RESTRICT
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    CREATE TABLE `Settings` (
        `Id` char(36) NOT NULL,
        `Key` varchar(100) NOT NULL,
        `Value` longtext NOT NULL,
        `Category` varchar(50) NOT NULL,
        `Description` longtext NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_Settings` PRIMARY KEY (`Id`)
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-07 01:00:48.901908'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-07 01:00:48.901909'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-07 01:00:48.901909'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-07 01:00:48.90191'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-07 01:00:48.90191'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-07 01:00:48.901911'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-07 01:00:48.901911'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-07 01:00:48.901911'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-07 01:00:48.901912'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-07 01:00:48.901912'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-07 01:00:48.901913'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-07 01:00:48.901913'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-07 01:00:48.901914'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-07 01:00:48.901914'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-07 01:00:48.901915'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-07 01:00:48.903329'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-07 01:00:48.903329'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-07 01:00:48.90333'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-07 01:00:48.90333'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    CREATE INDEX `IX_Messages_OrderId` ON `Messages` (`OrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    CREATE INDEX `IX_Messages_RecipientId` ON `Messages` (`RecipientId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    CREATE INDEX `IX_Messages_SenderId` ON `Messages` (`SenderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    CREATE INDEX `IX_Reviews_ClientId` ON `Reviews` (`ClientId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    CREATE INDEX `IX_Reviews_OrderId` ON `Reviews` (`OrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    CREATE UNIQUE INDEX `IX_Settings_Key_Category` ON `Settings` (`Key`, `Category`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260207010050_AddMessagesReviewsSettings') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260207010050_AddMessagesReviewsSettings', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    ALTER TABLE `Users` ADD `PasswordResetToken` longtext NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    ALTER TABLE `Users` ADD `PasswordResetTokenExpiryTime` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-09 14:18:58.335586'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-09 14:18:58.335586'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-09 14:18:58.335586'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-09 14:18:58.335587'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-09 14:18:58.335587'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-09 14:18:58.335587'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-09 14:18:58.335587'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-09 14:18:58.335588'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-09 14:18:58.335588'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-09 14:18:58.335588'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-09 14:18:58.335588'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-09 14:18:58.335589'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-09 14:18:58.335589'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-09 14:18:58.335589'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-09 14:18:58.335592'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-09 14:18:58.336053'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-09 14:18:58.336053'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-09 14:18:58.336054'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-09 14:18:58.336054'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260209141859_AddPasswordResetToken') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260209141859_AddPasswordResetToken', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    ALTER TABLE `Users` ADD `InvoiceEmail` varchar(256) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    ALTER TABLE `Users` ADD `SecondaryEmail` varchar(256) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    ALTER TABLE `ClientProfiles` ADD `Cell` varchar(20) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    ALTER TABLE `ClientProfiles` ADD `ContactName` varchar(200) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    ALTER TABLE `ClientProfiles` ADD `Fax` varchar(20) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    ALTER TABLE `ClientProfiles` ADD `Reference` varchar(200) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    ALTER TABLE `ClientProfiles` ADD `State` varchar(100) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    ALTER TABLE `ClientProfiles` ADD `Website` varchar(500) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 10:40:32.772367'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 10:40:32.772368'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 10:40:32.772368'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 10:40:32.772369'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 10:40:32.772369'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 10:40:32.772369'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 10:40:32.772369'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 10:40:32.77237'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 10:40:32.77237'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 10:40:32.77237'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 10:40:32.77237'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 10:40:32.772371'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 10:40:32.772376'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 10:40:32.772376'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 10:40:32.772376'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 10:40:32.774867'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 10:40:32.774867'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 10:40:32.774868'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 10:40:32.774868'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210104034_AddFullRegistrationFields') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260210104034_AddFullRegistrationFields', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    ALTER TABLE `Invoices` DROP FOREIGN KEY `FK_Invoices_LogoOrders_OrderId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    ALTER TABLE `Invoices` DROP INDEX `IX_Invoices_OrderId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    ALTER TABLE `Invoices` DROP COLUMN `IsPaid`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    ALTER TABLE `Invoices` RENAME COLUMN `OrderId` TO `ClientId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    ALTER TABLE `LogoOrders` ADD `Instructions` longtext NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    ALTER TABLE `LogoOrders` ADD `PriceApproved` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    ALTER TABLE `LogoOrders` ADD `ProposedPrice` decimal(18,2) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    ALTER TABLE `LogoOrders` ADD `RequiredFormats` varchar(500) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    ALTER TABLE `LogoOrders` ADD `RequiresPriceApproval` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    ALTER TABLE `LogoFiles` ADD `ApprovedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    ALTER TABLE `LogoFiles` ADD `ApprovedBy` char(36) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    ALTER TABLE `LogoFiles` ADD `FileType` int NOT NULL DEFAULT 1;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    ALTER TABLE `LogoFiles` ADD `IsAdminApproved` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    ALTER TABLE `LogoFiles` ADD `IsVisibleToClient` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    ALTER TABLE `LogoFiles` ADD `VersionNumber` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    ALTER TABLE `Invoices` ADD `LogoOrderId` char(36) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    ALTER TABLE `Invoices` ADD `Status` int NOT NULL DEFAULT 1;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    CREATE TABLE `ClientGalleries` (
        `Id` char(36) NOT NULL,
        `ClientId` char(36) NOT NULL,
        `OrderId` char(36) NOT NULL,
        `FileId` char(36) NOT NULL,
        `PreviewImagePath` varchar(1000) NOT NULL,
        `FileName` varchar(500) NOT NULL,
        `OriginalFileName` varchar(500) NOT NULL,
        `FilePath` varchar(1000) NOT NULL,
        `ContentType` varchar(100) NOT NULL,
        `Format` varchar(50) NULL,
        `ApprovedAt` datetime(6) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_ClientGalleries` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ClientGalleries_ClientProfiles_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `ClientProfiles` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_ClientGalleries_LogoFiles_FileId` FOREIGN KEY (`FileId`) REFERENCES `LogoFiles` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_ClientGalleries_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `LogoOrders` (`Id`) ON DELETE RESTRICT
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    CREATE TABLE `InvoiceOrders` (
        `Id` char(36) NOT NULL,
        `InvoiceId` char(36) NOT NULL,
        `OrderId` char(36) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_InvoiceOrders` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_InvoiceOrders_Invoices_InvoiceId` FOREIGN KEY (`InvoiceId`) REFERENCES `Invoices` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_InvoiceOrders_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `LogoOrders` (`Id`) ON DELETE RESTRICT
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    CREATE TABLE `Notifications` (
        `Id` char(36) NOT NULL,
        `UserId` char(36) NOT NULL,
        `OrderId` char(36) NULL,
        `Title` varchar(200) NOT NULL,
        `Message` varchar(1000) NOT NULL,
        `Type` int NOT NULL DEFAULT 1,
        `IsRead` tinyint(1) NOT NULL,
        `ReadAt` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_Notifications` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Notifications_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `LogoOrders` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_Notifications_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    CREATE TABLE `OrderComments` (
        `Id` char(36) NOT NULL,
        `OrderId` char(36) NOT NULL,
        `Content` varchar(2000) NOT NULL,
        `CreatedBy` char(36) NOT NULL,
        `IsInternal` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_OrderComments` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_OrderComments_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `LogoOrders` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_OrderComments_Users_CreatedBy` FOREIGN KEY (`CreatedBy`) REFERENCES `Users` (`Id`) ON DELETE RESTRICT
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    CREATE TABLE `OrderRevisions` (
        `Id` char(36) NOT NULL,
        `OrderId` char(36) NOT NULL,
        `Instructions` longtext NOT NULL,
        `RequestedBy` char(36) NOT NULL,
        `IsResolved` tinyint(1) NOT NULL,
        `ResolvedAt` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_OrderRevisions` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_OrderRevisions_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `LogoOrders` (`Id`) ON DELETE CASCADE
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    CREATE TABLE `RevisionFiles` (
        `Id` char(36) NOT NULL,
        `RevisionId` char(36) NOT NULL,
        `FileName` varchar(500) NOT NULL,
        `OriginalFileName` varchar(500) NOT NULL,
        `FilePath` varchar(1000) NOT NULL,
        `ContentType` varchar(100) NOT NULL,
        `FileSize` bigint NOT NULL,
        `FileType` int NOT NULL DEFAULT 1,
        `Description` varchar(1000) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_RevisionFiles` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_RevisionFiles_OrderRevisions_RevisionId` FOREIGN KEY (`RevisionId`) REFERENCES `OrderRevisions` (`Id`) ON DELETE CASCADE
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 12:50:18.185978'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 12:50:18.185978'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 12:50:18.185979'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 12:50:18.18598'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 12:50:18.18598'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 12:50:18.185981'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 12:50:18.185981'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 12:50:18.185982'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 12:50:18.185982'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 12:50:18.185983'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 12:50:18.185983'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 12:50:18.185984'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 12:50:18.185984'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 12:50:18.185985'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 12:50:18.185987'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 12:50:18.189779'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 12:50:18.189781'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 12:50:18.189781'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 12:50:18.189782'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    CREATE INDEX `IX_LogoFiles_ApprovedBy` ON `LogoFiles` (`ApprovedBy`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    CREATE INDEX `IX_Invoices_ClientId` ON `Invoices` (`ClientId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    CREATE INDEX `IX_Invoices_LogoOrderId` ON `Invoices` (`LogoOrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    CREATE INDEX `IX_ClientGalleries_ClientId` ON `ClientGalleries` (`ClientId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    CREATE INDEX `IX_ClientGalleries_FileId` ON `ClientGalleries` (`FileId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    CREATE INDEX `IX_ClientGalleries_OrderId` ON `ClientGalleries` (`OrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    CREATE UNIQUE INDEX `IX_InvoiceOrders_InvoiceId_OrderId` ON `InvoiceOrders` (`InvoiceId`, `OrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    CREATE INDEX `IX_InvoiceOrders_OrderId` ON `InvoiceOrders` (`OrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    CREATE INDEX `IX_Notifications_OrderId` ON `Notifications` (`OrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    CREATE INDEX `IX_Notifications_UserId_IsRead` ON `Notifications` (`UserId`, `IsRead`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    CREATE INDEX `IX_OrderComments_CreatedBy` ON `OrderComments` (`CreatedBy`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    CREATE INDEX `IX_OrderComments_OrderId` ON `OrderComments` (`OrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    CREATE INDEX `IX_OrderRevisions_OrderId` ON `OrderRevisions` (`OrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    CREATE INDEX `IX_RevisionFiles_RevisionId` ON `RevisionFiles` (`RevisionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    ALTER TABLE `Invoices` ADD CONSTRAINT `FK_Invoices_ClientProfiles_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `ClientProfiles` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    ALTER TABLE `Invoices` ADD CONSTRAINT `FK_Invoices_LogoOrders_LogoOrderId` FOREIGN KEY (`LogoOrderId`) REFERENCES `LogoOrders` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    ALTER TABLE `LogoFiles` ADD CONSTRAINT `FK_LogoFiles_Users_ApprovedBy` FOREIGN KEY (`ApprovedBy`) REFERENCES `Users` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210125019_OrderManagementSystemEnhancement') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260210125019_OrderManagementSystemEnhancement', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    ALTER TABLE `LogoOrders` ADD `Priority` int NOT NULL DEFAULT 2;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 17:33:20.558213'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 17:33:20.558215'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 17:33:20.558216'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 17:33:20.558235'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 17:33:20.558237'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 17:33:20.558237'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 17:33:20.558238'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 17:33:20.558239'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 17:33:20.55824'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 17:33:20.558241'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 17:33:20.558248'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 17:33:20.558249'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 17:33:20.55825'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 17:33:20.558251'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 17:33:20.558252'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 17:33:20.567044'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 17:33:20.567047'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 17:33:20.567048'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 17:33:20.567049'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210173325_AddOrderPriority') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260210173325_AddOrderPriority', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    ALTER TABLE `LogoOrders` ADD `ArchivedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    ALTER TABLE `LogoOrders` ADD `ArchivedBy` char(36) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    ALTER TABLE `LogoOrders` ADD `CancellationReason` varchar(2000) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    ALTER TABLE `LogoOrders` ADD `CancelledAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    ALTER TABLE `LogoOrders` ADD `CancelledBy` char(36) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    ALTER TABLE `LogoOrders` ADD `IsArchived` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    ALTER TABLE `LogoOrders` ADD `IsCancelledByUser` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    ALTER TABLE `LogoOrders` ADD `IsRefunded` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    ALTER TABLE `LogoOrders` ADD `RefundAmount` decimal(18,2) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    ALTER TABLE `LogoOrders` ADD `RefundReason` varchar(2000) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    ALTER TABLE `LogoOrders` ADD `RefundedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    ALTER TABLE `LogoOrders` ADD `RefundedBy` char(36) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    CREATE TABLE `OrderLogs` (
        `Id` char(36) NOT NULL,
        `OrderId` char(36) NOT NULL,
        `Action` int NOT NULL,
        `PreviousStatus` int NULL,
        `NewStatus` int NULL,
        `PerformedBy` varchar(100) NULL,
        `PerformedById` char(36) NULL,
        `Note` varchar(2000) NULL,
        `Metadata` longtext NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_OrderLogs` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_OrderLogs_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `LogoOrders` (`Id`) ON DELETE RESTRICT
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 18:55:44.294338'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 18:55:44.294339'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 18:55:44.294339'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 18:55:44.29434'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 18:55:44.29434'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 18:55:44.29434'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 18:55:44.29434'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 18:55:44.294341'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 18:55:44.294341'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 18:55:44.294341'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 18:55:44.294341'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 18:55:44.294342'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 18:55:44.294342'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 18:55:44.294342'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 18:55:44.294343'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 18:55:44.295205'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 18:55:44.295205'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 18:55:44.295206'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 18:55:44.295206'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    CREATE INDEX `IX_OrderLogs_Action` ON `OrderLogs` (`Action`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    CREATE INDEX `IX_OrderLogs_CreatedAt` ON `OrderLogs` (`CreatedAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    CREATE INDEX `IX_OrderLogs_OrderId` ON `OrderLogs` (`OrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210185545_ProfessionalOrderManagementSystem') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260210185545_ProfessionalOrderManagementSystem', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    SET FOREIGN_KEY_CHECKS = 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    ALTER TABLE `InvoiceOrders` DROP INDEX `IX_InvoiceOrders_InvoiceId_OrderId`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    SET FOREIGN_KEY_CHECKS = 1;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    ALTER TABLE `Invoices` ADD `BillingType` int NOT NULL DEFAULT 1;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    ALTER TABLE `InvoiceOrders` MODIFY COLUMN `OrderId` char(36) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    ALTER TABLE `InvoiceOrders` ADD `Amount` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    ALTER TABLE `InvoiceOrders` ADD `Description` varchar(500) NOT NULL DEFAULT '';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN


                    UPDATE InvoiceOrders io
                    INNER JOIN LogoOrders o ON io.OrderId = o.Id
                    SET io.Amount = o.Price,
                        io.Description = IFNULL(o.Title, CONCAT('Logo Design - Order #', LEFT(o.Id, 8)))
                    WHERE io.OrderId IS NOT NULL;
                

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    CREATE TABLE `InvoiceLogs` (
        `Id` char(36) NOT NULL,
        `InvoiceId` char(36) NOT NULL,
        `Action` int NOT NULL,
        `PerformedBy` char(36) NULL,
        `Notes` varchar(1000) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_InvoiceLogs` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_InvoiceLogs_Invoices_InvoiceId` FOREIGN KEY (`InvoiceId`) REFERENCES `Invoices` (`Id`) ON DELETE CASCADE
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:22:30.921464'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:22:30.921464'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:22:30.921464'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:22:30.921464'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:22:30.921465'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:22:30.921465'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:22:30.921465'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:22:30.921465'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:22:30.921465'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:22:30.921465'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:22:30.921466'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:22:30.921466'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:22:30.921466'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:22:30.921466'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:22:30.921467'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:22:30.922123'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:22:30.922123'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:22:30.922124'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:22:30.922124'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    CREATE UNIQUE INDEX `IX_InvoiceOrders_InvoiceId_OrderId` ON `InvoiceOrders` (`InvoiceId`, `OrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    CREATE INDEX `IX_InvoiceLogs_CreatedAt` ON `InvoiceLogs` (`CreatedAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    CREATE INDEX `IX_InvoiceLogs_InvoiceId` ON `InvoiceLogs` (`InvoiceId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210192231_InvoiceSystemEnhancement') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260210192231_InvoiceSystemEnhancement', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    ALTER TABLE `Users` ADD `DeactivatedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    ALTER TABLE `Users` ADD `DeactivatedBy` char(36) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    ALTER TABLE `Users` ADD `FailedLoginAttempts` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    ALTER TABLE `Users` ADD `LockoutEnd` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    ALTER TABLE `DesignerProfiles` ADD `Notes` longtext NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    ALTER TABLE `ClientProfiles` ADD `Notes` longtext NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    CREATE TABLE `AuditLogs` (
        `Id` char(36) NOT NULL,
        `EntityType` varchar(50) NOT NULL,
        `EntityId` char(36) NOT NULL,
        `Action` varchar(50) NOT NULL,
        `PreviousValue` longtext NULL,
        `NewValue` longtext NULL,
        `PerformedByUserId` char(36) NULL,
        `PerformedByRole` varchar(50) NULL,
        `Timestamp` datetime(6) NOT NULL,
        `Notes` varchar(1000) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_AuditLogs` PRIMARY KEY (`Id`)
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:56:59.854292'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:56:59.854292'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:56:59.854292'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:56:59.854298'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:56:59.854298'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:56:59.854298'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:56:59.854298'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:56:59.854298'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:56:59.854298'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:56:59.854299'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:56:59.854299'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:56:59.854299'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:56:59.854299'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:56:59.854299'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:56:59.854299'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:56:59.855038'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:56:59.855038'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:56:59.855038'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-10 19:56:59.855038'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    CREATE INDEX `IX_AuditLogs_EntityType_EntityId` ON `AuditLogs` (`EntityType`, `EntityId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    CREATE INDEX `IX_AuditLogs_PerformedByUserId` ON `AuditLogs` (`PerformedByUserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    CREATE INDEX `IX_AuditLogs_Timestamp` ON `AuditLogs` (`Timestamp`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260210195700_AddGapFillingFeatures') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260210195700_AddGapFillingFeatures', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    ALTER TABLE LogoFiles ADD COLUMN FileCategory int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    ALTER TABLE LogoFiles ADD COLUMN FileStatus int NOT NULL DEFAULT 1;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 12:28:13.724711'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 12:28:13.724715'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 12:28:13.724715'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 12:28:13.724716'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 12:28:13.724716'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 12:28:13.724716'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 12:28:13.724717'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 12:28:13.724717'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 12:28:13.724717'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 12:28:13.724717'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 12:28:13.724719'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 12:28:13.724719'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 12:28:13.724719'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 12:28:13.72472'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 12:28:13.72472'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-12 12:28:13.726206'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-12 12:28:13.726208'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-12 12:28:13.726208'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-12 12:28:13.726209'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212122815_AddFileCategoryAndStatus') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260212122815_AddFileCategoryAndStatus', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    ALTER TABLE `LogoOrders` ADD `AllowUploads` tinyint(1) NOT NULL DEFAULT TRUE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 17:47:39.863821'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 17:47:39.863821'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 17:47:39.863822'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 17:47:39.863822'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 17:47:39.863822'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 17:47:39.863823'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 17:47:39.863823'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 17:47:39.863823'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 17:47:39.863824'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 17:47:39.863824'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 17:47:39.863824'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 17:47:39.863825'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 17:47:39.863825'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 17:47:39.863825'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 17:47:39.863826'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-12 17:47:39.864881'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-12 17:47:39.864881'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-12 17:47:39.864882'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-12 17:47:39.864882'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212174741_AddAllowUploadsToLogoOrder') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260212174741_AddAllowUploadsToLogoOrder', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    CREATE TABLE `Payments` (
        `Id` char(36) NOT NULL,
        `InvoiceId` char(36) NOT NULL,
        `PaymentMethod` varchar(50) NOT NULL,
        `Amount` decimal(18,2) NOT NULL,
        `Currency` varchar(10) NOT NULL DEFAULT 'USD',
        `Status` int NOT NULL DEFAULT 0,
        `PaymentLink` varchar(1000) NULL,
        `TransactionId` varchar(200) NULL,
        `CompletedAt` datetime(6) NULL,
        `ErrorMessage` varchar(1000) NULL,
        `ReturnUrl` varchar(500) NULL,
        `CancelUrl` varchar(500) NULL,
        `ExpiresAt` datetime(6) NULL,
        `InvoiceId1` char(36) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) NULL,
        `UpdatedBy` char(36) NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) NULL,
        CONSTRAINT `PK_Payments` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Payments_Invoices_InvoiceId` FOREIGN KEY (`InvoiceId`) REFERENCES `Invoices` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Payments_Invoices_InvoiceId1` FOREIGN KEY (`InvoiceId1`) REFERENCES `Invoices` (`Id`)
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 18:22:55.056168'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 18:22:55.056169'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 18:22:55.056169'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 18:22:55.056169'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 18:22:55.056169'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 18:22:55.05617'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 18:22:55.05617'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 18:22:55.05617'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 18:22:55.05617'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 18:22:55.056171'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 18:22:55.056171'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 18:22:55.056171'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 18:22:55.056172'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 18:22:55.056172'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-02-12 18:22:55.056173'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-12 18:22:55.057138'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-12 18:22:55.057139'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-12 18:22:55.057139'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-02-12 18:22:55.057139'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    CREATE INDEX `IX_Payments_CreatedAt` ON `Payments` (`CreatedAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    CREATE INDEX `IX_Payments_InvoiceId` ON `Payments` (`InvoiceId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    CREATE INDEX `IX_Payments_InvoiceId1` ON `Payments` (`InvoiceId1`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    CREATE INDEX `IX_Payments_Status` ON `Payments` (`Status`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    CREATE INDEX `IX_Payments_TransactionId` ON `Payments` (`TransactionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260212182256_AddPaymentEntity') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260212182256_AddPaymentEntity', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305122820_AddIsRootAdminToUser') THEN

    ALTER TABLE `Users` ADD `IsRootAdmin` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305122820_AddIsRootAdminToUser') THEN


                    UPDATE Users u
                    SET u.IsRootAdmin = 1
                    WHERE u.Id = (
                        SELECT Id FROM (
                            SELECT u2.Id FROM Users u2
                            INNER JOIN Roles r ON u2.RoleId = r.Id
                            WHERE r.Name = 'SuperAdmin' AND u2.IsDeleted = 0
                            ORDER BY u2.CreatedAt ASC
                            LIMIT 1
                        ) tmp
                    );
                

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260305122820_AddIsRootAdminToUser') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260305122820_AddIsRootAdminToUser', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    ALTER TABLE `Notifications` ADD `ReferenceId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    ALTER TABLE `Notifications` ADD `ReferenceType` int NOT NULL DEFAULT 1;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 00:00:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 00:00:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 00:00:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 00:00:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 00:00:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 00:00:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 00:00:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 00:00:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 00:00:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 00:00:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 00:00:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 00:00:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 00:00:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 00:00:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 00:00:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 00:00:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 00:00:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 00:00:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 00:00:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306000009_AddNotificationReferenceTypeAndReferenceId') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260306000009_AddNotificationReferenceTypeAndReferenceId', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    CREATE INDEX `IX_Notifications_UserId_CreatedAt` ON `Notifications` (`UserId`, `CreatedAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    CREATE INDEX `IX_Notifications_UserId_ReferenceType_CreatedAt` ON `Notifications` (`UserId`, `ReferenceType`, `CreatedAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:12:06'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:12:06'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:12:06'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:12:06'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:12:06'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:12:06'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:12:06'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:12:06'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:12:06'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:12:06'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:12:06'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:12:06'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:12:06'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:12:06'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:12:06'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:12:06'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:12:06'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:12:06'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:12:06'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011208_AddNotificationIndexes') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260306011208_AddNotificationIndexes', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    ALTER TABLE `Notifications` ADD `AggregationCount` int NOT NULL DEFAULT 1;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    ALTER TABLE `Notifications` ADD `LastOccurrenceAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:15:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:15:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:15:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:15:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:15:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:15:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:15:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:15:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:15:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:15:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:15:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:15:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:15:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:15:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:15:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:15:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:15:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:15:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 01:15:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306011504_AddNotificationAggregationFields') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260306011504_AddNotificationAggregationFields', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    ALTER TABLE `Messages` ADD `ForwardedByAdmin` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    ALTER TABLE `Messages` ADD `ForwardedToMessageId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    ALTER TABLE `Messages` ADD `IsRejected` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    ALTER TABLE `Messages` ADD `OriginalSenderRole` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    ALTER TABLE `Messages` ADD `RejectedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    ALTER TABLE `Messages` ADD `RejectedBy` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    ALTER TABLE `Messages` ADD `RequiresAdminApproval` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 10:03:00'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 10:03:00'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 10:03:00'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 10:03:00'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 10:03:00'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 10:03:00'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 10:03:00'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 10:03:00'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 10:03:00'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 10:03:00'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 10:03:00'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 10:03:00'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 10:03:00'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 10:03:00'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 10:03:00'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 10:03:00'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 10:03:00'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 10:03:00'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 10:03:00'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306100301_AddMessageAdminRelayFields') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260306100301_AddMessageAdminRelayFields', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    ALTER TABLE `LogoFiles` ADD `PreviewBatchId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:09:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:09:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:09:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:09:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:09:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:09:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:09:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:09:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:09:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:09:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:09:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:09:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:09:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:09:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:09:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:09:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:09:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:09:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:09:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306110947_AddPreviewBatchIdToLogoFile') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260306110947_AddPreviewBatchIdToLogoFile', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:50:27'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:50:27'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:50:27'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:50:27'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:50:27'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:50:27'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:50:27'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:50:27'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:50:27'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:50:27'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:50:27'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:50:27'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:50:27'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:50:27'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:50:27'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:50:27'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:50:27'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:50:27'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 11:50:27'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    CREATE INDEX `IX_LogoFiles_PreviewBatchId` ON `LogoFiles` (`PreviewBatchId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306115028_AddPreviewBatchIdIndexToLogoFile') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260306115028_AddPreviewBatchIdIndexToLogoFile', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    ALTER TABLE `Notifications` ADD `RedirectUrl` varchar(500) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 13:22:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 13:22:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 13:22:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 13:22:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 13:22:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 13:22:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 13:22:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 13:22:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 13:22:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 13:22:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 13:22:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 13:22:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 13:22:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 13:22:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-06 13:22:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 13:22:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 13:22:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 13:22:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-06 13:22:03'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260306132204_AddRedirectUrlToNotification') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260306132204_AddRedirectUrlToNotification', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:17:32'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:17:32'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:17:32'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:17:32'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:17:32'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:17:32'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:17:32'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:17:32'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:17:32'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:17:32'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:17:32'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:17:32'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:17:32'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:17:32'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:17:32'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:17:32'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:17:32'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:17:32'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:17:32'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    CREATE INDEX `IX_LogoOrders_ClientId_CreatedAt` ON `LogoOrders` (`ClientId`, `CreatedAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    CREATE INDEX `IX_LogoOrders_CreatedAt` ON `LogoOrders` (`CreatedAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    CREATE INDEX `IX_LogoOrders_DesignerId_Status` ON `LogoOrders` (`DesignerId`, `Status`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    CREATE INDEX `IX_LogoOrders_Status` ON `LogoOrders` (`Status`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    CREATE INDEX `IX_LogoOrders_Status_CreatedAt` ON `LogoOrders` (`Status`, `CreatedAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    CREATE INDEX `IX_LogoOrders_UpdatedAt` ON `LogoOrders` (`UpdatedAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308031733_AddAnalyticsIndexesToLogoOrder') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260308031733_AddAnalyticsIndexesToLogoOrder', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:47:58'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:47:58'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:47:58'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:47:58'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:47:58'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:47:58'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:47:58'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:47:58'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:47:58'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:47:58'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:47:58'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:47:58'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:47:58'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:47:58'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:47:58'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:47:58'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:47:58'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:47:58'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 03:47:58'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    CREATE INDEX `IX_LogoOrders_ClientId_Status` ON `LogoOrders` (`ClientId`, `Status`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    CREATE INDEX `IX_Invoices_ClientId_Status` ON `Invoices` (`ClientId`, `Status`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    CREATE INDEX `IX_Invoices_PaidDate` ON `Invoices` (`PaidDate`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    CREATE INDEX `IX_Invoices_Status` ON `Invoices` (`Status`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308034759_AddClientAnalyticsIndexToLogoOrder') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260308034759_AddClientAnalyticsIndexToLogoOrder', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN


                    UPDATE LogoOrders
                    SET Status = 14
                    WHERE Status IN (10, 11, 12, 16, 17)
                    AND IsDeleted = 0;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:10:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:10:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:10:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:10:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:10:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:10:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:10:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:10:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:10:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:10:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:10:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:10:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:10:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:10:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:10:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:10:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:10:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:10:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:10:07'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    ALTER TABLE `LogoOrders` ADD `AllowExtraRevisions` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    ALTER TABLE `LogoOrders` ADD `RevisionCount` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    ALTER TABLE `LogoOrders` ADD `RevisionLimit` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:22:28'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212230_AddRevisionTrackingToLogoOrder') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260308212230_AddRevisionTrackingToLogoOrder', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    ALTER TABLE `OrderComments` ADD `CommentType` int NOT NULL DEFAULT 1;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    ALTER TABLE `OrderComments` ADD `IsReadByAdmin` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    ALTER TABLE `OrderComments` ADD `IsReadByClient` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    ALTER TABLE `OrderComments` ADD `IsReadByDesigner` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    ALTER TABLE `OrderComments` ADD `VisibleToClient` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:29:54'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:29:54'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:29:54'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:29:54'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:29:54'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:29:54'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:29:54'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:29:54'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:29:54'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:29:54'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:29:54'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:29:54'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:29:54'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:29:54'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:29:54'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:29:54'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:29:54'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:29:54'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:29:54'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308212956_AddOrderCommentSecurityAndVisibility') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260308212956_AddOrderCommentSecurityAndVisibility', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    UPDATE OrderComments SET VisibleToClient = 1 WHERE IsInternal = 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:36:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:36:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:36:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:36:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:36:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:36:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:36:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:36:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:36:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:36:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:36:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:36:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:36:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:36:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:36:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:36:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:36:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:36:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 21:36:46'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308213647_SetVisibleToClientForExistingNonInternalComments') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260308213647_SetVisibleToClientForExistingNonInternalComments', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    ALTER TABLE `LogoOrders` ADD `BillingEligible` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    ALTER TABLE `LogoOrders` ADD `CompletedDate` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    ALTER TABLE `LogoOrders` ADD `InvoiceId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    ALTER TABLE `LogoOrders` ADD `IsInvoiced` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    ALTER TABLE `Invoices` ADD `BillingPeriod` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    ALTER TABLE `ClientProfiles` ADD `BillingType` int NOT NULL DEFAULT 1;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:30:34'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:30:34'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:30:34'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:30:34'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:30:34'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:30:34'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:30:34'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:30:34'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:30:34'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:30:34'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:30:34'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:30:34'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:30:34'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:30:34'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:30:34'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:30:34'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:30:34'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:30:34'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:30:34'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    CREATE INDEX `IX_LogoOrders_ClientId_BillingEligible_IsInvoiced` ON `LogoOrders` (`ClientId`, `BillingEligible`, `IsInvoiced`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN


                    UPDATE LogoOrders o
                    INNER JOIN (SELECT OrderId, MIN(InvoiceId) as InvoiceId FROM InvoiceOrders WHERE OrderId IS NOT NULL AND IsDeleted = 0 GROUP BY OrderId) io ON io.OrderId = o.Id
                    SET o.BillingEligible = 1, o.IsInvoiced = 1, o.CompletedDate = COALESCE(o.UpdatedAt, o.CreatedAt), o.InvoiceId = io.InvoiceId
                    WHERE o.Status = 7 AND o.IsDeleted = 0;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN


                    UPDATE LogoOrders o
                    LEFT JOIN (SELECT DISTINCT OrderId FROM InvoiceOrders WHERE OrderId IS NOT NULL AND IsDeleted = 0) io ON io.OrderId = o.Id
                    SET o.BillingEligible = 1, o.CompletedDate = COALESCE(o.UpdatedAt, o.CreatedAt)
                    WHERE o.Status = 7 AND o.IsDeleted = 0 AND io.OrderId IS NULL;


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308223039_AddBillingQueueAndClientBillingType') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260308223039_AddBillingQueueAndClientBillingType', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    ALTER TABLE `LogoOrders` ADD `ApprovedPrice` decimal(18,2) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    ALTER TABLE `LogoOrders` ADD `DesignCategory` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    ALTER TABLE `LogoOrders` ADD `DesignType` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    ALTER TABLE `LogoOrders` ADD `DesignerInvoiceId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    ALTER TABLE `LogoOrders` ADD `IsDesignerInvoiced` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    ALTER TABLE `LogoOrders` ADD `PriceApprovalStatus` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    CREATE TABLE `DesignerInvoices` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `DesignerId` char(36) COLLATE ascii_general_ci NOT NULL,
        `InvoiceNumber` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `TotalAmount` decimal(18,2) NOT NULL,
        `Status` int NOT NULL DEFAULT 0,
        `BillingPeriod` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `IssueDate` datetime(6) NOT NULL,
        `PaidDate` datetime(6) NULL,
        `Notes` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_DesignerInvoices` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_DesignerInvoices_DesignerProfiles_DesignerId` FOREIGN KEY (`DesignerId`) REFERENCES `DesignerProfiles` (`Id`) ON DELETE RESTRICT
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    CREATE TABLE `DesignerInvoiceItems` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `DesignerInvoiceId` char(36) COLLATE ascii_general_ci NOT NULL,
        `OrderId` char(36) COLLATE ascii_general_ci NOT NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `Amount` decimal(18,2) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_DesignerInvoiceItems` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_DesignerInvoiceItems_DesignerInvoices_DesignerInvoiceId` FOREIGN KEY (`DesignerInvoiceId`) REFERENCES `DesignerInvoices` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_DesignerInvoiceItems_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `LogoOrders` (`Id`) ON DELETE RESTRICT
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:46:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:46:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:46:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:46:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:46:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:46:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:46:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:46:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:46:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:46:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:46:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:46:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:46:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:46:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:46:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:46:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:46:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:46:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 22:46:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    CREATE INDEX `IX_LogoOrders_DesignerId_IsDesignerInvoiced` ON `LogoOrders` (`DesignerId`, `IsDesignerInvoiced`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    CREATE INDEX `IX_LogoOrders_DesignerInvoiceId` ON `LogoOrders` (`DesignerInvoiceId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    CREATE UNIQUE INDEX `IX_DesignerInvoiceItems_DesignerInvoiceId_OrderId` ON `DesignerInvoiceItems` (`DesignerInvoiceId`, `OrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    CREATE INDEX `IX_DesignerInvoiceItems_OrderId` ON `DesignerInvoiceItems` (`OrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    CREATE INDEX `IX_DesignerInvoices_DesignerId_BillingPeriod` ON `DesignerInvoices` (`DesignerId`, `BillingPeriod`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    CREATE UNIQUE INDEX `IX_DesignerInvoices_InvoiceNumber` ON `DesignerInvoices` (`InvoiceNumber`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    ALTER TABLE `LogoOrders` ADD CONSTRAINT `FK_LogoOrders_DesignerInvoices_DesignerInvoiceId` FOREIGN KEY (`DesignerInvoiceId`) REFERENCES `DesignerInvoices` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308224609_AddDesignerPayoutAndPricing') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260308224609_AddDesignerPayoutAndPricing', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

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

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 23:28:17'
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

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 23:28:17'
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

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 23:28:17'
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

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 23:28:17'
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

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 23:28:17'
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

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 23:28:17'
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

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 23:28:17'
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

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 23:28:17'
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

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 23:28:17'
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

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 23:28:17'
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

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 23:28:17'
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

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 23:28:17'
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

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 23:28:17'
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

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 23:28:17'
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

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-08 23:28:17'
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

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 23:28:17'
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

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 23:28:17'
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

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 23:28:17'
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

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-08 23:28:17'
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

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    CREATE TABLE `DesignPricings` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
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
        CONSTRAINT `PK_DesignPricings` PRIMARY KEY (`Id`)
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 01:01:05'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 01:01:05'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 01:01:05'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 01:01:05'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 01:01:05'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 01:01:05'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 01:01:05'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 01:01:05'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 01:01:05'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 01:01:05'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 01:01:05'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 01:01:05'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 01:01:05'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 01:01:05'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 01:01:05'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-09 01:01:05'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-09 01:01:05'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-09 01:01:05'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-09 01:01:05'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    CREATE UNIQUE INDEX `IX_DesignPricings_DesignCategory_DesignType` ON `DesignPricings` (`DesignCategory`, `DesignType`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    INSERT INTO `DesignPricings` (`Id`, `DesignCategory`, `DesignType`, `DefaultPrice`, `IsActive`, `CreatedAt`, `IsDeleted`)
    VALUES ('a1000001-0000-0000-0000-000000000001', 1, 1, 350.0, TRUE, TIMESTAMP '2026-03-09 01:01:05', FALSE),
    ('a1000001-0000-0000-0000-000000000002', 1, 2, 700.0, TRUE, TIMESTAMP '2026-03-09 01:01:05', FALSE),
    ('a1000001-0000-0000-0000-000000000003', 2, 3, 350.0, TRUE, TIMESTAMP '2026-03-09 01:01:05', FALSE),
    ('a1000001-0000-0000-0000-000000000004', 2, 4, 0.0, TRUE, TIMESTAMP '2026-03-09 01:01:05', FALSE);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309010106_AddDesignPricingTable') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260309010106_AddDesignPricingTable', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    ALTER TABLE `LogoOrders` ADD `InvoicedDate` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 16:31:21'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 16:31:21'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 16:31:21'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 16:31:21'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 16:31:21'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 16:31:21'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 16:31:21'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 16:31:21'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 16:31:21'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 16:31:21'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 16:31:21'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 16:31:21'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 16:31:21'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 16:31:21'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 16:31:21'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-09 16:31:21'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-09 16:31:21'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-09 16:31:21'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-09 16:31:21'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309163126_AddInvoicedDateToLogoOrder') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260309163126_AddInvoicedDateToLogoOrder', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN


                    SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'LogoOrders' AND COLUMN_NAME = 'ClientPrice');
                    SET @sql = IF(@col_exists = 0, 'ALTER TABLE LogoOrders ADD COLUMN ClientPrice DECIMAL(18,2) NULL', 'SELECT 1');
                    PREPARE stmt FROM @sql;
                    EXECUTE stmt;
                    DEALLOCATE PREPARE stmt;
                    SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'LogoOrders' AND COLUMN_NAME = 'CurrencyCode');
                    SET @sql = IF(@col_exists = 0, 'ALTER TABLE LogoOrders ADD COLUMN CurrencyCode VARCHAR(3) NULL CHARACTER SET utf8mb4', 'SELECT 1');
                    PREPARE stmt FROM @sql;
                    EXECUTE stmt;
                    DEALLOCATE PREPARE stmt;
                

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    CREATE TABLE `ClientLogoPricings` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `ClientId` char(36) NOT NULL,
        `DesignCategory` int NOT NULL,
        `DesignType` int NOT NULL,
        `Price` decimal(18,2) NOT NULL,
        `CurrencyCode` varchar(3) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'USD',
        `IsActive` tinyint(1) NOT NULL DEFAULT TRUE,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        `CreatedBy` char(36) COLLATE ascii_general_ci NULL,
        `UpdatedBy` char(36) COLLATE ascii_general_ci NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_ClientLogoPricings` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ClientLogoPricings_ClientProfiles_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `ClientProfiles` (`Id`) ON DELETE RESTRICT
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 18:17:29'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 18:17:29'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 18:17:29'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 18:17:29'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 18:17:29'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 18:17:29'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 18:17:29'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 18:17:29'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 18:17:29'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 18:17:29'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 18:17:29'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 18:17:29'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 18:17:29'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 18:17:29'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 18:17:29'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-09 18:17:29'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-09 18:17:29'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-09 18:17:29'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-09 18:17:29'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    CREATE UNIQUE INDEX `IX_ClientLogoPricings_ClientId_DesignCategory_DesignType` ON `ClientLogoPricings` (`ClientId`, `DesignCategory`, `DesignType`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309181731_AddClientLogoPricingAndOrderFields') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260309181731_AddClientLogoPricingAndOrderFields', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309214804_AddSoftDeleteFiltersAndIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 21:48:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309214804_AddSoftDeleteFiltersAndIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 21:48:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309214804_AddSoftDeleteFiltersAndIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 21:48:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309214804_AddSoftDeleteFiltersAndIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 21:48:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309214804_AddSoftDeleteFiltersAndIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 21:48:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309214804_AddSoftDeleteFiltersAndIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 21:48:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309214804_AddSoftDeleteFiltersAndIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 21:48:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309214804_AddSoftDeleteFiltersAndIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 21:48:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309214804_AddSoftDeleteFiltersAndIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 21:48:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309214804_AddSoftDeleteFiltersAndIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 21:48:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309214804_AddSoftDeleteFiltersAndIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 21:48:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309214804_AddSoftDeleteFiltersAndIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 21:48:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309214804_AddSoftDeleteFiltersAndIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 21:48:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309214804_AddSoftDeleteFiltersAndIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 21:48:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309214804_AddSoftDeleteFiltersAndIndexes') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-09 21:48:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309214804_AddSoftDeleteFiltersAndIndexes') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-09 21:48:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309214804_AddSoftDeleteFiltersAndIndexes') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-09 21:48:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309214804_AddSoftDeleteFiltersAndIndexes') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-09 21:48:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309214804_AddSoftDeleteFiltersAndIndexes') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-09 21:48:02'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260309214804_AddSoftDeleteFiltersAndIndexes') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260309214804_AddSoftDeleteFiltersAndIndexes', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    UPDATE LogoOrders SET CurrencyCode = 'USD' WHERE CurrencyCode IS NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    ALTER TABLE `LogoOrders` MODIFY COLUMN `CurrencyCode` varchar(3) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'USD';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    ALTER TABLE `LogoOrders` ADD `ClientBasePrice` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    ALTER TABLE `LogoOrders` ADD `ClientChargePrice` decimal(18,2) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    ALTER TABLE `LogoOrders` ADD `DesignerApprovedPrice` decimal(18,2) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    ALTER TABLE `LogoOrders` ADD `DesignerProposedPrice` decimal(18,2) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN


                    UPDATE LogoOrders
                    SET ClientBasePrice = COALESCE(ClientPrice, Price, 0),
                        ClientChargePrice = COALESCE(ClientPrice, Price, 0),
                        DesignerProposedPrice = ProposedPrice,
                        DesignerApprovedPrice = ApprovedPrice;
                

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 00:33:47'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 00:33:47'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 00:33:47'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 00:33:47'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 00:33:47'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 00:33:47'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 00:33:47'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 00:33:47'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 00:33:47'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 00:33:47'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 00:33:47'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 00:33:47'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 00:33:47'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 00:33:47'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 00:33:47'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-10 00:33:47'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-10 00:33:47'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-10 00:33:47'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-10 00:33:47'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310003349_AddPricingWorkflowFields') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260310003349_AddPricingWorkflowFields', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    ALTER TABLE `LogoOrders` ADD `PriceUpdatedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    ALTER TABLE `LogoOrders` ADD `PriceUpdatedByRole` varchar(50) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    ALTER TABLE `LogoOrders` ADD `PriceUpdatedByUserId` char(36) COLLATE ascii_general_ci NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 19:24:24'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 19:24:24'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 19:24:24'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 19:24:24'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 19:24:24'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 19:24:24'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 19:24:24'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 19:24:24'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 19:24:24'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 19:24:24'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 19:24:24'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 19:24:24'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 19:24:24'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 19:24:24'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    UPDATE `Permissions` SET `CreatedAt` = TIMESTAMP '2026-03-10 19:24:24'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-10 19:24:24'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-10 19:24:24'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-10 19:24:24'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    UPDATE `Roles` SET `CreatedAt` = TIMESTAMP '2026-03-10 19:24:24'
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260310192426_AddPriceUpdatedTrackingFields') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260310192426_AddPriceUpdatedTrackingFields', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

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

