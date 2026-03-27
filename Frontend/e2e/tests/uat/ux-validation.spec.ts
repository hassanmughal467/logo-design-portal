import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';

test('UAT - core pages have visible controls and readable content', async ({ page }) => {
  const login = new LoginPage(page);
  await login.goto();
  await login.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
  await login.expectRedirectToDashboard();

  await page.goto('/dashboard');
  await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
  await expect(page.locator('button:visible').first()).toBeVisible();

  await page.goto('/orders');
  await expect(page.getByRole('heading', { name: /Orders/i })).toBeVisible();
  await expect(page.getByTestId('orders-open-create')).toBeVisible();

  await page.goto('/invoices');
  await expect(page.getByText('Invoices Management')).toBeVisible();
  await expect(page.locator('body')).not.toContainText(/undefined|null/i);
});
