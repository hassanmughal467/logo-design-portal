import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';

test.describe('Elite - visual regression', () => {
  test('dashboard visual baseline', async ({ page }) => {
    const login = new LoginPage(page);
    await login.goto();
    await login.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
    await login.expectRedirectToDashboard();
    await page.waitForLoadState('networkidle');
    await expect(page).toHaveScreenshot('dashboard-elite.png', { fullPage: true, animations: 'disabled' });
  });

  test('orders page visual baseline', async ({ page }) => {
    const login = new LoginPage(page);
    await login.goto();
    await login.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
    await login.expectRedirectToDashboard();
    await page.goto('/orders');
    await expect(page.getByRole('heading', { name: /Orders/i })).toBeVisible();
    await page.waitForLoadState('networkidle');
    await expect(page).toHaveScreenshot('orders-elite.png', { fullPage: true, animations: 'disabled' });
  });
});
