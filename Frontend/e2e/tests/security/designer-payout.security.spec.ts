import { test, expect } from '@playwright/test';
import { apiUrl, loginApi, loginApiBearerOnly } from '../../utils/api-client';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';
import { getAdminToken, provisionClientUser, provisionDesignerUser, teardownUsers } from '../../utils/api-helpers';

test.describe('Security — designer payout API', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('client cannot access payout summary', async ({ request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const client = await provisionClientUser(request, adminToken, testInfo);
    cleanup.push(client.userId);

    const clientAuth = await loginApiBearerOnly(request, client.email, client.password);
    const response = await request.get(apiUrl('/designer-invoice/payout-summary'), {
      headers: { Authorization: `Bearer ${clientAuth.token}` },
    });
    expect(response.status()).toBe(403);
  });

  test('designer cannot access admin payout summary', async ({ request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const designer = await provisionDesignerUser(request, adminToken, testInfo);
    cleanup.push(designer.userId);

    const designerAuth = await loginApiBearerOnly(request, designer.email, designer.password);
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
