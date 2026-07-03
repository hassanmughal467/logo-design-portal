import { test, expect } from '@playwright/test';
import { LoginPage, OrdersListPage } from '../../pom';
import { apiUrl, approveOrderApi, getOrderApi, loginApi } from '../../utils/api-client';
import { getAdminToken, provisionClientUser, teardownUsers } from '../../utils/api-helpers';
import { uniqueSuffix } from '../../utils/test-data';

test.describe.configure({ mode: 'serial' });

test.describe('Elite - data persistence', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('order state persists after reload and API re-fetch', async ({ page, request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const client = await provisionClientUser(request, adminToken, testInfo);
    cleanup.push(client.userId);
    const clientAuth = await loginApi(request, client.email, client.password);

    const title = `Persistence ${uniqueSuffix(testInfo)}`;
    const orders = new OrdersListPage(page);
    const login = new LoginPage(page);
    await login.goto();
    await login.login(client.email, client.password);
    // Wait for the authenticated redirect before navigating, or the auth guard bounces us back to login.
    await login.expectRedirectToDashboard();
    await orders.goto();
    await orders.openCreateOrderModal();
    await orders.createOrder.fillAndSubmit(title, 'Persistence scenario for reload verification of order state.');

    const myOrdersRes = await request.get(apiUrl('/orders/my-orders'), {
      headers: { Authorization: `Bearer ${clientAuth.token}` },
    });
    const myOrders = (await myOrdersRes.json()) as Array<{ id: string; title: string }>;
    const created = myOrders.find((o) => o.title === title);
    expect(created).toBeTruthy();

    await approveOrderApi(request, adminToken, created!.id);
    const beforeReload = await getOrderApi(request, clientAuth.token, created!.id);
    await page.reload();
    await expect(page.getByRole('heading', { name: /Orders/i })).toBeVisible();
    const afterReload = await getOrderApi(request, clientAuth.token, created!.id);

    expect(String(afterReload.status).toLowerCase()).toContain(String(beforeReload.status).toLowerCase());
  });
});
