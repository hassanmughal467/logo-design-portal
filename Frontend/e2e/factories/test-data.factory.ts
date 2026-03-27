import type { APIRequestContext } from '@playwright/test';
import type { TestInfo } from '@playwright/test';
import { createOrderApi, createUserApi, loginApi, createInvoiceApi } from '../utils/api-client';
import { buildClientUserPayload, buildDesignerUserPayload, uniqueSuffix } from '../utils/test-data';
import { getAdminToken } from '../utils/api-helpers';

export type FactoryOverrides<T> = Partial<T>;

/**
 * Composable E2E factories — keep payloads in one place; supports per-call overrides.
 */

export async function createTestUser(
  request: APIRequestContext,
  testInfo: TestInfo,
  role: 'Client' | 'Designer',
  overrides: FactoryOverrides<Record<string, unknown>> = {}
): Promise<{ email: string; password: string; userId: string; token: string }> {
  const admin = await getAdminToken(request);
  const suffix = uniqueSuffix(testInfo);
  const password = (overrides.password as string | undefined) ?? 'Test@123';
  const email =
    (overrides.email as string | undefined) ??
    (role === 'Client'
      ? `factory-client-${suffix}@example.com`.toLowerCase()
      : `factory-designer-${suffix}@example.com`.toLowerCase());

  const base =
    role === 'Client'
      ? buildClientUserPayload(email, password)
      : buildDesignerUserPayload(email, password);

  const created = await createUserApi(request, admin, { ...base, ...overrides, email, password });
  const auth = await loginApi(request, email, password);
  return { email, password, userId: created.id, token: auth.token };
}

export async function createTestOrder(
  request: APIRequestContext,
  clientToken: string,
  testInfo: TestInfo,
  overrides: FactoryOverrides<{ title?: string; description?: string; price?: number }> = {}
) {
  const suffix = uniqueSuffix(testInfo);
  return createOrderApi(request, clientToken, {
    title: overrides.title ?? `Factory order ${suffix}`,
    description:
      overrides.description ??
      'Factory-generated order description with enough characters for validation.',
    price: overrides.price ?? 99,
  });
}

export async function createTestInvoice(
  request: APIRequestContext,
  adminToken: string,
  orderId: string,
  price: number,
  overrides: FactoryOverrides<{ billingType?: number; taxAmount?: number }> = {}
) {
  return createInvoiceApi(request, adminToken, {
    orders: [{ orderId, price }],
    billingType: overrides.billingType ?? 1,
    taxAmount: overrides.taxAmount ?? 0,
  });
}
