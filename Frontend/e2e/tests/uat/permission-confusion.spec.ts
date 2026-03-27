import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { getAdminToken, provisionClientUser, teardownUsers } from '../../utils/api-helpers';

test.describe.configure({ mode: 'serial' });

test.describe('UAT - permission confusion', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('client navigating to admin urls is blocked safely', async ({ page, request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const client = await provisionClientUser(request, adminToken, testInfo);
    cleanup.push(client.userId);

    const login = new LoginPage(page);
    await login.goto();
    await login.login(client.email, client.password);
    await login.expectRedirectToDashboard();

    await page.goto('/users');
    await expect(page).not.toHaveURL(/\/users$/);
    await expect(page.locator('body')).toContainText(/unauthorized|forbidden|dashboard|access/i);

    await page.goto('/permissions');
    await expect(page).not.toHaveURL(/\/permissions$/);
    await expect(page.locator('body')).toContainText(/unauthorized|forbidden|dashboard|access/i);
  });
});
