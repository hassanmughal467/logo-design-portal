import { expect } from '@playwright/test';
import type { Page } from '@playwright/test';

/** Minimal valid 1×1 PNG for required reference upload in create-order modal. */
const minimalPng = Buffer.from(
  'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==',
  'base64'
);

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
    await expect(this.page.getByRole('heading', { name: OrderCreateSelectors.modalHeading })).toBeVisible();
  }

  /** Attaches at least one file so submit passes reference-file validation. */
  async attachMinimalReferenceFile(): Promise<void> {
    await this.page.locator('app-order-create input[type="file"]').setInputFiles({
      name: 'e2e-reference.png',
      mimeType: 'image/png',
      buffer: minimalPng,
    });
  }

  async fillAndSubmit(title: string, description: string): Promise<void> {
    await this.page.getByTestId(OrderCreateSelectors.title).fill(title);
    await this.page.getByTestId(OrderCreateSelectors.description).fill(description);
    await this.attachMinimalReferenceFile();
    await this.page.getByTestId(OrderCreateSelectors.submit).click();
    await expect(this.page.getByRole('heading', { name: OrderCreateSelectors.modalHeading })).toBeHidden({
      timeout: 60_000,
    });
  }
}
