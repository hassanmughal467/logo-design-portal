-- =============================================================================
-- Logo Design Portal - Wipe all data EXCEPT Users and Roles
-- Database: MySQL (Pomelo / EF Core table names)
-- =============================================================================
-- WARNING: Destructive. Backup first. Also clear uploaded files on disk if needed.
--
-- This removes Permissions and RolePermissions. The app normally expects those
-- rows (migration seed). After this run, restore permissions or re-apply seed
-- data, or authorization may deny most actions.
--
-- For a safer reset that keeps RBAC intact, use:
--   clean-database-keep-users-roles.sql
--
-- Usage (adjust database name):
--   mysql -u USER -p DB_NAME < clean-database-users-roles-only.sql
-- =============================================================================

SET FOREIGN_KEY_CHECKS = 0;
SET SQL_SAFE_UPDATES = 0;

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
DELETE FROM RolePermissions;
DELETE FROM Permissions;

SET SQL_SAFE_UPDATES = 1;
SET FOREIGN_KEY_CHECKS = 1;

SELECT 'Done. Only Users and Roles remain. Permissions/RolePermissions were cleared.' AS Status;
