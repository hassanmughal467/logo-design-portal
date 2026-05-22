import { test, expect } from '@playwright/test';
import { protectedProbe } from '../../commands/auth.commands';
import { apiUrl } from '../../utils/api-client';

test.describe('Auth — token expiry / tampering', () => {
  test('expired-style JWT is rejected', async ({ request }) => {
    const res = await protectedProbe(
      request,
      'eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJzdWIiOiIxMjM0NTY3ODkwIiwiZXhwIjoxfQ.'
    );
    expect(res.status()).toBe(401);
  });

  test('refresh with invalid token returns 400 or 401', async ({ request }) => {
    const res = await request.post(apiUrl('/auth/refresh-token'),
      {
        data: { refreshToken: 'invalid-refresh' },
        headers: { 'Content-Type': 'application/json' },
      }
    );
    expect([400, 401]).toContain(res.status());
  });
});
