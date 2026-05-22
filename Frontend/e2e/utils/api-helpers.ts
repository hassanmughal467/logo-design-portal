import type { APIRequestContext } from '@playwright/test';
import { E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD } from './env';
import { buildClientUserPayload, buildDesignerUserPayload, uniqueSuffix } from './test-data';
import type { TestInfo } from '@playwright/test';
import {
  assignPermissionToRoleApi,
  createUserApi,
  deleteUserApi,
  getAllPermissionsApi,
  getRolePermissionsApi,
  loginApi,
  loginApiBearerOnly,
} from './api-client';
import { PortalRole, SeededRoleIds } from './test-data';

/**
 * High-level test data orchestration: create entities through the API (fast, reliable),
 * keep handles for teardown, and combine with UI flows for “real user” coverage.
 */

export type ProvisionedClient = { email: string; password: string; userId: string };
export type ProvisionedDesigner = { email: string; password: string; userId: string };

/** Client account + Bearer token — required for `POST /api/orders` and client-scoped uploads. */
export type LoggedInClient = ProvisionedClient & { token: string };

/** Permissions the Client role needs for order + upload API E2E (matches API startup seed). */
const CLIENT_ROLE_PERMISSIONS = ['CreateOrder', 'UploadFile', 'DownloadFile', 'UpdateOrderStatus'] as const;
const DESIGNER_ROLE_PERMISSIONS = ['UploadFile', 'DownloadFile', 'UpdateOrderStatus'] as const;

const ROLE_PERMISSION_SEED: Record<string, readonly string[]> = {
  [SeededRoleIds.Client]: CLIENT_ROLE_PERMISSIONS,
  [SeededRoleIds.Designer]: DESIGNER_ROLE_PERMISSIONS,
};

let clientRolePermissionsReady: Promise<void> | null = null;

/**
 * Ensures seeded Client role has CreateOrder/UploadFile/etc. when dev DB init skipped role-permission links.
 * Idempotent — safe to call from every `provisionLoggedInClient`.
 */
export async function ensureClientRolePermissions(request: APIRequestContext): Promise<void> {
  if (!clientRolePermissionsReady) {
    clientRolePermissionsReady = (async () => {
      const adminAuth = await loginApi(request, E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
      const adminToken = adminAuth.token;
      const allPerms = await getAllPermissionsApi(request, adminToken);
      if (allPerms.length === 0) {
        throw new Error(
          'ensureClientRolePermissions: Permissions table is empty. From Backend/src/LogoDesignPortal.API run: dotnet run -- --repair-database (then restart IIS Express / API).'
        );
      }
      const byName = new Map(allPerms.map((p) => [p.name, p.id]));
      for (const [roleId, names] of Object.entries(ROLE_PERMISSION_SEED)) {
        const rolePerms = await getRolePermissionsApi(request, adminToken, roleId);
        const existing = new Set(rolePerms.permissions.map((p) => p.name));
        for (const name of names) {
          if (existing.has(name)) continue;
          const permissionId = byName.get(name);
          if (!permissionId) {
            throw new Error(
              `ensureClientRolePermissions: permission "${name}" not found in DB — run: dotnet run --project Backend/src/LogoDesignPortal.API -- --repair-database`
            );
          }
          await assignPermissionToRoleApi(request, adminToken, roleId, permissionId);
        }
      }
    })();
  }
  await clientRolePermissionsReady;
}

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

/**
 * Provisions a disposable Client (via admin) and logs in — use this before `createOrderApi` / uploads.
 * Do not use `getAdminToken()` or `admin.json` storageState for order creation.
 */
export async function provisionLoggedInClient(
  request: APIRequestContext,
  testInfo: TestInfo,
  password = 'Test@123'
): Promise<LoggedInClient> {
  await ensureClientRolePermissions(request);
  const adminToken = await getAdminToken(request);
  const client = await provisionClientUser(request, adminToken, testInfo, password);
  const auth = await loginApiBearerOnly(request, client.email, client.password);
  if (auth.user.roleName !== PortalRole.Client) {
    throw new Error(
      `provisionLoggedInClient: expected role ${PortalRole.Client}, got "${auth.user.roleName}" for ${client.email}`
    );
  }
  return { ...client, token: auth.token };
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
/** SuperAdmin JWT for admin API calls. Ensures Client role permissions are seeded first. */
export async function getAdminToken(request: APIRequestContext): Promise<string> {
  await ensureClientRolePermissions(request);
  const auth = await loginApi(request, E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
  return auth.token;
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
