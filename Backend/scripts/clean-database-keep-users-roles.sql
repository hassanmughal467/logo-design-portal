-- =============================================================================
-- Logo Design Portal - Database Clean Script
-- DELETES ALL DATA - Users, Roles, Permissions, RolePermissions are preserved
-- Database: MySQL (LogoDesignPortalDb)
-- =============================================================================
-- WARNING: This permanently deletes all data. Backup first!
-- Usage: mysql -u root -p LogoDesignPortalDb < clean-database-keep-users-roles.sql
--
-- Use this to fix 500 errors after migrations caused by bad/orphaned data.
-- Preserves: Users, Roles, Permissions, RolePermissions (auth structure intact)
-- =============================================================================

USE LogoDesignPortalDb;

SET FOREIGN_KEY_CHECKS = 0;
SET SQL_SAFE_UPDATES = 0;

-- Delete ALL data from tables (preserve Users, Roles, Permissions, RolePermissions)
DELETE FROM AuditLogs;
DELETE FROM RevisionFiles;
DELETE FROM OrderRevisions;
DELETE FROM OrderComments;
DELETE FROM OrderLogs;
DELETE FROM OrderStatusHistories;
DELETE FROM LogoFiles;
DELETE FROM LogoOrders;
DELETE FROM Quotes;
DELETE FROM DesignerInvoiceAdjustments;
DELETE FROM DesignerInvoiceItems;
DELETE FROM DesignerInvoices;
DELETE FROM InvoiceOrders;
DELETE FROM InvoiceLogs;
DELETE FROM Payments;
DELETE FROM Invoices;
DELETE FROM ClientGalleries;
DELETE FROM ClientLogoPricings;
DELETE FROM DesignerLogoPricings;
DELETE FROM DesignerProfiles;
DELETE FROM ClientProfiles;
DELETE FROM Notifications;
DELETE FROM Messages;
DELETE FROM Reviews;
DELETE FROM DesignPricings;
DELETE FROM Settings;

SET SQL_SAFE_UPDATES = 1;
SET FOREIGN_KEY_CHECKS = 1;

SELECT 'Done. All data deleted. Users, Roles, Permissions, RolePermissions preserved.' AS Status;
