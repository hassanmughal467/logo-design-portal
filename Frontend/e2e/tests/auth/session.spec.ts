import { test, expect } from '@playwright/test';
import { LoginPage, MainLayoutPage } from '../../pom';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';

/**
 * Session lifecycle: authenticated shell should expose logout, and logging out returns to login.
 */
test('admin can log out from main layout', async ({ page }) => {
  const login = new LoginPage(page);
  const layout = new MainLayoutPage(page);
  await login.goto();
  await login.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
  await login.expectRedirectToDashboard();
  await layout.expectUserHeaderVisible();
  await layout.logout();
  await expect(page).toHaveURL(/\/(auth\/)?login/);
});
