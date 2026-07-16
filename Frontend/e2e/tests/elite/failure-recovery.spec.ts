import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';

test('Elite - API failure then recovery succeeds on retry', async ({ page }) => {
  const login = new LoginPage(page);
  await login.goto();
  await login.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
  await login.expectRedirectToDashboard();
  // Let dashboard finish its own /api/orders traffic before we inject failures.
  await page.waitForLoadState('networkidle');

  let intercepted = false;
  await page.route('**/api/orders**', async (route) => {
    if (!intercepted) {
      intercepted = true;
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ error: 'simulated first-request failure' }),
      });
      return;
    }
    await route.continue();
  });

  await page.goto('/orders');

  await expect(page.locator('body')).toContainText(/error|failed|unable|no orders/i, { timeout: 15_000 });
  await page.reload();
  await expect(page.getByRole('heading', { name: /Orders/i })).toBeVisible({ timeout: 30_000 });
  // Admins manage orders; Create Order is Client-only.
  await expect(page.getByTestId('orders-open-quick-completed')).toBeVisible();
});
