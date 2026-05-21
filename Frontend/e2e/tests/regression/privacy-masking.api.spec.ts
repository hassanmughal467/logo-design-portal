import { test, expect } from '@playwright/test';
import { apiUrl, approveOrderApi, assignDesignerApi, createOrderApi, loginApi } from '../../utils/api-client';
import { getAdminToken, provisionClientUser, provisionDesignerUser, teardownUsers } from '../../utils/api-helpers';
import { uniqueSuffix } from '../../utils/test-data';

/**
 * Regression: Week 1 privacy — clients must not see raw designer identity on orders.
 */
test.describe('Regression — DTO privacy masking', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('client order JSON does not expose designer object', async ({ request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const client = await provisionClientUser(request, adminToken, testInfo);
    const designer = await provisionDesignerUser(request, adminToken, testInfo);
    cleanup.push(client.userId, designer.userId);

    const auth = await loginApi(request, client.email, client.password);
    const order = await createOrderApi(request, auth.token, {
      title: `Mask ${uniqueSuffix(testInfo)}`,
      description: 'Regression test for designer identity masking on order DTO.',
      price: 50,
    });
    await approveOrderApi(request, adminToken, order.id);
    await assignDesignerApi(request, adminToken, order.id, designer.userId);

    const res = await request.get(apiUrl(`/orders/${order.id}`), {
      headers: { Authorization: `Bearer ${auth.token}` },
    });
    expect(res.ok()).toBeTruthy();
    const body = await res.text();
    expect(body).not.toMatch(/designer@test\.com/i);
    expect(body).not.toContain('"designerEmail"');
  });
});
