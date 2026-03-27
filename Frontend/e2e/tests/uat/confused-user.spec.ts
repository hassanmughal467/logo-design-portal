import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';
import { expectNoConsoleErrors, randomHumanDelay, trackConsoleErrors } from '../../utils/human-behavior';

test('UAT - confused user random navigation does not crash app', async ({ page }) => {
  const errors = trackConsoleErrors(page);
  const login = new LoginPage(page);
  await login.goto();
  await login.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
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
});
