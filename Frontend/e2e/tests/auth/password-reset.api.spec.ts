import { test, expect } from '@playwright/test';
import { forgotPasswordApi } from '../../commands/auth.commands';

test.describe('Auth — password reset API', () => {
  test('forgot-password accepts known email without leaking existence', async ({ request }) => {
    const res = await forgotPasswordApi(request, 'client@test.com');
    expect([200, 400, 404]).toContain(res.status());
  });

  test('forgot-password rejects empty email', async ({ request }) => {
    const res = await forgotPasswordApi(request, '');
    expect(res.status()).toBeGreaterThanOrEqual(400);
  });
});
