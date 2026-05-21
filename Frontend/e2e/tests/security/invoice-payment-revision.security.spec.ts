import { test, expect } from '@playwright/test';
import { apiUrl, loginApi } from '../../utils/api-client';

test.describe('Security — invoice payment revision API', () => {
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

  test('designer cannot request client revision', async ({ request }) => {
    const designerAuth = await loginApi(request, 'designer@test.com', 'Test@123');
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
