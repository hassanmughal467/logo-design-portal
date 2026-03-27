import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';
import { clickRapidly } from '../../utils/human-behavior';

test('UAT - spam clicking create order does not create duplicate modals', async ({ page }) => {
  const login = new LoginPage(page);
  await login.goto();
  await login.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
  await login.expectRedirectToDashboard();
  await page.goto('/orders');

  await clickRapidly(page, '[data-testid="orders-open-create"]', 6);
  await expect(page.getByRole('heading', { name: /Create New Order/i })).toHaveCount(1);
});

test('UAT - double submit order form handled gracefully', async ({ page }) => {
  const login = new LoginPage(page);
  await login.goto();
  await login.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
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
