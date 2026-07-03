import { test, expect } from '@playwright/test';
import { apiUrl, loginApiBearerOnly } from '../../utils/api-client';
import { getAdminToken, provisionDesignerUser, teardownUsers } from '../../utils/api-helpers';

test.describe('Security — invoice payment revision API', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('unauthenticated invoice list returns 401', async ({ request }) => {
    const response = await request.get(apiUrl('/invoices'));
    expect(response.status()).toBe(401);
  });

  test('paypal webhook without headers returns 401', async ({ request }) => {
    const response = await request.post(apiUrl('/payments/webhook/paypal'), {
      data: { event_type: 'PAYMENT.CAPTURE.COMPLETED' },
    });
    expect(response.status()).toBe(401);
  });

  test('designer cannot request client revision', async ({ request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const designer = await provisionDesignerUser(request, adminToken, testInfo);
    cleanup.push(designer.userId);

    const designerAuth = await loginApiBearerOnly(request, designer.email, designer.password);
    const orderId = '00000000-0000-0000-0000-000000000099';
    const response = await request.post(apiUrl(`/revisions/orders/${orderId}/request`), {
      headers: { Authorization: `Bearer ${designerAuth.token}` },
      multipart: {
        instructions: 'Should be rejected by role guard.',
      },
    });
    expect(response.status()).toBe(403);
  });
});
