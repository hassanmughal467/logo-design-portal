import { test, expect } from '@playwright/test';
import { apiUrl, loginApi } from '../../utils/api-client';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';

/**
 * API-level security tests for order/file/auth module (no UI dependency).
 */
test.describe('Security — order, file, auth module', () => {
  test('unauthenticated order create returns 401', async ({ request }) => {
    const res = await request.post(apiUrl('/orders'), {
      data: { title: 'x', description: 'y', price: 1 },
      headers: { 'Content-Type': 'application/json' },
    });
    expect(res.status()).toBe(401);
  });

  test('client cannot list all orders (ViewAllOrders)', async ({ request }) => {
    const auth = await loginApi(request, 'client@test.com', 'Test@123');
    const res = await request.get(apiUrl('/orders?page=1&pageSize=5'), {
      headers: { Authorization: `Bearer ${auth.token}` },
    });
    expect(res.status()).toBe(403);
  });

  test('designer cannot assign order', async ({ request }) => {
    const clientAuth = await loginApi(request, 'client@test.com', 'Test@123');
    const create = await request.post(apiUrl('/orders'), {
      headers: {
        Authorization: `Bearer ${clientAuth.token}`,
        'Content-Type': 'application/json',
      },
      data: { title: 'E2E assign block', description: 'd', price: 50 },
    });
    expect(create.ok()).toBeTruthy();
    const order = (await create.json()) as { id: string };

    const designerAuth = await loginApi(request, 'designer@test.com', 'Test@123');
    const assign = await request.post(apiUrl(`/orders/${order.id}/assign`), {
      headers: {
        Authorization: `Bearer ${designerAuth.token}`,
        'Content-Type': 'application/json',
      },
      data: { designerId: designerAuth.user.id },
    });
    expect(assign.status()).toBe(403);
  });

  test('invalid status transition returns 400', async ({ request }) => {
    const clientAuth = await loginApi(request, 'client@test.com', 'Test@123');
    const create = await request.post(apiUrl('/orders'), {
      headers: {
        Authorization: `Bearer ${clientAuth.token}`,
        'Content-Type': 'application/json',
      },
      data: { title: 'E2E status', description: 'd', price: 50 },
    });
    const order = (await create.json()) as { id: string };

    const adminAuth = await loginApi(request, E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
    const bad = await request.put(apiUrl(`/orders/${order.id}/status`), {
      headers: {
        Authorization: `Bearer ${adminAuth.token}`,
        'Content-Type': 'application/json',
      },
      data: { status: 'Completed', notes: 'invalid jump' },
    });
    expect(bad.status()).toBe(400);
  });

  test('upload with disallowed extension returns 400', async ({ request }) => {
    const clientAuth = await loginApi(request, 'client@test.com', 'Test@123');
    const create = await request.post(apiUrl('/orders'), {
      headers: {
        Authorization: `Bearer ${clientAuth.token}`,
        'Content-Type': 'application/json',
      },
      data: { title: 'E2E upload', description: 'd', price: 10 },
    });
    const order = (await create.json()) as { id: string };

    const form = {
      file: {
        name: 'bad.exe',
        mimeType: 'application/octet-stream',
        buffer: Buffer.from([0x4d, 0x5a]),
      },
      fileType: 'Reference',
    };

    const upload = await request.post(apiUrl(`/files/upload/${order.id}`), {
      headers: { Authorization: `Bearer ${clientAuth.token}` },
      multipart: form,
    });
    expect(upload.status()).toBe(400);
  });
});
