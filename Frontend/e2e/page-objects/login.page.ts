import { Page } from '@playwright/test';

export class LoginPage {
  constructor(private readonly page: Page) {}

  async goto() {
    await this.page.goto('/auth/login');
  }

  async login(email: string, password: string) {
    await this.page.getByLabel('Email').fill(email);
    await this.page.locator('input[type="password"]').fill(password);
    await this.page.getByRole('button', { name: 'Login' }).click();
  }

  async expectLoginError() {
    await this.page.locator('.p-error').first().waitFor({ state: 'visible' });
  }

  async expectRedirectToDashboard() {
    await this.page.waitForURL(/\/dashboard/);
  }
}
