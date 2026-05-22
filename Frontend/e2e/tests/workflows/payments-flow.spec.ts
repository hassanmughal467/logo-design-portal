import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';
import {
  approveOrderApi,
  assignDesignerApi,
  approveLogoApi,
  createInvoiceApi,
  createOrderApi,
  getInvoiceApi,
  listInvoicesApi,
  loginApiBearerOnly,
  updateOrderStatusAsAdminApi,
  markInvoicePaidApi,
  updateOrderStatusApi,
} from '../../utils/api-client';
import {
  getAdminToken,
  provisionDesignerUser,
  provisionLoggedInClient,
  teardownUsers,
} from '../../utils/api-helpers';
import { uniqueSuffix } from '../../utils/test-data';

test.describe.configure({ mode: 'serial' });

test.describe('Payments — invoices and mark paid', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('admin lists invoices, marks paid; invoice API reflects Paid', async ({ page, request }, testInfo) => {
    const client = await provisionLoggedInClient(request, testInfo);
    cleanup.push(client.userId);
    const suffix = uniqueSuffix(testInfo);

    const order = await createOrderApi(request, client.token, {
      title: `Pay flow ${suffix}`,
      description: 'Payments E2E branch — long enough description for validation rules.',
      price: 200,
    });

    const adminToken = await getAdminToken(request);
    const designer = await provisionDesignerUser(request, adminToken, testInfo);
    cleanup.push(designer.userId);
    const designerAuth = await loginApiBearerOnly(request, designer.email, designer.password);

    await approveOrderApi(request, adminToken, order.id);
    await assignDesignerApi(request, adminToken, order.id, designer.userId);
    await updateOrderStatusAsAdminApi(request, adminToken, order.id, 'PreviewDelivered');
    const clientAuth = await loginApiBearerOnly(request, client.email, client.password);
    await approveLogoApi(request, clientAuth.token, order.id);
    await updateOrderStatusAsAdminApi(request, adminToken, order.id, 'Completed');

    const invoice = await createInvoiceApi(request, adminToken, {
      orders: [{ orderId: order.id, price: 200 }],
      billingType: 1,
      taxAmount: 0,
    });
    const listedBefore = await listInvoicesApi(request, adminToken);
    expect(listedBefore.some((i) => i.id === invoice.id)).toBeTruthy();

    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
    await loginPage.expectRedirectToDashboard();
    await page.goto('/invoices');
    await expect(page.getByText('Invoices Management')).toBeVisible({ timeout: 30_000 });

    await markInvoicePaidApi(request, adminToken, invoice.id, 'BankTransfer');
    const snapshot = await getInvoiceApi(request, adminToken, invoice.id);
    expect(String(snapshot.status).toLowerCase()).toContain('paid');
  });
});
