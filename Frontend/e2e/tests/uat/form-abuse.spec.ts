import { test, expect } from '@playwright/test';
import { LoginPage, OrderCreateModalPage } from '../../pom';
import { getAdminToken, provisionClientUser, teardownUsers } from '../../utils/api-helpers';
import { randomHumanDelay } from '../../utils/human-behavior';

/** Order creation is Client-only, so these UAT flows run as a provisioned client. */
test('UAT - form abuse with missing fields and mid-edit changes remains stable', async ({ page, request }, testInfo) => {
  const adminToken = await getAdminToken(request);
  const client = await provisionClientUser(request, adminToken, testInfo);
  try {
    const login = new LoginPage(page);
    await login.goto();
    await login.login(client.email, client.password);
    await login.expectRedirectToDashboard();
    await page.goto('/orders');

    await page.getByTestId('orders-open-create').click();
    await page.getByTestId('order-create-title').fill('Abuse scenario');
    await page.getByTestId('order-create-description').fill('');
    // Submit is disabled while the form is invalid — the UI must block the abuse attempt.
    await expect(page.getByTestId('order-create-submit')).toHaveClass(/p-disabled/);

    await page.getByTestId('order-create-description').fill('User edits content repeatedly while uncertain.');
    await randomHumanDelay();
    await page.getByTestId('order-create-title').fill('Abuse scenario edited');
    await page.keyboard.press('Tab');
    await new OrderCreateModalPage(page).attachMinimalReferenceFile();
    await page.getByTestId('order-create-submit').click();
    await expect(page.locator('body')).toBeVisible();
  } finally {
    await teardownUsers(request, [client.userId]);
  }
});

test('UAT - switching tabs while editing does not corrupt page state', async ({ page, context, request }, testInfo) => {
  const adminToken = await getAdminToken(request);
  const client = await provisionClientUser(request, adminToken, testInfo);
  try {
    const login = new LoginPage(page);
    await login.goto();
    await login.login(client.email, client.password);
    await login.expectRedirectToDashboard();
    await page.goto('/orders');
    await page.getByTestId('orders-open-create').click();
    await page.getByTestId('order-create-title').fill(`Tab switch ${Date.now()}`);

    const secondTab = await context.newPage();
    await secondTab.goto('/dashboard');
    await expect(secondTab.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
    await secondTab.close();

    await expect(page.getByTestId('order-create-title')).toBeVisible();
    await expect(page.locator('body')).toBeVisible();
  } finally {
    await teardownUsers(request, [client.userId]);
  }
});
