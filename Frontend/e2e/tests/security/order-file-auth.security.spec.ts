import { test, expect } from '@playwright/test';
import { apiUrl, createOrderApi, loginApi, loginApiBearerOnly } from '../../utils/api-client';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from '../../utils/env';
import {
  getAdminToken,
  provisionDesignerUser,
  provisionLoggedInClient,
  teardownUsers,
} from '../../utils/api-helpers';

/**
 * API-level security tests for order/file/auth module (no UI dependency).
 * Users are provisioned per spec so the suite is deterministic on any database.
 */
test.describe('Security — order, file, auth module', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('unauthenticated order create returns 401', async ({ request }) => {
    const res = await request.post(apiUrl('/orders'), {
      data: { title: 'x', description: 'y', price: 1 },
      headers: { 'Content-Type': 'application/json' },
    });
    expect(res.status()).toBe(401);
  });

  test('client cannot list all orders (ViewAllOrders)', async ({ request }, testInfo) => {
    const client = await provisionLoggedInClient(request, testInfo);
    cleanup.push(client.userId);
    const res = await request.get(apiUrl('/orders?page=1&pageSize=5'), {
      headers: { Authorization: `Bearer ${client.token}` },
    });
    expect(res.status()).toBe(403);
  });

  test('designer cannot assign order', async ({ request }, testInfo) => {
    const client = await provisionLoggedInClient(request, testInfo);
    cleanup.push(client.userId);
    const order = await createOrderApi(request, client.token, {
      title: 'E2E assign block',
      description: 'Designer must not be able to assign orders to themselves.',
      price: 50,
    });

    const adminToken = await getAdminToken(request);
    const designer = await provisionDesignerUser(request, adminToken, testInfo);
    cleanup.push(designer.userId);
    const designerAuth = await loginApiBearerOnly(request, designer.email, designer.password);

    const assign = await request.post(apiUrl(`/orders/${order.id}/assign`), {
      headers: {
        Authorization: `Bearer ${designerAuth.token}`,
        'Content-Type': 'application/json',
      },
      data: { designerId: designerAuth.user.id },
    });
    expect(assign.status()).toBe(403);
  });

  test('invalid status transition returns 400', async ({ request }, testInfo) => {
    const client = await provisionLoggedInClient(request, testInfo);
    cleanup.push(client.userId);
    const order = await createOrderApi(request, client.token, {
      title: 'E2E status',
      description: 'Invalid status jumps must be rejected by the state machine.',
      price: 50,
    });

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

  test('upload with disallowed extension returns 400', async ({ request }, testInfo) => {
    const client = await provisionLoggedInClient(request, testInfo);
    cleanup.push(client.userId);
    const order = await createOrderApi(request, client.token, {
      title: 'E2E upload',
      description: 'Executable uploads must be blocked by file validation.',
      price: 10,
    });

    const form = {
      file: {
        name: 'bad.exe',
        mimeType: 'application/octet-stream',
        buffer: Buffer.from([0x4d, 0x5a]),
      },
      fileType: 'Reference',
    };

    const upload = await request.post(apiUrl(`/files/upload/${order.id}`), {
      headers: { Authorization: `Bearer ${client.token}` },
      multipart: form,
    });
    expect(upload.status()).toBe(400);
  });
});
