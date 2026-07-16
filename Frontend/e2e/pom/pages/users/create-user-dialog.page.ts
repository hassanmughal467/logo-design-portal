import type { Page } from '@playwright/test';
import { PrimeNgDropdown } from '../../components/prime-ng-dropdown.component';

const DialogSelectors = {
  root: 'users-create-dialog',
  email: 'create-user-email',
  firstName: 'create-user-first-name',
  lastName: 'create-user-last-name',
  password: 'create-user-password',
  roleDropdown: 'create-user-role',
  submit: 'create-user-submit',
} as const;

/**
 * Modal dialog: "Create New User". Fragment object scoped to the dialog root.
 */
export class CreateUserDialogPage {
  constructor(private readonly page: Page) {}

  private root() {
    return this.page.getByRole('dialog', { name: /Create New User/i });
  }

  private roleDropdown() {
    return new PrimeNgDropdown(this.page, DialogSelectors.roleDropdown);
  }

  async fillAndSubmitDesigner(opts: {
    email: string;
    firstName: string;
    lastName: string;
    password: string;
  }): Promise<void> {
    const dlg = this.root();
    await dlg.getByTestId(DialogSelectors.email).fill(opts.email);
    await dlg.getByTestId(DialogSelectors.firstName).fill(opts.firstName);
    await dlg.getByTestId(DialogSelectors.lastName).fill(opts.lastName);
    await dlg.getByTestId(DialogSelectors.password).locator('input').fill(opts.password);
    await this.roleDropdown().selectOption('Designer');
    await dlg.getByTestId(DialogSelectors.submit).click();
  }

  async fillAndSubmitClient(opts: {
    email: string;
    firstName: string;
    lastName: string;
    password: string;
  }): Promise<void> {
    const dlg = this.root();
    await dlg.getByTestId(DialogSelectors.email).fill(opts.email);
    await dlg.getByTestId(DialogSelectors.firstName).fill(opts.firstName);
    await dlg.getByTestId(DialogSelectors.lastName).fill(opts.lastName);
    await dlg.getByTestId(DialogSelectors.password).locator('input').fill(opts.password);
    await this.roleDropdown().selectOption('Client');
    await dlg.locator('#invoiceEmail').fill(opts.email);
    await dlg.locator('#companyName').fill('E2E Client Co');
    await dlg.locator('#contactName').fill('E2E Contact');
    await dlg.locator('#phoneNumber').fill('+10000000001');
    await dlg.getByTestId(DialogSelectors.submit).click();
  }
}
