import { test, expect } from '@playwright/test';
import { apiUrl, extractPagedItems } from '../../utils/api-client';
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
  const body = await res.json();
  const items = extractPagedItems(body);
  expect(Array.isArray(items)).toBeTruthy();
});
