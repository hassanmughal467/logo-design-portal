import type { TestInfo } from '@playwright/test';

/**
 * Build a unique suffix for emails and titles so parallel workers do not collide in the database.
 */
export function uniqueSuffix(testInfo: TestInfo): string {
  const w = testInfo.workerIndex ?? 0;
  return `${Date.now()}-${w}-${Math.floor(Math.random() * 1e6)}`;
}

/**
 * Example payloads aligned with `CreateUserRequestDto` and domain rules (Client needs company, etc.).
 * Role IDs are the fixed seeded GUIDs from `LogoDesignPortal.Domain.Constants.SeededRoleIds`.
 */
export const SeededRoleIds = {
  SuperAdmin: '11111111-1111-1111-1111-111111111111',
  Admin: '22222222-2222-2222-2222-222222222222',
  Designer: '33333333-3333-3333-3333-333333333333',
  Client: '44444444-4444-4444-4444-444444444444',
} as const;

/** Portal role names (API + JWT role claims). */
export const PortalRole = {
  /** Full access — seeded as `superadmin@logodesign.com` in API startup. */
  SuperAdmin: 'SuperAdmin',
  /** Manager-style operational admin (your “Manager” persona maps here). */
  Admin: 'Admin',
  /** End-user / customer (your “User” persona maps here). */
  Client: 'Client',
  /** Staff designer (your “Staff” persona maps here). */
  Designer: 'Designer',
} as const;

export function buildClientUserPayload(email: string, password: string) {
  return {
    email,
    firstName: 'E2E',
    lastName: 'Client',
    password,
    roleId: SeededRoleIds.Client,
    companyName: 'E2E Test Co',
    contactName: 'E2E Contact',
    phoneNumber: '+10000000000',
    invoiceEmail: email,
    billingType: 1,
  };
}

export function buildDesignerUserPayload(email: string, password: string) {
  return {
    email,
    firstName: 'E2E',
    lastName: 'Designer',
    password,
    roleId: SeededRoleIds.Designer,
    specialization: 'Embroidery',
    isAvailable: true,
  };
}

/** Dedicated SuperAdmin-role account for tests that hold a live UI session as an admin persona.
 *  Each caller gets its own account (see provisionAdminForUi) so concurrent sessions never
 *  share one refresh-token row and rotate each other's refresh token out from under them.
 *  Must be SuperAdmin, not Admin — the account it replaces (superadmin@logodesign.com) is
 *  SuperAdmin, and some endpoints (e.g. UsersController PUT /{id}) are SuperAdmin-only; only an
 *  existing SuperAdmin token can assign the SuperAdmin role (see UsersController.CreateUser). */
export function buildAdminUserPayload(email: string, password: string) {
  return {
    email,
    firstName: 'E2E',
    lastName: 'Admin',
    password,
    roleId: SeededRoleIds.SuperAdmin,
  };
}
