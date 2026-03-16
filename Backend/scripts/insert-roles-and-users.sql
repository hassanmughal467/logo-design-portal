-- =============================================================================
-- Insert roles and users into LogoDesignPortalDb
-- =============================================================================
-- Run: mysql -u root -p LogoDesignPortalDb < insert-roles-and-users.sql
-- =============================================================================

USE LogoDesignPortalDb;

-- 1. ROLES (run first - users reference RoleId)
INSERT INTO `roles` (
  `Id`, `Name`, `Description`, `CreatedAt`, `UpdatedAt`,
  `CreatedBy`, `UpdatedBy`, `IsDeleted`, `DeletedAt`, `DeletedBy`
) VALUES
('11111111-1111-1111-1111-111111111111', 'SuperAdmin', 'Full system access with all permissions', NOW(6), NULL, NULL, NULL, 0, NULL, NULL),
('22222222-2222-2222-2222-222222222222', 'Admin', 'Administrative access with restricted client data access', NOW(6), NULL, NULL, NULL, 0, NULL, NULL),
('33333333-3333-3333-3333-333333333333', 'Designer', 'Designer access without client identity information', NOW(6), NULL, NULL, NULL, 0, NULL, NULL),
('44444444-4444-4444-4444-444444444444', 'Client', 'Client access to their own data', NOW(6), NULL, NULL, NULL, 0, NULL, NULL)
ON DUPLICATE KEY UPDATE
  `Name` = VALUES(`Name`),
  `Description` = VALUES(`Description`);

-- 2. USERS
INSERT INTO `users` (
  `Id`, `Email`, `FirstName`, `LastName`, `PasswordHash`, `IsActive`,
  `RefreshToken`, `RefreshTokenExpiryTime`, `RoleId`, `CreatedAt`, `UpdatedAt`,
  `CreatedBy`, `UpdatedBy`, `IsDeleted`, `DeletedAt`, `DeletedBy`,
  `PasswordResetToken`, `PasswordResetTokenExpiryTime`, `InvoiceEmail`, `SecondaryEmail`,
  `DeactivatedAt`, `DeactivatedBy`, `FailedLoginAttempts`, `LockoutEnd`, `IsRootAdmin`
) VALUES
('27250b41-8cb5-40f1-bef6-ef4e114062a3', 'admin@logodesign.com', 'hammad', 'HMS', '$2a$11$1AZPidYDWpt3zkBxcen5.OsMlh.JEb6zX1n3UNWCzYrjmhgfrI0nK', 1, 'DrmSIlRuuNgs5N/4BbkdPAf11UNw8hTKIeOBqVR13PDWPuCkgKUbnVjeTDYziBb+rNaPveNH4MDYuKex30/ycg==', '2026-03-08 01:48:55.517621', '22222222-2222-2222-2222-222222222222', '2026-03-07 17:29:47.103150', '2026-03-08 00:48:55.517781', NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, 0),
('69fceb33-43b5-4c3c-989a-7eddf859ceef', 'designer@logodesign.com', 'Azeem ', 'Kamal', '$2a$12$tYwLEdeFFVBV8xTlJAJwWu4QBwxHYhQzAccwk8MVS2biAQVPUu3RC', 1, 'PCBKB77Dq2OkS+wGHHM5N0VZM5xnxdZ6iKogpFko9bXPy7Z8ujkW4e3wRuRMYg5q54TbRGpSbG0t9nP2veXn+g==', '2026-03-17 00:59:44.011132', '33333333-3333-3333-3333-333333333333', '2026-03-06 17:25:27.200995', '2026-03-10 00:59:44.011304', NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, 0),
('bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6', 'client@logodesign.com', 'Kevin', 'sharma', '$2a$12$2lGHqBLIryZjyQhngMOTu.6xcsECLl70n8seT.mqWQO6b2.NbEG2a', 1, 'TAlBkKO9xqtxLc1UJCoI0rj4i070oX0FArnECQAIv2jADQZSlBU3YPH/IkbJp6qMn2NLjumXIHPOthJS+v4/Ew==', '2026-03-20 02:19:26.352439', '44444444-4444-4444-4444-444444444444', '2026-03-05 15:03:38.768447', '2026-03-13 02:19:26.420459', NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, 0),
('da6639fc-4721-4958-940d-bf3a910b434a', 'superadmin@logodesign.com', 'Super', 'Admin', '$2a$11$TrpJjAo.7BMKzTSxKULhhuIV6BOXtI/1lvGut3ETxleGjVTBJedbG', 1, 'tmzo7SGpQireq/v84RoPleUdXeAQ77mfFi9B2yYdpAwRwNoa4GMiMW+viQP2mmeRuCNCsS/5jDoWPMZxAKY1DQ==', '2026-03-23 16:03:41.168313', '11111111-1111-1111-1111-111111111111', '2026-02-24 23:08:29.624238', '2026-03-16 16:03:41.198530', NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, 1)
ON DUPLICATE KEY UPDATE
  `Email` = VALUES(`Email`),
  `FirstName` = VALUES(`FirstName`),
  `LastName` = VALUES(`LastName`),
  `PasswordHash` = VALUES(`PasswordHash`),
  `RefreshToken` = VALUES(`RefreshToken`),
  `RefreshTokenExpiryTime` = VALUES(`RefreshTokenExpiryTime`),
  `RoleId` = VALUES(`RoleId`),
  `UpdatedAt` = VALUES(`UpdatedAt`),
  `IsRootAdmin` = VALUES(`IsRootAdmin`);

SELECT '4 roles and 4 users inserted/updated successfully' AS Result;
