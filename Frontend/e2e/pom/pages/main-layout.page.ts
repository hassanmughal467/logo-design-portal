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
    await this.openUserMenu();

    await this.page.locator('.p-menu-overlay').waitFor({
      state: 'visible',
    });

    await this.page.getByText('Logout').click();

    await this.page.waitForURL(/\/(auth\/)?login/);
  }

  async expectUserHeaderVisible(): Promise<void> {
    await expect(this.page.getByTestId(LayoutSelectors.userMenu)).toBeVisible();
  }
}
