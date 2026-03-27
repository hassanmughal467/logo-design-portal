import { expect, type Page } from '@playwright/test';

export async function randomHumanDelay(minMs = 120, maxMs = 700): Promise<void> {
  const ms = Math.floor(Math.random() * (maxMs - minMs + 1)) + minMs;
  await new Promise((resolve) => setTimeout(resolve, ms));
}

export function randomText(length: number): string {
  const chars =
    'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 !@#$%^&*()_+-={}[]|:;"<>,.?/~`';
  let out = '';
  for (let i = 0; i < length; i += 1) {
    out += chars[Math.floor(Math.random() * chars.length)];
  }
  return out;
}

export function invalidEmailSamples(): string[] {
  return ['plainaddress', 'missing-at.example.com', 'bad@@example.com', 'a@b', 'name@domain'];
}

export async function clickRapidly(page: Page, selector: string, count = 4): Promise<void> {
  for (let i = 0; i < count; i += 1) {
    await page.locator(selector).click({ force: true });
  }
}

export function trackConsoleErrors(page: Page): string[] {
  const errors: string[] = [];
  page.on('console', (msg) => {
    if (msg.type() === 'error') {
      errors.push(msg.text());
    }
  });
  page.on('pageerror', (err) => {
    errors.push(err.message);
  });
  return errors;
}

export async function expectNoConsoleErrors(errors: string[]): Promise<void> {
  expect(errors, `Console/page errors detected:\n${errors.join('\n')}`).toEqual([]);
}
