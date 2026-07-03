import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';

test('Elite - API failure then recovery succeeds on retry', async ({ page }) => {
  const login = new LoginPage(page);
  await login.goto();
  await login.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
  await login.expectRedirectToDashboard();
  // Let the dashboard finish its own orders-related calls before arming the failure,
  // so the simulated 500 hits the orders page request (not a dashboard prefetch).
  await page.waitForLoadState('networkidle');

  let intercepted = false;
  // Match only the paged orders LIST call — `**/api/orders**` would also swallow
  // sibling endpoints like /api/orders/unassigned-count.
  await page.route(/\/api\/orders(\?|$)/, async (route) => {
    if (!intercepted) {
      intercepted = true;
      await route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ error: 'simulated first-request failure' }),
      });
      return;
    }
    await route.continue();
  });

  // Deterministic failure signal: the orders list request itself returns 500.
  // (Toast text is racy under parallel runs — SignalR events from sibling tests
  // can trigger a successful re-fetch that repaints the grid within seconds.)
  const failedResponse = page.waitForResponse(
    (res) => /\/api\/orders(\?|$)/.test(res.url()) && res.status() === 500,
    { timeout: 30_000 }
  );
  await page.goto('/orders');
  await failedResponse;
  // The app must stay alive after the failure — no white screen, layout still rendered.
  await expect(page.getByRole('heading', { name: /Orders/i })).toBeVisible({ timeout: 15_000 });

  await page.reload();
  await expect(page.getByRole('heading', { name: /Orders/i })).toBeVisible({ timeout: 30_000 });
  // Admins see "Add Completed Order" — Create Order is Client-only.
  await expect(page.getByTestId('orders-open-quick-completed')).toBeVisible();
});
