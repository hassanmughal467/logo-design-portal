import { expect } from '@playwright/test';
import type { Page } from '@playwright/test';
import { BasePage } from '../base.page';

const LayoutSelectors = {
  userMenu: 'header-user-menu',
} as const;

/**
 * Shared chrome after login: top bar, user menu, logout.
 */
export class MainLayoutPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  async openUserMenu(): Promise<void> {
    await this.page.getByTestId(LayoutSelectors.userMenu).click();
  }

  async logout(): Promise<void> {
    // Dismiss any lingering dialogs/toasts that can intercept the header click.
    await this.page.keyboard.press('Escape').catch(() => {});
    await this.page.getByTestId(LayoutSelectors.userMenu).click({ force: true });
    const logoutItem = this.page.getByText(/^Logout$/i).last();
    await expect(logoutItem).toBeVisible({ timeout: 10_000 });
    await logoutItem.click({ force: true });
    await this.page.waitForURL(/\/(auth\/)?login/);
  }

  async expectUserHeaderVisible(): Promise<void> {
    await expect(this.page.getByTestId(LayoutSelectors.userMenu)).toBeVisible();
  }
}
