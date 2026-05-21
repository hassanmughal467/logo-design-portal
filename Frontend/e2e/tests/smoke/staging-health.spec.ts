import { test, expect } from '@playwright/test';

const stagingApiUrl = process.env.STAGING_API_URL?.replace(/\/$/, '');

test.describe('Staging smoke (optional)', () => {
  test.skip(!stagingApiUrl, 'Set STAGING_API_URL to run staging smoke tests');

  test('health live returns success', async ({ request }) => {
    const response = await request.get(`${stagingApiUrl}/health/live`);
    expect(response.ok()).toBeTruthy();
  });

  test('health ready returns success', async ({ request }) => {
    const response = await request.get(`${stagingApiUrl}/health/ready`);
    expect(response.ok()).toBeTruthy();
  });
});
