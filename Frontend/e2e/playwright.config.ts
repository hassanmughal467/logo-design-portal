import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright E2E config for Logo Design Portal.
 * Run: npx playwright test
 * Requires: Backend on http://localhost:5000, Frontend on http://localhost:4200
 */
export default defineConfig({
  testDir: './tests',
  testIgnore: ['**/src/**/*.spec.ts'],
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
  ],
  webServer: process.env.CI ? undefined : {
    command: 'ng serve --configuration=e2e',
    url: 'http://localhost:4200',
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },
});
