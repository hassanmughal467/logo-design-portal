import type { Page } from '@playwright/test';

/**
 * Base for all page objects. Keeps a single `Page` reference and documents the POM convention:
 * pages expose user-facing actions and assertions; tests should not reach for raw selectors.
 */
export abstract class BasePage {
  constructor(protected readonly page: Page) {}

  /** Expose Playwright page only when a test helper (e.g. API setup) legitimately needs it. */
  get rawPage(): Page {
    return this.page;
  }
}
