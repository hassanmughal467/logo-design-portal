-- =============================================================================
-- Seed Roles (run AFTER schema-only import)
-- =============================================================================
-- Schema-only import creates empty tables. This script adds Roles.
-- After running: RECYCLE the IIS Application Pool. The API will then create
-- SuperAdmin on startup. Login with: superadmin@logodesign.com / SuperAdmin@123
-- =============================================================================

USE LogoDesignPortalDb;

-- Insert Roles (skip if already exist)
INSERT IGNORE INTO Roles (Id, Name, Description, CreatedAt, IsDeleted) VALUES
('11111111-1111-1111-1111-111111111111', 'SuperAdmin', 'Full system access with all permissions', UTC_TIMESTAMP(), 0),
('22222222-2222-2222-2222-222222222222', 'Admin', 'Administrative access with restricted client data access', UTC_TIMESTAMP(), 0),
('33333333-3333-3333-3333-333333333333', 'Designer', 'Designer access without client identity information', UTC_TIMESTAMP(), 0),
('44444444-4444-4444-4444-444444444444', 'Client', 'Client access to their own data', UTC_TIMESTAMP(), 0);

SELECT 'Roles seeded. Now RECYCLE IIS Application Pool - API will create SuperAdmin. Login: superadmin@logodesign.com / SuperAdmin@123' AS Result;
