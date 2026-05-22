import type { Page } from '@playwright/test';
import { E2E_BASE_URL } from './env';

/** Absolute app URL — works even when Playwright `baseURL` is not inherited by a project. */
export function appUrl(path: string): string {
  const normalized = path.startsWith('/') ? path : `/${path}`;
  return new URL(normalized, E2E_BASE_URL).href;
}

/** Login route after AuthGuard / RoleGuard reject unauthenticated navigation. */
export const loginUrlPattern = /\/auth\/login|\/login/;

/**
 * Visit a protected route with no session and wait for redirect to login.
 * Uses `commit` navigation (heavy lazy routes like invoices) and clears persisted auth first.
 */
export async function gotoExpectUnauthenticatedRedirect(
  page: Page,
  path: string,
  options?: { navigationTimeout?: number; redirectTimeout?: number }
): Promise<void> {
  await page.context().clearCookies();
  await page.addInitScript(() => {
    localStorage.clear();
    sessionStorage.clear();
  });
  await page.goto(appUrl(path), {
    waitUntil: 'commit',
    timeout: options?.navigationTimeout ?? 60_000,
  });
  await page.waitForURL(loginUrlPattern, {
    timeout: options?.redirectTimeout ?? 30_000,
  });
}
