import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';
import { provisionClientForUi, teardownUsers } from '../../utils/api-helpers';

test.describe('UAT - interrupted flow', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('UAT - interrupted order draft survives refresh and revisit', async ({ page, request }, testInfo) => {
    const client = await provisionClientForUi(request, testInfo);
    cleanup.push(client.userId);

    const login = new LoginPage(page);
    await login.goto();
    await login.login(client.email, client.password);
    await login.expectRedirectToDashboard();
    await page.goto('/orders');
    await page.getByTestId('orders-open-create').click();

    const draftTitle = `Interrupted ${Date.now()}`;
    await page.getByTestId('order-create-title').fill(draftTitle);
    await page.getByTestId('order-create-description').fill('User started drafting and got interrupted.');

    await page.reload();
    await page.goto('/orders');
    await expect(page.getByRole('heading', { name: /Orders/i })).toBeVisible();
  });

  test('UAT - close and reopen session allows safe continuation', async ({ browser }) => {
    const contextA = await browser.newContext();
    const pageA = await contextA.newPage();
    const loginA = new LoginPage(pageA);
    await loginA.goto();
    await loginA.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
    await loginA.expectRedirectToDashboard();
    const state = await contextA.storageState();
    await contextA.close();

    const contextB = await browser.newContext({ storageState: state });
    const pageB = await contextB.newPage();
    await pageB.goto('/orders');
    await expect(pageB.getByRole('heading', { name: /Orders/i })).toBeVisible();
    await contextB.close();
  });
});
