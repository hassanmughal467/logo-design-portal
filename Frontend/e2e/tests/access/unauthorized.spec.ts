import { test, expect } from '@playwright/test';
import { apiUrl } from '../../utils/api-client';

test('orders API without bearer returns 401', async ({ request }) => {
  const res = await request.post(apiUrl('/orders'), {
    headers: { 'Content-Type': 'application/json' },
    data: { title: 'x', description: 'y', price: 1 },
  });
  expect(res.status()).toBe(401);
});

test('protected Angular route redirects unauthenticated users to login', async ({ page }) => {
  await page.goto('/users');
  await expect(page).toHaveURL(/\/auth\/login|\/login/);
});
