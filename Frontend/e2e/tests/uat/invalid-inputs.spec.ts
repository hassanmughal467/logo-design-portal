import { test, expect } from '@playwright/test';
import { LoginPage, OrdersListPage } from '../../pom';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';
import { invalidEmailSamples, randomText } from '../../utils/human-behavior';

test('UAT - invalid order form inputs show validation and keep UI stable', async ({ page }) => {
  const login = new LoginPage(page);
  const orders = new OrdersListPage(page);
  await login.goto();
  await login.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
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
