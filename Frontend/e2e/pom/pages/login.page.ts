import { expect } from '@playwright/test';
import type { Page } from '@playwright/test';
import { BasePage } from '../base.page';

/** Routes and `data-testid` values for the login screen — single source of truth for locators. */
const LoginSelectors = {
  email: 'login-email',
  password: 'login-password',
  submit: 'login-submit',
} as const;

/**
 * Authentication page (`/auth/login`).
 */
export class LoginPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  async goto(): Promise<void> {
    await this.page.goto('/auth/login');
  }

  async login(email: string, password: string): Promise<void> {
    await this.page.getByTestId(LoginSelectors.email).fill(email);
    await this.page.getByTestId(LoginSelectors.password).locator('input').fill(password);
    await this.page.getByTestId(LoginSelectors.submit).click();
  }

  async expectLoginError(): Promise<void> {
    await expect(this.page.locator('.p-error').first()).toBeVisible();
  }

  async expectRedirectToDashboard(): Promise<void> {
    try {
      await this.page.waitForURL(/\/dashboard/, { timeout: 45_000 });
    } catch {
      throw new Error(
        `Expected redirect to /dashboard after login, but URL is "${this.page.url()}". ` +
          `Check: (1) API running and login credentials; (2) Angular \`environment.e2e.ts\` apiUrl matches the API; ` +
          `(3) browser origin is allowed by API CORS (localhost:4200 and 127.0.0.1:4200).`
      );
    }
  }
}
