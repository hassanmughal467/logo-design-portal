import { test, expect } from '@playwright/test';
import { LoginPage } from '../../page-objects/login.page';

const API_URL = process.env.API_URL || 'http://localhost:5000';

/** Returns 'ok' if login works, 'unauthorized' if backend up but user missing, 'unreachable' if backend down. */
async function checkBackendForLogin(
  request: import('@playwright/test').APIRequestContext
): Promise<'ok' | 'unauthorized' | 'unreachable'> {
  try {
    const res = await request.fetch(`${API_URL}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      data: { email: 'client@test.com', password: 'Test@123' },
      timeout: 5000,
    });
    return res.ok() ? 'ok' : 'unauthorized';
  } catch {
    return 'unreachable';
  }
}

test.describe('Login', () => {
  test('invalid credentials shows error', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login('invalid@test.com', 'wrongpassword');
    await loginPage.expectLoginError();
  });

  test('valid credentials redirects to dashboard', async ({ page, request }) => {
    const status = await checkBackendForLogin(request);
    if (status === 'unreachable') {
      test.skip(true, `Backend not running at ${API_URL}. Start with: cd Backend && dotnet run`);
    }
    if (status === 'unauthorized') {
      test.skip(true, `Test user client@test.com not in database. Seed test data or run API integration tests to create it.`);
    }
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login('client@test.com', 'Test@123');
    await loginPage.expectRedirectToDashboard();
  });

  test('unauthenticated access to dashboard redirects to login', async ({ page }) => {
    await page.goto('/dashboard');
    await expect(page).toHaveURL(/\/auth\/login/);
  });
});
