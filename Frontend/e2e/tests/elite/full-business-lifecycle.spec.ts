import { test, expect } from '@playwright/test';
import { LoginPage } from '../../pom';
import { createTestInvoice } from '../../factories/test-data.factory';
import { getAdminToken, provisionClientUser, provisionDesignerUser, teardownUsers } from '../../utils/api-helpers';
import { getAdminAnalyticsOverviewApi, getInvoiceApi, loginApi, markInvoicePaidApi } from '../../utils/api-client';
import { runCoreBusinessLifecycle } from '../../utils/business-flow';

test.describe.configure({ mode: 'serial' });

test.describe('Elite - full business lifecycle', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('order to payment updates API and UI checkpoints', async ({ page, request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const analyticsBefore = await getAdminAnalyticsOverviewApi(request, adminToken);

    const client = await provisionClientUser(request, adminToken, testInfo);
    const designer = await provisionDesignerUser(request, adminToken, testInfo);
    cleanup.push(client.userId, designer.userId);

    const clientAuth = await loginApi(request, client.email, client.password);
    const designerAuth = await loginApi(request, designer.email, designer.password);

    const flow = await runCoreBusinessLifecycle(request, testInfo, {
      adminToken,
      clientToken: clientAuth.token,
      designerUserId: designer.userId,
      designerToken: designerAuth.token,
    });

    const invoice = await createTestInvoice(request, adminToken, flow.orderId, 250);
    expect(invoice.id).toBeTruthy();
    await markInvoicePaidApi(request, adminToken, invoice.id, 'BankTransfer');
    const invoiceSnapshot = await getInvoiceApi(request, adminToken, invoice.id);
    expect(String(invoiceSnapshot.status).toLowerCase()).toContain('paid');

    const login = new LoginPage(page);
    await login.goto();
    await login.login(process.env.E2E_ADMIN_EMAIL!, process.env.E2E_ADMIN_PASSWORD!);
    await login.expectRedirectToDashboard();
    await expect(page.getByRole('heading', { name: /Dashboard/i })).toBeVisible();
    await page.goto('/invoices');
    await expect(page.getByText('Invoices Management')).toBeVisible();

    const analyticsAfter = await getAdminAnalyticsOverviewApi(request, adminToken);
    expect(analyticsAfter.totalOrders).toBeGreaterThanOrEqual(analyticsBefore.totalOrders);
  });
});
