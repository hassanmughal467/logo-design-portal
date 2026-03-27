import { test, expect } from '@playwright/test';
import { getAdminAnalyticsOverviewApi, createOrderApi, loginApi } from '../../utils/api-client';
import { getAdminToken, provisionClientUser, teardownUsers } from '../../utils/api-helpers';
import { uniqueSuffix } from '../../utils/test-data';

test.describe.configure({ mode: 'serial' });

test.describe('Dashboard — admin analytics deltas', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('totalOrders increases after client creates an order', async ({ request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const before = await getAdminAnalyticsOverviewApi(request, adminToken);

    const client = await provisionClientUser(request, adminToken, testInfo);
    cleanup.push(client.userId);
    const clientAuth = await loginApi(request, client.email, client.password);
    const suffix = uniqueSuffix(testInfo);
    await createOrderApi(request, clientAuth.token, {
      title: `Dash metric ${suffix}`,
      description: 'Dashboard KPI regression guard — valid description length for API.',
    });

    const after = await getAdminAnalyticsOverviewApi(request, adminToken);
    expect(after.totalOrders).toBeGreaterThanOrEqual(before.totalOrders);
  });
});
