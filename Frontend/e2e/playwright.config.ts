import { defineConfig, devices, type ReporterDescription } from '@playwright/test';
import path from 'path';
import os from 'os';
import dotenv from 'dotenv';

/**
 * Load optional local secrets (never commit `e2e/.env`).
 * In CI, set the same variables in the workflow `env` block or repository secrets.
 */
dotenv.config({ path: path.join(__dirname, '.env') });

const e2eRoot = __dirname;
const testDir = path.join(e2eRoot, 'tests');
const adminStorageState = path.join(e2eRoot, '.auth', 'admin.json');
const outputDir = path.join(e2eRoot, 'test-results');
const reportDir = path.join(e2eRoot, 'playwright-report');

/** True on GitHub Actions — not when `e2e/.env` sets CI=true locally (that broke webServer). */
const isCi = Boolean(process.env.CI) && Boolean(process.env.GITHUB_ACTIONS);
/** Skip Playwright `webServer` when Angular is already on :4200 (see `e2e/.env`). */
const reuseAppServer = process.env.E2E_REUSE_SERVER === 'true';

function resolveWorkers(): number | undefined {
  if (process.env.PW_WORKERS !== undefined && process.env.PW_WORKERS !== '') {
    const n = parseInt(process.env.PW_WORKERS, 10);
    if (Number.isFinite(n) && n > 0) {
      return n;
    }
  }
  if (isCi) {
    const cpus = os.cpus().length || 2;
    return Math.min(4, Math.max(2, Math.floor(cpus / 2)));
  }
  return undefined;
}

function resolveRetries(): number {
  if (process.env.E2E_RETRIES !== undefined && process.env.E2E_RETRIES !== '') {
    const n = parseInt(process.env.E2E_RETRIES, 10);
    return Number.isFinite(n) && n >= 0 ? n : 0;
  }
  return isCi ? 2 : 1;
}

function buildReporters(): ReporterDescription[] {
  const reporters: ReporterDescription[] = [
    ['list', { printSteps: false }],
    [
      'html',
      {
        outputFolder: reportDir,
        open: isCi ? 'never' : 'on-failure',
      },
    ],
  ];
  if (isCi) {
    reporters.push(['github']);
  }
  return reporters;
}

/**
 * Playwright — Logo Design Portal E2E.
 * `baseURL` / `ignoreHTTPSErrors` live only on global `use`; projects add devices (and auth state).
 */
export default defineConfig({
  testDir,
  testMatch: '**/*.spec.ts',
  outputDir,

  fullyParallel: true,
  forbidOnly: isCi,
  retries: resolveRetries(),
  workers: resolveWorkers(),

  reporter: buildReporters(),

  timeout: 90_000,
  expect: {
    timeout: 15_000,
    toHaveScreenshot: { maxDiffPixels: 280, animations: 'disabled' },
  },

  use: {
    baseURL: 'http://localhost:4200',
    ignoreHTTPSErrors: true,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: isCi ? 'retain-on-failure' : 'off',
    actionTimeout: 15_000,
    navigationTimeout: 45_000,
  },

  projects: [
    {
      name: 'setup',
      testMatch: /auth\.setup\.ts/,
      fullyParallel: false,
      retries: 0,
    },
    {
      name: 'admin-chromium',
      dependencies: ['setup'],
      testMatch: /roles\/admin\/.*\.spec\.ts/,
      fullyParallel: true,
      use: {
        ...devices['Desktop Chrome'],
        storageState: adminStorageState,
      },
    },
    {
      name: 'chromium',
      dependencies: [],
      testIgnore: [/auth\.setup\.ts/, /roles\/admin\/.*\.spec\.ts/, /smoke\/.*\.spec\.ts/],
      fullyParallel: true,
      use: {
        ...devices['Desktop Chrome'],
      },
    },
    {
      name: 'mobile-chrome',
      dependencies: [],
      testMatch: /smoke\/.*\.spec\.ts/,
      use: {
        ...devices['Pixel 5'],
      },
    },
  ],

  webServer: reuseAppServer || isCi
    ? undefined
    : {
        command: 'npx ng serve --configuration=e2e --port 4200',
        url: 'http://localhost:4200',
        reuseExistingServer: true,
        timeout: 180_000,
      },
});
