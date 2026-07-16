import { expect } from '@playwright/test';
import type { Page } from '@playwright/test';
import { BasePage } from '../../base.page';
import { CreateUserDialogPage } from './create-user-dialog.page';

const UsersSelectors = {
  openCreate: 'users-open-create',
  dialog: 'users-create-dialog',
} as const;

/**
 * Users management list (`/users`) — SuperAdmin / Admin with permission.
 */
export class UsersManagementPage extends BasePage {
  /** Dialog fragment — use after `openCreateUserDialog()`. */
  readonly createUserDialog: CreateUserDialogPage;

  constructor(page: Page) {
    super(page);
    this.createUserDialog = new CreateUserDialogPage(page);
  }

  async goto(): Promise<void> {
    await this.page.goto('/users');
    await expect(this.page.getByRole('heading', { name: /Users Management/i })).toBeVisible();
  }

  async openCreateUserDialog(): Promise<void> {
    await this.page.getByTestId(UsersSelectors.openCreate).click();
    // p-dialog host with data-testid stays aria-hidden; assert the open overlay instead.
    await expect(this.page.getByRole('dialog', { name: /Create New User/i })).toBeVisible();
  }

  async expectUserInTable(email: string): Promise<void> {
    await expect(this.page.getByRole('cell', { name: email, exact: true })).toBeVisible({
      timeout: 30_000,
    });
  }
}
