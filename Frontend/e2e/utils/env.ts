/**
 * Central place for environment-derived configuration used by Playwright tests and helpers.
 * Values are supplied via `e2e/.env`, the shell, or CI workflow env — never hard-code secrets.
 */

/** Web origin where the Angular app is served (Playwright `baseURL`). */
export const E2E_BASE_URL = process.env.E2E_BASE_URL ?? 'http://localhost:4200';

/**
 * ASP.NET API root (no trailing `/api`). The frontend `environment.e2e.ts` should match this
 * so UI and `request` calls hit the same backend.
 */
export const E2E_API_URL = process.env.E2E_API_URL ?? 'http://localhost:5000';

/** Primary privileged operator for admin / SuperAdmin UI scenarios (seeded on fresh DB). */
export const E2E_ADMIN_EMAIL = process.env.E2E_ADMIN_EMAIL ?? 'superadmin@logodesign.com';
export const E2E_ADMIN_PASSWORD = process.env.E2E_ADMIN_PASSWORD ?? 'SuperAdmin@123';

/** Optional dedicated client account for faster repeat runs (skips provisioning in some tests). */
export const E2E_CLIENT_EMAIL = process.env.E2E_CLIENT_EMAIL;
export const E2E_CLIENT_PASSWORD = process.env.E2E_CLIENT_PASSWORD;
