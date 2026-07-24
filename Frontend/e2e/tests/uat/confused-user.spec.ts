import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { expectNoConsoleErrors, randomHumanDelay, trackConsoleErrors } from '../../utils/human-behavior';
import { provisionAdminForUi, teardownUsers } from '../../utils/api-helpers';

test('UAT - confused user random navigation does not crash app', async ({ page, request }, testInfo) => {
  // Dedicated admin account: this test holds a live, navigating session for its whole duration
  // and must not share a refresh-token row with any other concurrently-running admin session.
  const admin = await provisionAdminForUi(request, testInfo);
  try {
    const errors = trackConsoleErrors(page);
    const login = new LoginPage(page);
    await login.goto();
    await login.login(admin.email, admin.password);
    await login.expectRedirectToDashboard();

    const routes = ['/orders', '/invoices', '/dashboard', '/users', '/orders'];
    for (const route of routes) {
      await page.goto(route);
      await randomHumanDelay();
      await page.keyboard.press('Escape');
      await randomHumanDelay(60, 240);
    }

    await expect(page.locator('body')).toBeVisible();
    await expectNoConsoleErrors(errors);
  } finally {
    await teardownUsers(request, [admin.userId]);
  }
});
