import { test, expect } from '@playwright/test';
import { apiUrl } from '../../utils/api-client';
import { getAdminToken } from '../../utils/api-helpers';

/**
 * Invoice API smoke: authenticated operator can list invoices (foundation for UI billing flows).
 */
test('admin can GET /api/invoices', async ({ request }) => {
  const token = await getAdminToken(request);
  const res = await request.get(apiUrl('/invoices'), {
    headers: { Authorization: `Bearer ${token}` },
  });
  expect(res.ok()).toBeTruthy();
  // GET /api/invoices returns a paged result: { items, total, page, pageSize, ... }
  const body = (await res.json()) as { items: unknown[]; total: number };
  expect(Array.isArray(body.items)).toBeTruthy();
  expect(typeof body.total).toBe('number');
});
