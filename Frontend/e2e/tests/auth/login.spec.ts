import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { E2E_API_URL } from '../../utils/env';
import { apiUrl } from '../../utils/api-client';

/**
 * Smoke coverage for the anonymous login experience. These tests intentionally do not depend on
 * `storageState` — they run under the default `chromium` project.
 */

async function backendLoginProbe(
  request: import('@playwright/test').APIRequestContext
): Promise<'ok' | 'unauthorized' | 'unreachable'> {
  try {
    const res = await request.post(apiUrl('/auth/login'), {
      headers: { 'Content-Type': 'application/json' },
      data: { email: 'client@test.com', password: 'Test@123' },
      timeout: 8_000,
    });
    if (res.status() === 401 || res.status() === 400) {
      return 'unauthorized';
    }
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

  test('valid seeded credentials redirect to dashboard when test user exists', async ({ page, request }) => {
    const status = await backendLoginProbe(request);
    if (status === 'unreachable') {
      test.skip(true, `Backend not running at ${E2E_API_URL}. Start the API before E2E.`);
    }
    if (status !== 'ok') {
      test.skip(
        true,
        'Probe login did not succeed for client@test.com — seed that user or use workflow tests that provision via API.'
      );
    }
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login('client@test.com', 'Test@123');
    await loginPage.expectRedirectToDashboard();
  });

  test('unauthenticated access to dashboard redirects to login', async ({ page }) => {
    await page.goto('/dashboard');
    await expect(page).toHaveURL(/\/auth\/login|\/login/);
  });
});
