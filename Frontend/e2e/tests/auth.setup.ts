import { test as setup } from '@playwright/test';
import path from 'path';
import fs from 'fs';
import { LoginPage, MainLayoutPage } from '../pom';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD, E2E_API_URL } from '../utils/env';
import { apiUrl } from '../utils/api-client';
import { provisionAdminForUi } from '../utils/api-helpers';

/**
 * Production-style session bootstrap: perform a real UI login once, then reuse `storageState`
 * for every test in the `admin-chromium` project (avoids redundant login and speeds suites).
 *
 * Output path must match `storageState` in `playwright.config.ts` (`e2e/.auth/admin.json`).
 */
const authFile = path.join(__dirname, '..', '.auth', 'admin.json');
// Records the dedicated admin's userId so `auth.teardown.ts` (runs once, after every project
// that depends on this `setup` project has finished — see `teardown: 'cleanup-admin'` in
// playwright.config.ts) can delete it. The account must stay valid for the whole suite, so it
// cannot be cleaned up inline here.
const adminMetaFile = path.join(__dirname, '..', '.auth', 'admin-meta.json');

setup('authenticate admin operator', async ({ page, request }, testInfo) => {
  const healthUrl = `${E2E_API_URL}/api/system/health`;
  let health: Awaited<ReturnType<typeof request.get>> | null = null;
  try {
    health = await request.get(healthUrl);
  } catch (e) {
    const msg = e instanceof Error ? e.message : String(e);
    throw new Error(
      `Cannot reach API health at ${healthUrl} (${msg}). ` +
        `Start the ASP.NET API on port 5000 and ensure MySQL matches ConnectionStrings in appsettings. ` +
        `If the API uses another port, set E2E_API_URL in Frontend/e2e/.env.`
    );
  }
  if (!health.ok()) {
    const body = (await health.text().catch(() => '')).slice(0, 800);
    throw new Error(
      `Backend health check returned ${health.status()} at ${healthUrl}. ` +
        `Response: ${body || '(empty)'} — fix API/DB, then re-run. ` +
        `Angular E2E must use \`ng serve --configuration=e2e\` so the UI calls the same host as E2E_API_URL.`
    );
  }

  const loginCheck = await request.post(apiUrl('/auth/login'), {
    data: { email: E2E_ADMIN_EMAIL, password: E2E_ADMIN_PASSWORD },
    headers: { 'Content-Type': 'application/json' },
  });
  if (loginCheck.status() === 401) {
    throw new Error(
      `Admin login failed for ${E2E_ADMIN_EMAIL}. On a fresh database the seeded account is superadmin@logodesign.com / SuperAdmin@123 — set E2E_ADMIN_EMAIL / E2E_ADMIN_PASSWORD in e2e/.env if you use different operators.`
    );
  }
  if (!loginCheck.ok()) {
    throw new Error(`Unexpected login response during setup: ${loginCheck.status()} ${await loginCheck.text()}`);
  }

  // Authenticate the storageState session as a dedicated, freshly-provisioned Admin account
  // rather than the seeded E2E_ADMIN_EMAIL/E2E_ADMIN_PASSWORD directly. That account is shared
  // by other tests that also hold a live UI session (see provisionAdminForUi) — every ongoing
  // session refreshing the same account's single refresh-token row would rotate the others out.
  const admin = await provisionAdminForUi(request, testInfo);

  fs.mkdirSync(path.dirname(authFile), { recursive: true });
  fs.writeFileSync(adminMetaFile, JSON.stringify({ userId: admin.userId }), 'utf-8');

  const loginPage = new LoginPage(page);
  await loginPage.goto();
  await loginPage.login(admin.email, admin.password);
  await loginPage.expectRedirectToDashboard();
  await new MainLayoutPage(page).expectUserHeaderVisible();

  await page.context().storageState({ path: authFile });
});
