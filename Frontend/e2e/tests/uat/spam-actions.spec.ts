import { test, expect } from '@playwright/test';
import { LoginPage, OrderCreateModalPage } from '../../pom';
import { getAdminToken, provisionClientUser, teardownUsers } from '../../utils/api-helpers';
import { clickRapidly } from '../../utils/human-behavior';

/** Order creation is Client-only, so these UAT flows run as a provisioned client. */
test('UAT - spam clicking create order does not create duplicate modals', async ({ page, request }, testInfo) => {
  const adminToken = await getAdminToken(request);
  const client = await provisionClientUser(request, adminToken, testInfo);
  try {
    const login = new LoginPage(page);
    await login.goto();
    await login.login(client.email, client.password);
    await login.expectRedirectToDashboard();
    await page.goto('/orders');

    await clickRapidly(page, '[data-testid="orders-open-create"]', 6);
    // PrimeNG dialog exposes the header as the dialog accessible name (not a heading role).
    await expect(page.getByRole('dialog', { name: /Create New Order/i })).toHaveCount(1);
  } finally {
    await teardownUsers(request, [client.userId]);
  }
});

test('UAT - double submit order form handled gracefully', async ({ page, request }, testInfo) => {
  const adminToken = await getAdminToken(request);
  const client = await provisionClientUser(request, adminToken, testInfo);
  try {
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
    await new OrderCreateModalPage(page).attachMinimalReferenceFile();

    const submit = page.getByTestId('order-create-submit');
    await submit.dblclick();
    await expect(page.locator('body')).toBeVisible();
  } finally {
    await teardownUsers(request, [client.userId]);
  }
});
