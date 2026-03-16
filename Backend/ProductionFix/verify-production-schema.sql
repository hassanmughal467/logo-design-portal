-- =============================================================================
-- Verify Production Schema - Run this on your PRODUCTION database
-- =============================================================================
-- This checks if the required LogoOrders columns exist.
-- Run: mysql -h YOUR_PRODUCTION_HOST -u root -p LogoDesignPortalDb < verify-production-schema.sql
-- =============================================================================

USE LogoDesignPortalDb;

SELECT 'Checking LogoOrders columns...' AS Step;

SELECT COLUMN_NAME 
FROM information_schema.columns 
WHERE table_schema = 'LogoDesignPortalDb' 
  AND table_name = 'LogoOrders' 
  AND COLUMN_NAME IN (
    'ClientBasePrice', 
    'ClientChargePrice', 
    'DesignerProposedPrice', 
    'DesignerApprovedPrice', 
    'CurrencyCode', 
    'StandardPrice',
    'PriceApprovalStatus',
    'RequiresPriceApproval',
    'PriceApproved'
  )
ORDER BY COLUMN_NAME;

SELECT 'Expected: 9 rows. If fewer, run production-migration-fix.sql on THIS database.' AS Result;
