import { test } from '@playwright/test';
import { gotoExpectUnauthenticatedRedirect } from '../../utils/nav';

test.describe('Auth — unauthorized redirects', () => {
  test('orders route without session redirects to login', async ({ page }) => {
    await gotoExpectUnauthenticatedRedirect(page, '/orders');
  });

  test('invoices route without session redirects to login', async ({ page }) => {
    // Invoices lazy chunk is large; allow extra time before AuthGuard redirect completes.
    await gotoExpectUnauthenticatedRedirect(page, '/invoices', {
      navigationTimeout: 90_000,
      redirectTimeout: 45_000,
    });
  });
});
