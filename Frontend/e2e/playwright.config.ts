import { defineConfig, devices, type ReporterDescription } from '@playwright/test';
import path from 'path';
import os from 'os';
import dotenv from 'dotenv';

/**
 * Load optional local secrets (never commit `e2e/.env`).
 * In CI, set the same variables in the workflow `env` block or repository secrets.
 */
dotenv.config({ path: path.join(__dirname, '.env') });

const adminStorageState = path.join(__dirname, '.auth', 'admin.json');

/** Artifacts next to this config (`e2e/`), keeps the Frontend root clean. */
const outputDir = path.join(__dirname, 'test-results');
const reportDir = path.join(__dirname, 'playwright-report');

const isCi = !!process.env.CI;

/**
 * Worker count: Playwright default uses ~half of logical CPUs (good for local).
 * On CI we cap parallelism so Angular + API + DB are not starved (fewer flakes).
 *
 * Override anytime: `PW_WORKERS=6 npx playwright test ...`
 */
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

/**
 * Retries: transient UI/network flakes without slowing every run on the happy path
 * (retries only execute after a failure).
 *
 * - CI: 2 (common default for forked PRs / shared runners)
 * - Local: 1 unless `E2E_RETRIES=0` for fastest feedback while iterating
 *
 * Override: `E2E_RETRIES=0|1|2|...`
 */
function resolveRetries(): number {
  if (process.env.E2E_RETRIES !== undefined && process.env.E2E_RETRIES !== '') {
    const n = parseInt(process.env.E2E_RETRIES, 10);
    return Number.isFinite(n) && n >= 0 ? n : 0;
  }
  return isCi ? 2 : 1;
}

/**
 * Reporters: always write HTML to a stable folder; list gives fast terminal feedback.
 * `github` annotations only when CI is set (matches Actions).
 */
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
 * Playwright configuration for the Logo Design Portal (Angular + ASP.NET API).
 *
 * Projects:
 * - `setup` — logs in once as the admin operator and writes `e2e/.auth/admin.json` (storageState).
 * - `admin-chromium` — role suites that should start already authenticated as that operator.
 * - `chromium` — anonymous / multi-actor flows (workflows log in as different users).
 *
 * Parallelism:
 * - `fullyParallel: true` + per-project workers so independent files run concurrently.
 * - `setup` runs first (no retries); `admin-chromium` and `chromium` run in parallel after it.
 * - Serial suites remain opt-in via `test.describe.configure({ mode: 'serial' })`.
 */
export default defineConfig({
  testDir: './tests',
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
    baseURL: process.env.E2E_BASE_URL ?? 'http://localhost:4200',
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
      testIgnore: [/auth\.setup\.ts/, /roles\/admin\/.*\.spec\.ts/],
      fullyParallel: true,
      use: {
        ...devices['Desktop Chrome'],
      },
    },
    {
      name: 'mobile-chrome',
      dependencies: [],
      testMatch: /smoke\/.*\.spec\.ts/,
      use: { ...devices['Pixel 5'] },
    },
  ],

  webServer: isCi
    ? undefined
    : {
        command: 'npx ng serve --configuration=e2e --port 4200',
        url: process.env.E2E_BASE_URL ?? 'http://localhost:4200',
        reuseExistingServer: !isCi,
        timeout: 180_000,
      },
});
