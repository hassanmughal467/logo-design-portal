import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';

test.describe('Elite - visual regression', () => {
  // Dashboard/orders pixels vary with seeded data and chart antialiasing across CI runs.
  // maxDiffPixels must be overridden here too - the global default (playwright.config.ts) is 1200,
  // which silently overrides maxDiffPixelRatio as the binding constraint on these larger screenshots.
  const shot = { fullPage: true as const, animations: 'disabled' as const, maxDiffPixelRatio: 0.04, maxDiffPixels: 40000 };

  test('dashboard visual baseline', async ({ page }) => {
    const login = new LoginPage(page);
    await login.goto();
    await login.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
    await login.expectRedirectToDashboard();
    await page.waitForLoadState('networkidle');
    await expect(page).toHaveScreenshot('dashboard-elite.png', shot);
  });

  test('orders page visual baseline', async ({ page }) => {
    const login = new LoginPage(page);
    await login.goto();
    await login.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
    await login.expectRedirectToDashboard();
    await page.goto('/orders');
    await expect(page.getByRole('heading', { name: /Orders/i })).toBeVisible();
    await page.waitForLoadState('networkidle');
    await expect(page).toHaveScreenshot('orders-elite.png', shot);
  });
});
