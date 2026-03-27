import { test, expect } from '@playwright/test';
import { apiUrl, loginApi } from '../../utils/api-client';
import { getAdminToken, provisionClientUser, teardownUsers } from '../../utils/api-helpers';
import { uniqueSuffix } from '../../utils/test-data';

test.describe('Security — RBAC and bad tokens', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('malformed bearer returns 401 for protected resource', async ({ request }) => {
    const res = await request.get(apiUrl('/orders/my-orders'), {
      headers: { Authorization: 'Bearer not-a-valid-jwt' },
    });
    expect(res.status()).toBe(401);
  });

  test('client cannot load admin analytics overview (403)', async ({ request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const client = await provisionClientUser(request, adminToken, testInfo);
    cleanup.push(client.userId);
    const clientAuth = await loginApi(request, client.email, client.password);

    const res = await request.get(apiUrl('/admin/analytics/overview'), {
      headers: { Authorization: `Bearer ${clientAuth.token}` },
    });
    expect(res.status()).toBe(403);
  });

  test('structurally invalid JWT returns 401', async ({ request }) => {
    const res = await request.get(apiUrl('/invoices'), {
      headers: {
        Authorization:
          'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0In0.invalid-signature',
      },
    });
    expect(res.status()).toBe(401);
  });
});
