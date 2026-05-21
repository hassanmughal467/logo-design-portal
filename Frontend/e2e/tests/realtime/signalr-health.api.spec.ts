import { test, expect } from '@playwright/test';
import { healthReadyApi } from '../../utils/api-client';

test.describe('Realtime — infrastructure readiness', () => {
  test('ready health includes signalr check', async ({ request }) => {
    const res = await healthReadyApi(request);
    if (res.status() === 404) {
      test.skip(true, 'Health endpoint not available in this environment');
    }
    expect(res.ok()).toBeTruthy();
    const body = await res.text();
    expect(body.toLowerCase()).toContain('signalr');
  });
});
