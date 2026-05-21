import { test, expect } from '@playwright/test';
import { createOrderApi, loginApi } from '../../utils/api-client';
import { getAdminToken, provisionClientUser, teardownUsers } from '../../utils/api-helpers';
import { uniqueSuffix } from '../../utils/test-data';

test.describe('Workflows — create order', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('client can create order via API', async ({ request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const client = await provisionClientUser(request, adminToken, testInfo);
    cleanup.push(client.userId);
    const auth = await loginApi(request, client.email, client.password);
    const suffix = uniqueSuffix(testInfo);
    const order = await createOrderApi(request, auth.token, {
      title: `Create order ${suffix}`,
      description: 'Minimum description length for validation rules in API.',
      price: 75,
    });
    expect(order.id).toBeTruthy();
    expect(order.status).toBeTruthy();
  });
});
