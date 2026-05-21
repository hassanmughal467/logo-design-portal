import { test, expect } from '@playwright/test';
import { apiUrl, loginApi } from '../../utils/api-client';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';

test.describe('Security — designer payout API', () => {
  test('client cannot access payout summary', async ({ request }) => {
    const clientAuth = await loginApi(request, 'client@test.com', 'Test@123');
    const response = await request.get(apiUrl('/designer-invoice/payout-summary'), {
      headers: { Authorization: `Bearer ${clientAuth.token}` },
    });
    expect(response.status()).toBe(403);
  });

  test('designer cannot access admin payout summary', async ({ request }) => {
    const designerAuth = await loginApi(request, 'designer@test.com', 'Test@123');
    const response = await request.get(apiUrl('/designer-invoice/payout-summary'), {
      headers: { Authorization: `Bearer ${designerAuth.token}` },
    });
    expect(response.status()).toBe(403);
  });

  test('admin can access payout summary', async ({ request }) => {
    const adminAuth = await loginApi(request, E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
    const response = await request.get(apiUrl('/designer-invoice/payout-summary'), {
      headers: { Authorization: `Bearer ${adminAuth.token}` },
    });
    expect(response.ok()).toBeTruthy();
  });
});
