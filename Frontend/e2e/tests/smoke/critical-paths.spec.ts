import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { apiUrl } from '../../utils/api-client';
import { E2E_API_URL } from '../../utils/env';

test.describe('Smoke — critical paths', () => {
  test('login page loads', async ({ page }) => {
    const login = new LoginPage(page);
    await login.goto();
    await expect(page.locator('input[type="email"], input[name="email"]').first()).toBeVisible();
  });

  test('API health live responds', async ({ request }) => {
    const base = E2E_API_URL.replace(/\/$/, '');
    const res = await request.get(`${base}/health/live`);
    if (res.status() === 404) {
      test.skip(true, 'Health live not deployed');
    }
    expect(res.ok()).toBeTruthy();
  });

  test('unauthenticated orders API returns 401', async ({ request }) => {
    const res = await request.get(apiUrl('/orders/my-orders'));
    expect(res.status()).toBe(401);
  });
});
