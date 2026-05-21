import { test, expect } from '@playwright/test';

test.describe('Auth — unauthorized redirects', () => {
  test('orders route without session redirects to login', async ({ page }) => {
    await page.goto('/orders');
    await expect(page).toHaveURL(/\/auth\/login|\/login/);
  });

  test('invoices route without session redirects to login', async ({ page }) => {
    await page.goto('/invoices');
    await expect(page).toHaveURL(/\/auth\/login|\/login/);
  });
});
