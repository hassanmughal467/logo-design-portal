import type { APIRequestContext } from '@playwright/test';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from './env';
import { buildClientUserPayload, buildDesignerUserPayload, uniqueSuffix } from './test-data';
import type { TestInfo } from '@playwright/test';
import { createUserApi, deleteUserApi, loginApi } from './api-client';

/**
 * High-level test data orchestration: create entities through the API (fast, reliable),
 * keep handles for teardown, and combine with UI flows for “real user” coverage.
 */

export type ProvisionedClient = { email: string; password: string; userId: string };
export type ProvisionedDesigner = { email: string; password: string; userId: string };

/** Creates a disposable Client user via API using the SuperAdmin / admin token. */
export async function provisionClientUser(
  request: APIRequestContext,
  adminToken: string,
  testInfo: TestInfo,
  password = 'Test@123'
): Promise<ProvisionedClient> {
  const suffix = uniqueSuffix(testInfo);
  const email = `e2e-client-${suffix}@example.com`.toLowerCase();
  const payload = buildClientUserPayload(email, password);
  const created = await createUserApi(request, adminToken, payload);
  return { email, password, userId: created.id };
}

export async function provisionDesignerUser(
  request: APIRequestContext,
  adminToken: string,
  testInfo: TestInfo,
  password = 'Test@123'
): Promise<ProvisionedDesigner> {
  const suffix = uniqueSuffix(testInfo);
  const email = `e2e-designer-${suffix}@example.com`.toLowerCase();
  const payload = buildDesignerUserPayload(email, password);
  const created = await createUserApi(request, adminToken, payload);
  return { email, password, userId: created.id };
}

/** Obtain admin API token using configured operator credentials (SuperAdmin in dev). */
export async function getAdminToken(request: APIRequestContext): Promise<string> {
  const auth = await loginApi(request, E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
  return auth.token;
}

/**
 * Provision a Client and return credentials for UI login in create-order scenarios.
 * Callers should push `userId` into their cleanup list.
 */
export async function provisionClientForUi(
  request: APIRequestContext,
  testInfo: TestInfo,
  password = 'Test@123'
): Promise<ProvisionedClient> {
  const adminToken = await getAdminToken(request);
  return provisionClientUser(request, adminToken, testInfo, password);
}

/**
 * Deletes users created during a test when SuperAdmin is available.
 * Hard delete requires SuperAdmin (`UsersController.DeleteUser`).
 */
export async function teardownUsers(request: APIRequestContext, userIds: string[]): Promise<void> {
  if (userIds.length === 0) {
    return;
  }
  try {
    const token = await getAdminToken(request);
    for (const id of userIds) {
      try {
        await deleteUserApi(request, token, id, true);
      } catch {
        // Best-effort cleanup; do not fail the build if data was already removed.
      }
    }
  } catch {
    /* login/delete may fail if DB was reset — ignore */
  }
}
