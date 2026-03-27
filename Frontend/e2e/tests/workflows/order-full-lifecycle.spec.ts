import { test, expect } from '@playwright/test';
import {
  approveLogoApi,
  approveOrderApi,
  assignDesignerApi,
  createInvoiceApi,
  createOrderApi,
  getOrderApi,
  loginApi,
  requestRevisionApi,
  updateOrderStatusApi,
} from '../../utils/api-client';
import { getAdminToken, provisionClientUser, provisionDesignerUser, teardownUsers } from '../../utils/api-helpers';
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
    const adminToken = await getAdminToken(request);
    const client = await provisionClientUser(request, adminToken, testInfo);
    const designer = await provisionDesignerUser(request, adminToken, testInfo);
    cleanup.push(client.userId, designer.userId);

    const clientAuth = await loginApi(request, client.email, client.password);
    const designerAuth = await loginApi(request, designer.email, designer.password);

    const suffix = uniqueSuffix(testInfo);
    const order = await createOrderApi(request, clientAuth.token, {
      title: `Lifecycle ${suffix}`,
      description: 'Full lifecycle API scenario — description length meets minimum requirement.',
      price: 120,
    });
    await approveOrderApi(request, adminToken, order.id);
    await assignDesignerApi(request, adminToken, order.id, designer.userId);

    await updateOrderStatusApi(request, designerAuth.token, order.id, 'PreviewDelivered');
    await requestRevisionApi(
      request,
      clientAuth.token,
      order.id,
      'Please refine spacing and balance — revision round one.'
    );
    await updateOrderStatusApi(request, designerAuth.token, order.id, 'PreviewDelivered');
    await approveLogoApi(request, clientAuth.token, order.id);
    await updateOrderStatusApi(request, adminToken, order.id, 'Completed');

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
