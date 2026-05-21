import { test, expect } from '@playwright/test';

/**
 * Placeholder for file-download IDOR E2E.
 * Backend coverage: FilesControllerDownloadSecurityTests + FileServiceDownloadVisibilityTests.
 * Extend with api-helpers order/file seeding when E2E environment provides stable fixtures.
 */
test.describe('File download IDOR', () => {
  test.skip(true, 'Enable when E2E seed creates client order + hidden preview file');

  test('client cannot download hidden preview by GUID', async () => {
    expect(true).toBeTruthy();
  });
});
