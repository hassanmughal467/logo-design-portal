import { expect, type Browser, type BrowserContext, type Page } from '@playwright/test';
import { LoginPage } from '../pom';

export type RoleSession = {
  context: BrowserContext;
  page: Page;
};

/**
 * Creates an isolated browser context for one user account.
 */
export async function createRoleSession(
  browser: Browser,
  creds: { email: string; password: string }
): Promise<RoleSession> {
  const context = await browser.newContext();
  const page = await context.newPage();
  const login = new LoginPage(page);

  await login.goto();
  await login.login(creds.email, creds.password);
  await login.expectRedirectToDashboard();
  await expect(page).toHaveURL(/dashboard/i);

  return { context, page };
}

export async function closeRoleSessions(sessions: RoleSession[]): Promise<void> {
  await Promise.all(sessions.map((s) => s.context.close()));
}
