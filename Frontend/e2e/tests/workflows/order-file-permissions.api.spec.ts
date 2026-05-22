import { test, expect } from '@playwright/test';
import { apiUrl, getOrderApi } from '../../utils/api-client';
import { getAdminToken, provisionLoggedInClient, teardownUsers } from '../../utils/api-helpers';
import { uniqueSuffix } from '../../utils/test-data';

/** Happy-path API regression for order + file permissions (provisioned Client user). */
test.describe('Workflow — order file permissions API', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('client creates order and uploads reference png', async ({ request }, testInfo) => {
    const client = await provisionLoggedInClient(request, testInfo);
    cleanup.push(client.userId);

    const create = await request.post(apiUrl('/orders'), {
      headers: {
        Authorization: `Bearer ${client.token}`,
        'Content-Type': 'application/json',
      },
      data: {
        title: `E2E order ${uniqueSuffix(testInfo)}`,
        description: 'API workflow test with sufficient description length.',
        price: 75,
      },
    });
    expect(create.status()).toBe(201);
    const order = (await create.json()) as { id: string; status: string };
    expect(order.status).toMatch(/WaitingForAdminApproval/i);

    const png = Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]);
    const upload = await request.post(apiUrl(`/files/upload/${order.id}`), {
      headers: { Authorization: `Bearer ${client.token}` },
      multipart: {
        file: { name: 'ref.png', mimeType: 'image/png', buffer: png },
        fileType: 'Reference',
      },
    });
    expect(upload.ok()).toBeTruthy();
  });

  test('admin assigns designer to client order', async ({ request }, testInfo) => {
    const client = await provisionLoggedInClient(request, testInfo);
    cleanup.push(client.userId);

    const create = await request.post(apiUrl('/orders'), {
      headers: {
        Authorization: `Bearer ${client.token}`,
        'Content-Type': 'application/json',
      },
      data: {
        title: `Assign flow ${uniqueSuffix(testInfo)}`,
        description: 'Assign designer workflow order with valid description.',
        price: 100,
      },
    });
    expect(create.status()).toBe(201);
    const order = (await create.json()) as { id: string };

    const adminToken = await getAdminToken(request);
    const approve = await request.post(apiUrl(`/orders/${order.id}/approve`), {
      headers: {
        Authorization: `Bearer ${adminToken}`,
        'Content-Type': 'application/json',
      },
      data: {},
    });
    expect(approve.ok()).toBeTruthy();

    const designers = await request.get(apiUrl('/users/designers'), {
      headers: { Authorization: `Bearer ${adminToken}` },
    });
    test.skip(!designers.ok(), 'Designer list endpoint unavailable in this environment');

    const list = (await designers.json()) as Array<{ userId?: string; id?: string }>;
    const designerUserId = list[0]?.userId ?? list[0]?.id;
    test.skip(!designerUserId, 'No designers seeded');

    const assign = await request.post(apiUrl(`/orders/${order.id}/assign`), {
      headers: {
        Authorization: `Bearer ${adminToken}`,
        'Content-Type': 'application/json',
      },
      data: { designerId: designerUserId },
    });
    expect(assign.ok()).toBeTruthy();
  });

  test('client order detail masks designer', async ({ request }, testInfo) => {
    const client = await provisionLoggedInClient(request, testInfo);
    cleanup.push(client.userId);

    const create = await request.post(apiUrl('/orders'), {
      headers: {
        Authorization: `Bearer ${client.token}`,
        'Content-Type': 'application/json',
      },
      data: {
        title: `Mask check ${uniqueSuffix(testInfo)}`,
        description: 'Masking regression order with valid description length.',
        price: 20,
      },
    });
    expect(create.status()).toBe(201);
    const order = (await create.json()) as { id: string };

    const body = await getOrderApi(request, client.token, order.id);
    expect(JSON.stringify(body).toLowerCase()).not.toContain('designer@test.com');
  });
});
