import { test, expect } from '@playwright/test';
import { OrdersListPage } from '../../pom';
import {
  apiUrl,
  approveLogoApi,
  approveOrderApi,
  assignDesignerApi,
  extractPagedItems,
  getOrderApi,
  loginApi,
  requestRevisionApi,
  updateOrderStatusApi,
} from '../../utils/api-client';
import {
  getAdminToken,
  provisionAdminUser,
  provisionClientUser,
  provisionDesignerUser,
  teardownUsers,
} from '../../utils/api-helpers';
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

    // Dedicated admin account for the live UI session below (kept open for the whole test) —
    // must not share a refresh-token row with other concurrently-running admin sessions. The
    // one-shot `adminToken` above is unaffected since it's never refreshed, only reused directly.
    const adminForUi = await provisionAdminUser(request, adminToken, testInfo);
    cleanup.push(adminForUi.userId);

    // Obtain the client's one-shot API token BEFORE the client's UI session logs in below.
    // AuthService.LoginAsync unconditionally overwrites the account's single stored refresh
    // token on every login (same field RefreshTokenAsync checks). Calling loginApi() after the
    // UI session started would silently invalidate that session's refresh token, so the next
    // full-page navigation (clientOrders.goto() below) would fail its proactive refresh with
    // "Invalid refresh token" and get logged out.
    const clientAuth = await loginApi(request, client.email, client.password);

    const sessions = await Promise.all([
      createRoleSession(browser, { email: adminForUi.email, password: adminForUi.password }),
      createRoleSession(browser, { email: designer.email, password: designer.password }),
      createRoleSession(browser, { email: client.email, password: client.password }),
    ]);
    const [, , clientSession] = sessions;

    const orderTitle = `Multi-user ${uniqueSuffix(testInfo)}`;
    // Only Clients can create orders (RBAC).
    const clientOrders = new OrdersListPage(clientSession.page);
    await clientOrders.goto();
    await clientOrders.openCreateOrderModal();
    await clientOrders.createOrder.fillAndSubmit(
      orderTitle,
      'Multi-user workflow order created from client UI for cross-role collaboration.'
    );

    const myOrdersRes = await request.get(apiUrl('/orders/my-orders'), {
      headers: { Authorization: `Bearer ${clientAuth.token}` },
    });
    expect(myOrdersRes.ok()).toBeTruthy();
    const myOrders = extractPagedItems<{ id: string; title: string }>(await myOrdersRes.json());
    const created = myOrders.find((x) => x.title === orderTitle);
    expect(created).toBeTruthy();

    await approveOrderApi(request, adminToken, created!.id);
    await assignDesignerApi(request, adminToken, created!.id, designer.userId);
    // Admin advances to PreviewDelivered (designers cannot via status API).
    await updateOrderStatusApi(request, adminToken, created!.id, 'PreviewDelivered', 'Draft shared.');
    await requestRevisionApi(request, clientAuth.token, created!.id, 'Need a second revision before approval.');
    await updateOrderStatusApi(request, adminToken, created!.id, 'PreviewDelivered', 'Second draft shared.');
    await approveLogoApi(request, clientAuth.token, created!.id, 'Approved by client.');

    const final = await getOrderApi(request, clientAuth.token, created!.id);
    expect(String(final.status).toLowerCase()).toContain('approved');

    await closeRoleSessions(sessions);
  });
});
