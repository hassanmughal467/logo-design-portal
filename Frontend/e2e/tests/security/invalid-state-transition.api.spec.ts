import { test, expect } from '@playwright/test';
import { createOrderApi, loginApi } from '../../utils/api-client';
import { getAdminToken, provisionClientUser, teardownUsers } from '../../utils/api-helpers';
import { uniqueSuffix } from '../../utils/test-data';

test.describe('Security — invalid state transitions', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('client cannot jump order to Completed directly', async ({ request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const client = await provisionClientUser(request, adminToken, testInfo);
    cleanup.push(client.userId);
    const auth = await loginApi(request, client.email, client.password);
    const order = await createOrderApi(request, auth.token, {
      title: `State ${uniqueSuffix(testInfo)}`,
      description: 'State machine must block illegal Completed transition.',
      price: 30,
    });
    const res = await request.put(
      `${process.env.E2E_API_URL ?? 'http://localhost:5000'}/api/orders/${order.id}/status`,
      {
        headers: { Authorization: `Bearer ${auth.token}`, 'Content-Type': 'application/json' },
        data: { status: 'Completed', notes: '' },
      }
    );
    expect([400, 403]).toContain(res.status());
  });
});
