/**
 * Root Playwright entry so `npx playwright test` from `Frontend/` picks up E2E settings
 * (baseURL, HTTPS ignore, `.env`). Prefer `npm run e2e` for the same config explicitly.
 */
export { default } from './e2e/playwright.config';
