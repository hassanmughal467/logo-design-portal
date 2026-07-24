import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { provisionAdminForUi, teardownUsers } from '../../utils/api-helpers';

test('UAT - core pages have visible controls and readable content', async ({ page, request }, testInfo) => {
  // Dedicated admin account: avoids sharing a refresh-token row with other concurrently-running
  // admin sessions (see AuthService.RefreshTokenAsync rotation).
  const admin = await provisionAdminForUi(request, testInfo);
  try {
    const login = new LoginPage(page);
    await login.goto();
    await login.login(admin.email, admin.password);
    await login.expectRedirectToDashboard();

    await page.goto('/dashboard');
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
    await expect(page.locator('button:visible').first()).toBeVisible();

    await page.goto('/orders');
    await expect(page.getByRole('heading', { name: /Orders/i })).toBeVisible();
    // Admins cannot create client orders; they can add completed orders.
    await expect(page.getByTestId('orders-open-quick-completed')).toBeVisible();

    await page.goto('/invoices');
    await expect(page.getByText('Invoices Management')).toBeVisible();
    await expect(page.locator('body')).not.toContainText(/undefined|null/i);
  } finally {
    await teardownUsers(request, [admin.userId]);
  }
});
