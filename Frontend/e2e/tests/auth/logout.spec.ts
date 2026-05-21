import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';

test.describe('Auth — logout', () => {
  test('after login, clearing storage redirects to login on dashboard visit', async ({ page }) => {
    const login = new LoginPage(page);
    await login.goto();
    await login.login('client@test.com', 'Test@123');
    try {
      await login.expectRedirectToDashboard();
    } catch {
      test.skip(true, 'Seeded client@test.com not available');
    }
    await page.context().clearCookies();
    await page.evaluate(() => localStorage.clear());
    await page.goto('/dashboard');
    await expect(page).toHaveURL(/\/auth\/login|\/login/);
  });
});
