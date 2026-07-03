import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';

/**
 * Visual baselines compare the application shell (sidebar, navbar, page chrome).
 * Data-driven regions (router outlet content, notification badge) are masked —
 * order counts and rows change every run, so pixel-diffing them is pure noise.
 */
test.describe('Elite - visual regression', () => {
  const dynamicMasks = (page: import('@playwright/test').Page) => [
    page.locator('main.page-content'),
    page.locator('.top-navbar .p-badge'),
    // Real-time SignalR toasts (and the elements they overlap) can pop at any moment.
    page.locator('p-toast'),
    // Notification bell counter changes as background tests generate events.
    page.locator('.top-navbar'),
  ];

  // networkidle is unreliable here: SignalR keeps polling, so the page never goes idle.
  // The data regions are masked, so a short fixed settle for fonts/layout is enough.
  const settle = async (page: import('@playwright/test').Page) => {
    await page.evaluate(() => (document as Document & { fonts: FontFaceSet }).fonts.ready);
    await page.waitForTimeout(1_500);
  };

  test('dashboard visual baseline', async ({ page }) => {
    const login = new LoginPage(page);
    await login.goto();
    await login.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
    await login.expectRedirectToDashboard();
    await settle(page);
    await expect(page).toHaveScreenshot('dashboard-elite.png', {
      animations: 'disabled',
      mask: dynamicMasks(page),
    });
  });

  test('orders page visual baseline', async ({ page }) => {
    const login = new LoginPage(page);
    await login.goto();
    await login.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
    await login.expectRedirectToDashboard();
    await page.goto('/orders');
    await expect(page.getByRole('heading', { name: /Orders/i })).toBeVisible();
    await settle(page);
    await expect(page).toHaveScreenshot('orders-elite.png', {
      animations: 'disabled',
      mask: dynamicMasks(page),
    });
  });
});
