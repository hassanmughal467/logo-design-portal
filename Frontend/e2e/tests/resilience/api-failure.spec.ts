import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';

/**
 * Simulates a hard API failure after login shell loads — ensures the app does not hard-crash the browser.
 */
test('dashboard survives orders API returning 503 (graceful degradation)', async ({ page }) => {
  await page.route('**/api/orders**', (route) =>
    route.fulfill({
      status: 503,
      contentType: 'application/json',
      body: JSON.stringify({ error: 'simulated outage' }),
    })
  );

  const login = new LoginPage(page);
  await login.goto();
  await login.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
  await login.expectRedirectToDashboard();
  await expect(page).toHaveURL(/dashboard/i, { timeout: 15_000 });
});
