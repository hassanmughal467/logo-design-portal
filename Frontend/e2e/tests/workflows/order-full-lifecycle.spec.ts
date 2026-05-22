import { test, expect } from '@playwright/test';
import {
  approveLogoApi,
  approveOrderApi,
  assignDesignerApi,
  createInvoiceApi,
  createOrderApi,
  getOrderApi,
  updateOrderStatusAsAdminApi,
  loginApiBearerOnly,
  requestRevisionApi,
  updateOrderStatusApi,
} from '../../utils/api-client';
import {
  getAdminToken,
  provisionDesignerUser,
  provisionLoggedInClient,
  teardownUsers,
} from '../../utils/api-helpers';
import { uniqueSuffix } from '../../utils/test-data';

/**
 * API-driven full lifecycle (stable in CI): create → approve → assign → deliver → revision → approve → complete → invoice.
 */
test.describe.configure({ mode: 'serial' });

test.describe('Order — full lifecycle to invoice', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('create through completed order and generate invoice', async ({ request }, testInfo) => {
    const client = await provisionLoggedInClient(request, testInfo);
    cleanup.push(client.userId);

    const suffix = uniqueSuffix(testInfo);
    const order = await createOrderApi(request, client.token, {
      title: `Lifecycle ${suffix}`,
      description: 'Full lifecycle API scenario — description length meets minimum requirement.',
      price: 120,
    });

    const adminToken = await getAdminToken(request);
    const designer = await provisionDesignerUser(request, adminToken, testInfo);
    cleanup.push(designer.userId);
    let designerAuth = await loginApiBearerOnly(request, designer.email, designer.password);

    await approveOrderApi(request, adminToken, order.id);
    await assignDesignerApi(request, adminToken, order.id, designer.userId);

    await updateOrderStatusAsAdminApi(request, adminToken, order.id, 'PreviewDelivered');
    let clientAuth = await loginApiBearerOnly(request, client.email, client.password);
    await requestRevisionApi(
      request,
      clientAuth.token,
      order.id,
      'Please refine spacing and balance — revision round one.'
    );
    await updateOrderStatusAsAdminApi(request, adminToken, order.id, 'PreviewDelivered');
    clientAuth = await loginApiBearerOnly(request, client.email, client.password);
    await approveLogoApi(request, clientAuth.token, order.id);
    await updateOrderStatusAsAdminApi(request, adminToken, order.id, 'Completed');

    const final = await getOrderApi(request, clientAuth.token, order.id);
    expect(final.status.toLowerCase()).toContain('completed');

    const invoice = await createInvoiceApi(request, adminToken, {
      orders: [{ orderId: order.id, price: 120 }],
      billingType: 1,
      taxAmount: 0,
    });
    expect(invoice.id).toBeTruthy();
  });
});
