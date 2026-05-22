import { test, expect } from '@playwright/test';
import {
  apiUrl,
  approveOrderApi,
  assignDesignerApi,
  createInvoiceApi,
  createOrderApi,
  getInvoiceApi,
  loginApi,
  markInvoicePaidApi,
  updateOrderStatusApi,
} from '../../utils/api-client';
import { getAdminToken, provisionClientUser, provisionDesignerUser, teardownUsers } from '../../utils/api-helpers';
import { uniqueSuffix } from '../../utils/test-data';

test.describe.configure({ mode: 'serial' });

test.describe('Invoices — lifecycle', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('generate invoice, mark paid, duplicate mark-paid fails', async ({ request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const client = await provisionClientUser(request, adminToken, testInfo);
    const designer = await provisionDesignerUser(request, adminToken, testInfo);
    cleanup.push(client.userId, designer.userId);

    const clientAuth = await loginApi(request, client.email, client.password);
    const designerAuth = await loginApi(request, designer.email, designer.password);
    const suffix = uniqueSuffix(testInfo);

    const order = await createOrderApi(request, clientAuth.token, {
      title: `Invoice ${suffix}`,
      description: 'Invoice lifecycle E2E — sufficient description length.',
      price: 100,
    });
    await approveOrderApi(request, adminToken, order.id);
    await assignDesignerApi(request, adminToken, order.id, designer.userId);
    await updateOrderStatusApi(request, designerAuth.token, order.id, 'PreviewDelivered');
    await updateOrderStatusApi(request, adminToken, order.id, 'Completed');

    const invoice = await createInvoiceApi(request, adminToken, {
      orders: [{ orderId: order.id, price: 100 }],
    });
    await markInvoicePaidApi(request, adminToken, invoice.id, 'BankTransfer');
    const paid = await getInvoiceApi(request, clientAuth.token, invoice.id);
    expect(String(paid.status).toLowerCase()).toMatch(/paid/);

    const dup = await request.put(apiUrl(`/invoices/${invoice.id}/mark-paid`),
      {
        headers: { Authorization: `Bearer ${adminToken}`, 'Content-Type': 'application/json' },
        data: { paymentMethod: 'BankTransfer' },
      }
    );
    expect(dup.status()).toBe(400);
  });
});
