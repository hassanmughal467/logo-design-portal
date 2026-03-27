import { test, expect } from '@playwright/test';
import {
  apiUrl,
  assignDesignerApi,
  createOrderApi,
  loginApi,
  approveOrderApi,
} from '../../utils/api-client';
import { getAdminToken, provisionClientUser, provisionDesignerUser, teardownUsers } from '../../utils/api-helpers';
import { uniqueSuffix } from '../../utils/test-data';

/**
 * Assignment + visibility: after admin assigns a designer, the designer API surface lists the order.
 */
test.describe.configure({ mode: 'serial' });

test.describe('Assign designer workflow', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('admin assigns designer; designer sees order in assigned-orders', async ({ request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const client = await provisionClientUser(request, adminToken, testInfo);
    const designer = await provisionDesignerUser(request, adminToken, testInfo);
    cleanup.push(client.userId, designer.userId);

    const clientAuth = await loginApi(request, client.email, client.password);
    const suffix = uniqueSuffix(testInfo);
    const order = await createOrderApi(request, clientAuth.token, {
      title: `Assign test ${suffix}`,
      description: 'Designer assignment E2E — minimum description length ok.',
    });

    await approveOrderApi(request, adminToken, order.id);
    await assignDesignerApi(request, adminToken, order.id, designer.userId);

    const designerAuth = await loginApi(request, designer.email, designer.password);
    const res = await request.get(apiUrl('/orders/assigned-orders'), {
      headers: { Authorization: `Bearer ${designerAuth.token}` },
    });
    expect(res.ok()).toBeTruthy();
    const list = (await res.json()) as Array<{ id: string; title: string }>;
    expect(list.some((o) => o.id === order.id)).toBeTruthy();
  });
});
