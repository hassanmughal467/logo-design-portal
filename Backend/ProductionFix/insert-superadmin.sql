-- =============================================================================
-- Insert SuperAdmin user (run when Users table is empty)
-- =============================================================================
-- Login: superadmin@logodesign.com / SuperAdmin@123
-- =============================================================================

USE LogoDesignPortalDb;

INSERT INTO Users (Id, Email, FirstName, LastName, PasswordHash, IsActive, RoleId, CreatedAt, IsDeleted)
VALUES (
  'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
  'superadmin@logodesign.com',
  'Super',
  'Admin',
  '$2a$11$vqnzOWz44wOmxRg20XTcHOEYmfWb3bnVW3z3g/AA/8VB941FvR9We',
  1,
  '11111111-1111-1111-1111-111111111111',
  UTC_TIMESTAMP(),
  0
);

SELECT 'SuperAdmin created. Login: superadmin@logodesign.com / SuperAdmin@123' AS Result;
