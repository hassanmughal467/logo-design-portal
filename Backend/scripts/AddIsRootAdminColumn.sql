-- Add IsRootAdmin column to Users table
-- Run this script manually if EF migration fails due to schema drift

-- Add the column (MySQL)
ALTER TABLE `Users` ADD COLUMN `IsRootAdmin` tinyint(1) NOT NULL DEFAULT 0;

-- Set the first SuperAdmin user as Root Admin
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
