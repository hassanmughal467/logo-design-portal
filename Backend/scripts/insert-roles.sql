-- =============================================================================
-- Insert roles into LogoDesignPortalDb
-- =============================================================================
-- Run: mysql -u root -p LogoDesignPortalDb < insert-roles.sql
-- Or execute in MySQL Workbench / your DB client
-- =============================================================================

USE LogoDesignPortalDb;

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

SELECT '4 roles inserted/updated successfully' AS Result;
