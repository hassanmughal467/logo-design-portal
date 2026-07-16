import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { randomHumanDelay } from '../../utils/human-behavior';
import { provisionClientForUi, teardownUsers } from '../../utils/api-helpers';

test.describe('UAT - form abuse', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('UAT - form abuse with missing fields and mid-edit changes remains stable', async ({ page, request }, testInfo) => {
    const client = await provisionClientForUi(request, testInfo);
    cleanup.push(client.userId);

    const login = new LoginPage(page);
    await login.goto();
    await login.login(client.email, client.password);
    await login.expectRedirectToDashboard();
    await page.goto('/orders');

    await page.getByTestId('orders-open-create').click();
    await page.getByTestId('order-create-title').fill('Abuse scenario');
    await page.getByTestId('order-create-description').fill('');
    await page.getByTestId('order-create-submit').click();
    await expect(page.locator('body')).toContainText(/required|description|invalid/i);

    await page.getByTestId('order-create-description').fill('User edits content repeatedly while uncertain.');
    await randomHumanDelay();
    await page.getByTestId('order-create-title').fill('Abuse scenario edited');
    await page.keyboard.press('Tab');
    await page.getByTestId('order-create-submit').click();
    await expect(page.locator('body')).toBeVisible();
  });

  test('UAT - switching tabs while editing does not corrupt page state', async ({ page, context, request }, testInfo) => {
    const client = await provisionClientForUi(request, testInfo);
    cleanup.push(client.userId);

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
  });
});
