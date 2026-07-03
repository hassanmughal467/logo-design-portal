import { test, expect } from '@playwright/test';
import { OrdersListPage } from '../../pom';
import {
  apiUrl,
  approveLogoApi,
  approveOrderApi,
  assignDesignerApi,
  getOrderApi,
  loginApi,
  requestRevisionApi,
  updateOrderStatusAsAdminApi,
} from '../../utils/api-client';
import { getAdminToken, provisionClientUser, provisionDesignerUser, teardownUsers } from '../../utils/api-helpers';
import { createRoleSession, closeRoleSessions } from '../../utils/role-session';
import { uniqueSuffix } from '../../utils/test-data';

test.describe.configure({ mode: 'serial' });

test.describe('Elite - multi user workflow', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('admin, designer, and client complete isolated multi-context flow', async ({ browser, request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const client = await provisionClientUser(request, adminToken, testInfo);
    const designer = await provisionDesignerUser(request, adminToken, testInfo);
    cleanup.push(client.userId, designer.userId);

    const adminCreds = { email: process.env.E2E_ADMIN_EMAIL!, password: process.env.E2E_ADMIN_PASSWORD! };
    const sessions = await Promise.all([
      createRoleSession(browser, adminCreds),
      createRoleSession(browser, { email: designer.email, password: designer.password }),
      createRoleSession(browser, { email: client.email, password: client.password }),
    ]);
    const [adminSession, , clientSession] = sessions;

    const clientAuth = await loginApi(request, client.email, client.password);
    const designerAuth = await loginApi(request, designer.email, designer.password);

    const orderTitle = `Multi-user ${uniqueSuffix(testInfo)}`;
    const clientOrders = new OrdersListPage(clientSession.page);
    await clientOrders.goto();
    await clientOrders.openCreateOrderModal();
    await clientOrders.createOrder.fillAndSubmit(
      orderTitle,
      'Multi-user workflow order created by client UI; only Client role may create orders.'
    );

    const allOrdersRes = await request.get(apiUrl('/orders?pageSize=100'), {
      headers: { Authorization: `Bearer ${adminToken}` },
    });
    expect(allOrdersRes.ok()).toBeTruthy();
    // GET /api/orders returns an ApiResponse envelope with paged data.
    const allOrdersBody = (await allOrdersRes.json()) as {
      data: { items: Array<{ id: string; title: string }> };
    };
    const created = allOrdersBody.data.items.find((x) => x.title === orderTitle);
    expect(created).toBeTruthy();

    await approveOrderApi(request, adminToken, created!.id);
    await assignDesignerApi(request, adminToken, created!.id, designer.userId);
    // PreviewDelivered is an admin forwarding step now (designers upload files; admin sends to client).
    await updateOrderStatusAsAdminApi(request, adminToken, created!.id, 'PreviewDelivered', 'Draft shared.');
    await requestRevisionApi(request, clientAuth.token, created!.id, 'Need a second revision before approval.');
    await updateOrderStatusAsAdminApi(request, adminToken, created!.id, 'PreviewDelivered', 'Second draft shared.');
    await approveLogoApi(request, clientAuth.token, created!.id, 'Approved by client.');

    const final = await getOrderApi(request, clientAuth.token, created!.id);
    expect(String(final.status).toLowerCase()).toContain('approved');

    await closeRoleSessions(sessions);
  });
});
