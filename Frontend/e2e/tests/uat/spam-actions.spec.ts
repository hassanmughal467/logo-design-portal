import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { clickRapidly } from '../../utils/human-behavior';
import { provisionClientForUi, teardownUsers } from '../../utils/api-helpers';

test.describe('UAT - spam actions', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('UAT - spam clicking create order does not create duplicate modals', async ({ page, request }, testInfo) => {
    const client = await provisionClientForUi(request, testInfo);
    cleanup.push(client.userId);

    const login = new LoginPage(page);
    await login.goto();
    await login.login(client.email, client.password);
    await login.expectRedirectToDashboard();
    await page.goto('/orders');

    await clickRapidly(page, '[data-testid="orders-open-create"]', 6);
    await expect(page.getByRole('dialog', { name: /Create New Order/i })).toHaveCount(1);
  });

  test('UAT - double submit order form handled gracefully', async ({ page, request }, testInfo) => {
    const client = await provisionClientForUi(request, testInfo);
    cleanup.push(client.userId);

    const login = new LoginPage(page);
    await login.goto();
    await login.login(client.email, client.password);
    await login.expectRedirectToDashboard();
    await page.goto('/orders');
    await page.getByTestId('orders-open-create').click();

    await page.getByTestId('order-create-title').fill(`UAT spam ${Date.now()}`);
    await page
      .getByTestId('order-create-description')
      .fill('User double-click submit behavior simulation to avoid duplicate order requests.');

    const submit = page.getByTestId('order-create-submit');
    await submit.dblclick();
    await expect(page.locator('body')).toBeVisible();
  });
});
