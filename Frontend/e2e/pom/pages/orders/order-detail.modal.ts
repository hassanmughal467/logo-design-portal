import { expect } from '@playwright/test';
import type { Page } from '@playwright/test';

const OrderDetailSelectors = {
  approve: 'order-approve',
} as const;

/**
 * Order detail dialog (admin actions such as Approve Order).
 */
export class OrderDetailModalPage {
  constructor(private readonly page: Page) {}

  async expectApproveVisible(): Promise<void> {
    await expect(this.page.getByTestId(OrderDetailSelectors.approve)).toBeVisible({ timeout: 30_000 });
  }

  async approveOrder(): Promise<void> {
    await this.page.getByTestId(OrderDetailSelectors.approve).click();
  }
}
