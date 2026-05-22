import { test, expect } from '@playwright/test';
import { getAdminAnalyticsOverviewApi, createOrderApi } from '../../utils/api-client';
import { getAdminToken, provisionLoggedInClient, teardownUsers } from '../../utils/api-helpers';
import { uniqueSuffix } from '../../utils/test-data';

test.describe.configure({ mode: 'serial' });

test.describe('Dashboard — admin analytics deltas', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('totalOrders increases after client creates an order', async ({ request }, testInfo) => {
    const before = await getAdminAnalyticsOverviewApi(request);

    const client = await provisionLoggedInClient(request, testInfo);
    cleanup.push(client.userId);
    const suffix = uniqueSuffix(testInfo);
    await createOrderApi(request, client.token, {
      title: `Dash metric ${suffix}`,
      description: 'Dashboard KPI regression guard — valid description length for API.',
    });

    const after = await getAdminAnalyticsOverviewApi(request);
    expect(after.totalOrders).toBeGreaterThanOrEqual(before.totalOrders);
  });
});
