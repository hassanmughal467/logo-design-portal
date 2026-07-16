import { expect } from '@playwright/test';
import type { Page } from '@playwright/test';

const OrderCreateSelectors = {
  modalHeading: /Create New Order/i,
  title: 'order-create-title',
  description: 'order-create-description',
  submit: 'order-create-submit',
} as const;

/**
 * "Create New Order" dialog hosted on the orders page.
 */
export class OrderCreateModalPage {
  constructor(private readonly page: Page) {}

  async expectVisible(): Promise<void> {
    await expect(this.page.getByRole('dialog', { name: OrderCreateSelectors.modalHeading })).toBeVisible();
  }

  async fillAndSubmit(title: string, description: string): Promise<void> {
    await this.page.getByTestId(OrderCreateSelectors.title).fill(title);
    await this.page.getByTestId(OrderCreateSelectors.description).fill(description);
    await this.page.getByTestId(OrderCreateSelectors.submit).click();
    await expect(this.page.getByRole('dialog', { name: OrderCreateSelectors.modalHeading })).toBeHidden({
      timeout: 60_000,
    });
  }
}
