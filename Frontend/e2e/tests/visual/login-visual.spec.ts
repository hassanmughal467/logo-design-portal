import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';

test.describe('Visual regression', () => {
  test('login page baseline', async ({ page }) => {
    const login = new LoginPage(page);
    await login.goto();
    await page.waitForLoadState('networkidle');
    await expect(page).toHaveScreenshot('login-shell.png', {
      fullPage: true,
      animations: 'disabled',
    });
  });
});
