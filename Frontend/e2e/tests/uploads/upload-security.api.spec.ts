import { test, expect } from '@playwright/test';
import { createOrderApi, uploadFileApi } from '../../utils/api-client';
import { EXE_MAGIC, PDF_MAGIC, PNG_HEADER, oversizedBuffer } from '../../fixtures/upload.fixtures';
import { provisionLoggedInClient, teardownUsers } from '../../utils/api-helpers';
import { uniqueSuffix } from '../../utils/test-data';

test.describe('Uploads — security', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('valid PNG upload succeeds', async ({ request }, testInfo) => {
    const client = await provisionLoggedInClient(request, testInfo);
    cleanup.push(client.userId);
    const order = await createOrderApi(request, client.token, {
      title: `Upload ${uniqueSuffix(testInfo)}`,
      description: 'Upload security test order with valid reference file.',
      price: 10,
    });
    const res = await uploadFileApi(request, client.token, order.id, 'ref.png', PNG_HEADER, 'image/png');
    expect(res.status()).toBe(200);
  });

  test('exe extension rejected', async ({ request }, testInfo) => {
    const client = await provisionLoggedInClient(request, testInfo);
    cleanup.push(client.userId);
    const order = await createOrderApi(request, client.token, {
      title: `Upload exe ${uniqueSuffix(testInfo)}`,
      description: 'Reject executable uploads at API boundary.',
      price: 10,
    });
    const res = await uploadFileApi(request, client.token, order.id, 'malware.exe', EXE_MAGIC, 'application/octet-stream');
    expect(res.status()).toBe(400);
  });

  test('png extension with pdf magic bytes rejected', async ({ request }, testInfo) => {
    const client = await provisionLoggedInClient(request, testInfo);
    cleanup.push(client.userId);
    const order = await createOrderApi(request, client.token, {
      title: `Upload fake ${uniqueSuffix(testInfo)}`,
      description: 'Extension bypass must fail content sniffing.',
      price: 10,
    });
    const res = await uploadFileApi(request, client.token, order.id, 'fake.png', PDF_MAGIC, 'image/png');
    expect(res.status()).toBe(400);
  });

  test('oversized upload rejected', async ({ request }, testInfo) => {
    const client = await provisionLoggedInClient(request, testInfo);
    cleanup.push(client.userId);
    const order = await createOrderApi(request, client.token, {
      title: `Upload big ${uniqueSuffix(testInfo)}`,
      description: 'Oversized payload should be rejected by server limits.',
      price: 10,
    });
    const res = await uploadFileApi(
      request,
      client.token,
      order.id,
      'huge.png',
      oversizedBuffer(6),
      'image/png'
    );
    expect([400, 413]).toContain(res.status());
  });
});
