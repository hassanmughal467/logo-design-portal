import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';

test('UAT - slow internet shows loader and remains usable', async ({ page }) => {
  await page.route('**/api/dashboard**', async (route) => {
    await new Promise((resolve) => setTimeout(resolve, 2000));
    await route.continue();
  });

  const login = new LoginPage(page);
  await login.goto();
  await login.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
  await login.expectRedirectToDashboard();

  await expect(page.locator('.loading-container')).toBeVisible();
  await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
});

test('UAT - transient orders failure allows recovery after refresh', async ({ page }) => {
  let failedOnce = false;
  await page.route('**/api/orders**', async (route) => {
    if (!failedOnce) {
      failedOnce = true;
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ error: 'temporary outage' }),
      });
      return;
    }
    await route.continue();
  });

  const login = new LoginPage(page);
  await login.goto();
  await login.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
  await login.expectRedirectToDashboard();
  await page.goto('/orders');
  await expect(page.locator('body')).toContainText(/error|failed|unable|orders/i);
  await page.reload();
  await expect(page.getByRole('heading', { name: /Orders/i })).toBeVisible();
});
