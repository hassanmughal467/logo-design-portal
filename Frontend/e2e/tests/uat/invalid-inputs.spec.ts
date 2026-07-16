import { test, expect } from '@playwright/test';
import { LoginPage, OrdersListPage } from '../../pom';
import { invalidEmailSamples, randomText } from '../../utils/human-behavior';
import { provisionClientForUi, teardownUsers } from '../../utils/api-helpers';

test.describe('UAT - invalid inputs', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('UAT - invalid order form inputs show validation and keep UI stable', async ({ page, request }, testInfo) => {
    const client = await provisionClientForUi(request, testInfo);
    cleanup.push(client.userId);

    const login = new LoginPage(page);
    const orders = new OrdersListPage(page);
    await login.goto();
    await login.login(client.email, client.password);
    await login.expectRedirectToDashboard();
    await orders.goto();
    await orders.openCreateOrderModal();

    await page.getByTestId('order-create-submit').click();
    await expect(page.locator('body')).toContainText(/required|invalid|title/i);

    await page.getByTestId('order-create-title').fill(randomText(300));
    await page.getByTestId('order-create-description').fill(randomText(5000));
    await page.getByTestId('order-create-submit').click();
    await expect(page.locator('body')).toBeVisible();
  });

  test('UAT - invalid email patterns do not break login page', async ({ page }) => {
    const login = new LoginPage(page);
    await login.goto();

    for (const invalidEmail of invalidEmailSamples()) {
      await page.getByTestId('login-email').fill(invalidEmail);
      await page.getByTestId('login-password').locator('input').fill('bad-password');
      await page.getByTestId('login-submit').click();
      await expect(page.locator('body')).toContainText(/invalid|error|failed|credentials/i);
    }
  });
});
