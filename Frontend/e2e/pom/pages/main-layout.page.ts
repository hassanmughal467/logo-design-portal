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
    // PrimeNG popup menu may render as menuitem or plain text link depending on version.
    const logoutItem = this.page
      .locator('.p-menu, [role="menu"]')
      .getByText(/^Logout$/i)
      .or(this.page.getByRole('menuitem', { name: /^Logout$/i }));
    await logoutItem.first().click();
    await this.page.waitForURL(/\/(auth\/)?login/);
  }

  async expectUserHeaderVisible(): Promise<void> {
    await expect(this.page.getByTestId(LayoutSelectors.userMenu)).toBeVisible();
  }
}
