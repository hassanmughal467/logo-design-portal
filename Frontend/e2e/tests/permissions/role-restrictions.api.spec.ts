import { test, expect } from '@playwright/test';
import { apiUrl, loginApi } from '../../utils/api-client';
import { getAdminToken, provisionClientUser, provisionDesignerUser, teardownUsers } from '../../utils/api-helpers';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';

test.describe('Permissions — role restrictions', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('client cannot list users', async ({ request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const client = await provisionClientUser(request, adminToken, testInfo);
    cleanup.push(client.userId);
    const auth = await loginApi(request, client.email, client.password);
    const res = await request.get(apiUrl('/users'), {
      headers: { Authorization: `Bearer ${auth.token}` },
    });
    expect(res.status()).toBe(403);
  });

  test('designer cannot create invoice', async ({ request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const designer = await provisionDesignerUser(request, adminToken, testInfo);
    cleanup.push(designer.userId);
    const auth = await loginApi(request, designer.email, designer.password);
    const res = await request.post(apiUrl('/invoices'), {
      headers: { Authorization: `Bearer ${auth.token}`, 'Content-Type': 'application/json' },
      data: { orders: [], billingType: 1, taxAmount: 0 },
    });
    expect([403, 400]).toContain(res.status());
  });

  test('superadmin can reach admin analytics', async ({ request }) => {
    const auth = await loginApi(request, E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
    const res = await request.get(apiUrl('/admin/analytics/overview'), {
      headers: { Authorization: `Bearer ${auth.token}` },
    });
    expect(res.status()).toBe(200);
  });
});
