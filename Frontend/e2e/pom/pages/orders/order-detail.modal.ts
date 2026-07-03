import { expect } from '@playwright/test';
import type { Page } from '@playwright/test';

const OrderDetailSelectors = {
  approve: 'order-approve',
  approveSubmit: 'approve-submit',
} as const;

/**
 * Order detail dialog (admin actions such as Approve Order).
 */
export class OrderDetailModalPage {
  constructor(private readonly page: Page) {}

  /** p-dialog uses appendTo="body", so the overlay is not under app-order-detail. */
  private detailDialog() {
    return this.page.locator('.order-detail-dialog');
  }

  /** Closes the post-create detail dialog so header actions (logout) are clickable. */
  async closeIfOpen(): Promise<void> {
    const dialog = this.detailDialog();
    if (!(await dialog.isVisible().catch(() => false))) {
      return;
    }
    const closeBtn = dialog.locator('.p-dialog-header-close');
    if (await closeBtn.isVisible().catch(() => false)) {
      await closeBtn.click();
    } else {
      await this.page.keyboard.press('Escape');
    }
    await expect(dialog).toBeHidden({ timeout: 15_000 });
  }

  async expectApproveVisible(): Promise<void> {
    await expect(this.detailDialog().getByTestId(OrderDetailSelectors.approve)).toBeVisible({
      timeout: 30_000,
    });
  }

  /**
   * Header "Approve Order" opens a choice modal (Approve Only / Approve & Assign).
   * Default choice is "Approve Only" → order becomes ApprovedUnassigned.
   */
  async approveOrder(): Promise<void> {
    await this.detailDialog().getByTestId(OrderDetailSelectors.approve).click();
    const submit = this.page.getByTestId(OrderDetailSelectors.approveSubmit);
    await expect(submit).toBeVisible({ timeout: 15_000 });
    const approveResponse = this.page.waitForResponse(
      (res) => /\/api\/orders\/[^/]+\/approve/.test(res.url()) && res.request().method() === 'POST',
      { timeout: 30_000 }
    );
    await submit.click();
    const res = await approveResponse;
    expect(res.ok()).toBeTruthy();
  }
}
