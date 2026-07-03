import { test, expect } from '@playwright/test';
import { LoginPage, MainLayoutPage, OrdersListPage } from '../../pom';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';
import { apiUrl, cancelOrderApi, createOrderApi, getOrderApi, loginApi } from '../../utils/api-client';
import { getAdminToken, provisionLoggedInClient, teardownUsers } from '../../utils/api-helpers';
import { uniqueSuffix } from '../../utils/test-data';

/**
 * End-to-end lifecycle: provision a real Client via API (fast + reliable), then exercise the UI as
 * that customer, then mediate the order as an Admin — finally assert state through the REST API.
 *
 * Runs under the anonymous `chromium` project; multiple actors log in sequentially.
 */
test.describe.configure({ mode: 'serial' });

test.describe('Workflow — client order and admin approval', () => {
  const cleanupIds: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanupIds);
  });

  test('client creates order in UI; admin approves in UI; API shows approved state', async ({ page, request }) => {
    test.slow();
    const client = await provisionLoggedInClient(request, test.info(), 'Test@123');
    cleanupIds.push(client.userId);

    const suffix = uniqueSuffix(test.info());
    const orderTitle = `E2E Order ${suffix}`;

    const loginPage = new LoginPage(page);
    const mainLayout = new MainLayoutPage(page);
    const orders = new OrdersListPage(page);

    await loginPage.goto();
    await loginPage.login(client.email, client.password);
    await loginPage.expectRedirectToDashboard();

    await orders.goto();
    await orders.openCreateOrderModal();
    await orders.createOrder.fillAndSubmit(
      orderTitle,
      'Automated end-to-end scenario — description meets minimum length.'
    );
    await orders.orderDetail.closeIfOpen();

    await mainLayout.logout();

    await loginPage.goto();
    await loginPage.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
    await loginPage.expectRedirectToDashboard();

    await orders.goto();
    await orders.openOrderInTableByTitle(orderTitle);
    await orders.orderDetail.approveOrder();

    const clientToken = (await loginApi(request, client.email, client.password)).token;
    const clientOrders = await request.get(apiUrl('/orders/my-orders'), {
      headers: { Authorization: `Bearer ${clientToken}` },
    });
    expect(clientOrders.ok()).toBeTruthy();
    const list = (await clientOrders.json()) as Array<{ id: string; title: string; status: string }>;
    const row = list.find((o) => o.title === orderTitle);
    expect(row, 'created order visible to client').toBeTruthy();
    const refreshed = await getOrderApi(request, clientToken, row!.id);
    // Approve without assigning a designer parks the order at ApprovedUnassigned;
    // it becomes InProgress once a designer is assigned.
    expect(refreshed.status).toMatch(/ApprovedUnassigned|InProgress/i);
  });

  test('admin can reject a pending order via cancel API (backend contract)', async ({ request }) => {
    const client = await provisionLoggedInClient(request, test.info(), 'Test@123');
    cleanupIds.push(client.userId);
    const clientToken = client.token;
    const suffix = uniqueSuffix(test.info());
    const created = await createOrderApi(request, clientToken, {
      title: `E2E Cancel ${suffix}`,
      description: 'Order to be cancelled by admin for workflow negative path.',
    });
    const token = await getAdminToken(request);

    await cancelOrderApi(request, token, created.id, 'E2E: admin rejects pending work — needs rewrite of brief.');

    const snap = await getOrderApi(request, clientToken, created.id);
    expect(snap.status).toMatch(/CancelledByAdmin/i);
  });
});
