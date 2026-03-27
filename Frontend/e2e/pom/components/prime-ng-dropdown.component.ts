import type { Page } from '@playwright/test';

/**
 * Reusable wrapper for PrimeNG `p-dropdown` when the host has a stable `data-testid`.
 * Overlays attach to `body`; this waits for the panel and avoids timing flakes.
 */
export class PrimeNgDropdown {
  constructor(
    private readonly page: Page,
    private readonly hostTestId: string
  ) {}

  async selectOption(optionLabel: string): Promise<void> {
    const host = this.page.getByTestId(this.hostTestId);
    await host.click();
    const panel = this.page.locator('.p-dropdown-panel:visible');
    await panel.waitFor({ state: 'visible' });
    const item = panel.locator('li.p-dropdown-item').filter({ hasText: optionLabel }).first();
    await item.click();
    await panel.waitFor({ state: 'hidden' }).catch(() => {});
  }
}
