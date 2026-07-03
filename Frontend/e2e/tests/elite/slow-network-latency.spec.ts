import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';

test('Elite - slow network keeps UI responsive with loading indicator', async ({ page }) => {
  await page.route('**/api/orders**', async (route) => {
    await new Promise((resolve) => setTimeout(resolve, 1800));
    await route.continue();
  });

  const login = new LoginPage(page);
  await login.goto();
  await login.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
  await login.expectRedirectToDashboard();

  await page.goto('/orders');
  await expect(page.getByTestId('scoped-page-skeleton')).toBeVisible();
  await expect(page.getByRole('heading', { name: /Orders/i })).toBeVisible({ timeout: 30_000 });
  // Admins see "Add Completed Order" — Create Order is Client-only.
  await expect(page.getByTestId('orders-open-quick-completed')).toBeVisible();
});
