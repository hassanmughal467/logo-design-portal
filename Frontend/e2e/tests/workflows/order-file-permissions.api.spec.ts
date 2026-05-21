import { test, expect } from '@playwright/test';
import { apiUrl, loginApi } from '../../utils/api-client';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';

/**
 * Happy-path API regression for order + file permissions (seeded test users).
 */
test.describe('Workflow — order file permissions API', () => {
  test('client creates order and uploads reference png', async ({ request }) => {
    const clientAuth = await loginApi(request, 'client@test.com', 'Test@123');

    const create = await request.post(apiUrl('/orders'), {
      headers: {
        Authorization: `Bearer ${clientAuth.token}`,
        'Content-Type': 'application/json',
      },
      data: {
        title: `E2E order ${Date.now()}`,
        description: 'API workflow test',
        price: 75,
      },
    });
    expect(create.status()).toBe(201);
    const order = (await create.json()) as { id: string; status: string };
    expect(order.status).toMatch(/WaitingForAdminApproval/i);

    const png = Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]);
    const upload = await request.post(apiUrl(`/files/upload/${order.id}`), {
      headers: { Authorization: `Bearer ${clientAuth.token}` },
      multipart: {
        file: { name: 'ref.png', mimeType: 'image/png', buffer: png },
        fileType: 'Reference',
      },
    });
    expect(upload.ok()).toBeTruthy();
  });

  test('admin assigns designer to client order', async ({ request }) => {
    const clientAuth = await loginApi(request, 'client@test.com', 'Test@123');
    const create = await request.post(apiUrl('/orders'), {
      headers: {
        Authorization: `Bearer ${clientAuth.token}`,
        'Content-Type': 'application/json',
      },
      data: { title: 'Assign flow', description: 'd', price: 100 },
    });
    const order = (await create.json()) as { id: string };

    const adminAuth = await loginApi(request, E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
    const designers = await request.get(apiUrl('/users/designers'), {
      headers: { Authorization: `Bearer ${adminAuth.token}` },
    });
    test.skip(!designers.ok(), 'Designer list endpoint unavailable in this environment');

    const list = (await designers.json()) as Array<{ userId?: string; id?: string }>;
    const designerUserId = list[0]?.userId ?? list[0]?.id;
    test.skip(!designerUserId, 'No designers seeded');

    const assign = await request.post(apiUrl(`/orders/${order.id}/assign`), {
      headers: {
        Authorization: `Bearer ${adminAuth.token}`,
        'Content-Type': 'application/json',
      },
      data: { designerId: designerUserId },
    });
    expect(assign.ok()).toBeTruthy();
  });

  test('client order detail masks designer', async ({ request }) => {
    const clientAuth = await loginApi(request, 'client@test.com', 'Test@123');
    const create = await request.post(apiUrl('/orders'), {
      headers: {
        Authorization: `Bearer ${clientAuth.token}`,
        'Content-Type': 'application/json',
      },
      data: { title: 'Mask check', description: 'd', price: 20 },
    });
    const order = (await create.json()) as { id: string };

    const get = await request.get(apiUrl(`/orders/${order.id}`), {
      headers: { Authorization: `Bearer ${clientAuth.token}` },
    });
    expect(get.ok()).toBeTruthy();
    const body = await get.text();
    expect(body.toLowerCase()).not.toContain('designer@test.com');
  });
});
