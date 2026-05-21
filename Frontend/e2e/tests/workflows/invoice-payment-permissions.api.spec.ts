import { test, expect } from '@playwright/test';
import { apiUrl, loginApi } from '../../utils/api-client';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';

test.describe('Workflow — invoice and payment permissions API', () => {
  test('client cannot create invoice', async ({ request }) => {
    const clientAuth = await loginApi(request, 'client@test.com', 'Test@123');
    const response = await request.post(apiUrl('/invoices'), {
      headers: {
        Authorization: `Bearer ${clientAuth.token}`,
        'Content-Type': 'application/json',
      },
      data: {
        orders: [],
        billingType: 'PerLogo',
        taxAmount: 0,
      },
    });
    expect(response.status()).toBe(403);
  });

  test('admin can list invoices', async ({ request }) => {
    const adminAuth = await loginApi(request, E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
    const response = await request.get(apiUrl('/invoices'), {
      headers: { Authorization: `Bearer ${adminAuth.token}` },
    });
    expect(response.ok()).toBeTruthy();
  });

  test('client can read bank details', async ({ request }) => {
    const clientAuth = await loginApi(request, 'client@test.com', 'Test@123');
    const response = await request.get(apiUrl('/payments/bank-details'), {
      headers: { Authorization: `Bearer ${clientAuth.token}` },
    });
    expect(response.ok()).toBeTruthy();
  });
});
