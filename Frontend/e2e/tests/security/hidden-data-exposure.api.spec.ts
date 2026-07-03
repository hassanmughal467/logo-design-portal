import { test, expect } from '@playwright/test';
import {
  apiUrl,
  approveOrderApi,
  assignDesignerApi,
  createOrderApi,
  loginApi,
} from '../../utils/api-client';
import { getAdminToken, provisionClientUser, provisionDesignerUser, teardownUsers } from '../../utils/api-helpers';
import { uniqueSuffix } from '../../utils/test-data';

test.describe('Security — hidden data exposure', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('designer assigned-orders does not include client email', async ({ request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const client = await provisionClientUser(request, adminToken, testInfo);
    const designer = await provisionDesignerUser(request, adminToken, testInfo);
    cleanup.push(client.userId, designer.userId);

    const clientAuth = await loginApi(request, client.email, client.password);
    const designerAuth = await loginApi(request, designer.email, designer.password);
    const order = await createOrderApi(request, clientAuth.token, {
      title: `Hide ${uniqueSuffix(testInfo)}`,
      description: 'Designer must not see client PII in list responses.',
      price: 40,
    });
    await approveOrderApi(request, adminToken, order.id);
    await assignDesignerApi(request, adminToken, order.id, designer.userId);

    // Designers list their work via /orders/assigned-orders (my-orders is Client-only).
    const res = await request.get(apiUrl('/orders/assigned-orders'), {
      headers: { Authorization: `Bearer ${designerAuth.token}` },
    });
    expect(res.ok()).toBeTruthy();
    const body = await res.text();
    expect(body).not.toContain(client.email);
  });
});
