import { test, expect } from '@playwright/test';
import { LoginPage, MainLayoutPage, UsersManagementPage } from '../pom';
import {
  E2E_ADMIN_EMAIL,
  E2E_ADMIN_PASSWORD,
  E2E_API_URL,
} from '../utils/env';
import {
  getAdminToken,
  provisionDesignerUser,
  provisionClientUser,
  teardownUsers,
} from '../utils/api-helpers';
import { apiUrl, loginApi } from '../utils/api-client';
import { uniqueSuffix } from '../utils/test-data';

// ---------------------------------------------------------------------------
// 1. Admin login — valid and invalid credentials
// ---------------------------------------------------------------------------

test.describe('Admin login', () => {
  test('valid SuperAdmin credentials redirect to dashboard', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login(E2E_ADMIN_EMAIL, E2E_ADMIN_PASSWORD);
    await loginPage.expectRedirectToDashboard();
    await new MainLayoutPage(page).expectUserHeaderVisible();
  });

  test('wrong password shows an error and stays on login', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login(E2E_ADMIN_EMAIL, 'definitely-wrong-password');
    await loginPage.expectLoginError();
    await expect(page).toHaveURL(/\/auth\/login|\/login/);
  });

  test('unknown email shows an error and stays on login', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login('no-such-user@example.com', 'Test@123');
    await loginPage.expectLoginError();
    await expect(page).toHaveURL(/\/auth\/login|\/login/);
  });

  test('empty form submission shows validation errors', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await page.getByTestId('login-submit').click();
    // At least one validation message should appear without navigating away.
    await expect(page.locator('.p-error').first()).toBeVisible();
    await expect(page).toHaveURL(/\/auth\/login|\/login/);
  });
});

// ---------------------------------------------------------------------------
// 2. Create / edit / delete a staff (Designer) user
// ---------------------------------------------------------------------------

test.describe('Staff (Designer) user management', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('admin creates a Designer — row appears in grid and is confirmed via API', async ({
    page,
    request,
  }, testInfo) => {
    const suffix = uniqueSuffix(testInfo);
    const email = `e2e-staff-create-${suffix}@example.com`.toLowerCase();

    // Get admin token via API, then inject into localStorage BEFORE Angular bootstraps.
    // This avoids the flakiness of UI login under parallel workers (PrimeNG p-password
    // component is briefly non-editable during Angular hydration).
    const authRes = await request.post(`${E2E_API_URL}/api/auth/login`, {
      data: { email: E2E_ADMIN_EMAIL, password: E2E_ADMIN_PASSWORD },
      headers: { 'Content-Type': 'application/json' },
    });
    expect(authRes.ok()).toBeTruthy();
    const authBody = await authRes.json() as {
      token: string; refreshToken: string; expiresAt: string;
      user: { id: string; email: string; firstName: string; lastName: string; roleName: string };
    };

    // Inject session into sessionStorage before Angular bootstraps (addInitScript runs first).
    // auth_user must have a `role` key — matches setAuthData() in auth.service.ts.
    // auth_token_for_refresh holds the raw JWT so the token interceptor can call /refresh-token
    // on the first 401 without needing the in-memory accessToken (which is gone after a page load).
    const userForStorage = { ...authBody.user, role: authBody.user.roleName };
    await page.addInitScript((vals) => {
      sessionStorage.setItem('auth_token_for_refresh', vals.token);
      sessionStorage.setItem('auth_refresh_token', vals.refreshToken);
      sessionStorage.setItem('auth_user', vals.userJson);
    }, {
      token: authBody.token,
      refreshToken: authBody.refreshToken,
      userJson: JSON.stringify(userForStorage),
    });

    const users = new UsersManagementPage(page);
    await users.goto();
    await users.openCreateUserDialog();
    await users.createUserDialog.fillAndSubmitDesigner({
      email,
      firstName: 'Staff',
      lastName: 'Create',
      password: 'Test@123',
    });

    // Filter the table by email so the new row is visible regardless of pagination.
    // Target the users-table filter specifically ("Search users...") not the global search bar.
    await page.locator('input[placeholder*="Search users"]').fill(email);
    await users.expectUserInTable(email);

    // Confirm via API
    const token = await getAdminToken(request);
    const res = await request.get(apiUrl('/users?page=1&pageSize=100'), {
      headers: { Authorization: `Bearer ${token}` },
    });
    expect(res.ok()).toBeTruthy();
    const body = await res.json();
    const items: Array<{ id: string; email: string }> = body?.data?.items ?? body?.items ?? [];
    const created = items.find((u) => u.email?.toLowerCase() === email);
    expect(created).toBeDefined();
    if (created?.id) cleanup.push(created.id);
  });

  test('admin edits a Designer user name via API and change persists', async ({
    request,
  }, testInfo) => {
    const token = await getAdminToken(request);
    const designer = await provisionDesignerUser(request, token, testInfo);
    cleanup.push(designer.userId);

    // UpdateUserRequestDto requires Email, Role, and IsActive alongside name fields.
    const getRes = await request.get(apiUrl(`/users/${designer.userId}`), {
      headers: { Authorization: `Bearer ${token}` },
    });
    expect(getRes.ok()).toBeTruthy();
    const current = await getRes.json() as { email: string; roleName?: string; isActive?: boolean };

    const res = await request.put(apiUrl(`/users/${designer.userId}`), {
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      data: {
        email: current.email,
        firstName: 'Updated',
        lastName: 'Name',
        role: current.roleName ?? 'Designer',
        isActive: current.isActive ?? true,
      },
    });
    expect(res.ok()).toBeTruthy();

    const verify = await request.get(apiUrl(`/users/${designer.userId}`), {
      headers: { Authorization: `Bearer ${token}` },
    });
    expect(verify.ok()).toBeTruthy();
    const user = await verify.json() as { firstName?: string; lastName?: string };
    expect(user.firstName).toBe('Updated');
    expect(user.lastName).toBe('Name');
  });

  test('admin soft-deletes a Designer — user can no longer log in', async ({
    request,
  }, testInfo) => {
    const token = await getAdminToken(request);
    const designer = await provisionDesignerUser(request, token, testInfo);
    // Hard-delete on teardown handles both soft and hard states.
    cleanup.push(designer.userId);

    const del = await request.delete(apiUrl(`/users/${designer.userId}`), {
      headers: { Authorization: `Bearer ${token}` },
    });
    expect(del.ok()).toBeTruthy();

    // Deleted user should not be able to authenticate.
    const loginRes = await request.post(apiUrl('/auth/login'), {
      headers: { 'Content-Type': 'application/json' },
      data: { email: designer.email, password: designer.password },
    });
    expect(loginRes.status()).toBe(401);
  });
});

// ---------------------------------------------------------------------------
// 3. Role-based access control — Designer and Client cannot reach admin areas
// ---------------------------------------------------------------------------

test.describe('RBAC — role cannot access restricted sections', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('Designer cannot call admin analytics overview (403)', async ({ request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const designer = await provisionDesignerUser(request, adminToken, testInfo);
    cleanup.push(designer.userId);

    const auth = await loginApi(request, designer.email, designer.password);
    const res = await request.get(apiUrl('/admin/analytics/overview'), {
      headers: { Authorization: `Bearer ${auth.token}` },
    });
    expect(res.status()).toBe(403);
  });

  test('Designer cannot list all users (403)', async ({ request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const designer = await provisionDesignerUser(request, adminToken, testInfo);
    cleanup.push(designer.userId);

    const auth = await loginApi(request, designer.email, designer.password);
    const res = await request.get(apiUrl('/users?page=1&pageSize=10'), {
      headers: { Authorization: `Bearer ${auth.token}` },
    });
    expect(res.status()).toBe(403);
  });

  test('Client cannot call admin analytics overview (403)', async ({ request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const client = await provisionClientUser(request, adminToken, testInfo);
    cleanup.push(client.userId);

    const auth = await loginApi(request, client.email, client.password);
    const res = await request.get(apiUrl('/admin/analytics/overview'), {
      headers: { Authorization: `Bearer ${auth.token}` },
    });
    expect(res.status()).toBe(403);
  });

  test('Client cannot delete another user (403)', async ({ request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const clientA = await provisionClientUser(request, adminToken, testInfo);
    const clientB = await provisionClientUser(request, adminToken, testInfo);
    cleanup.push(clientA.userId, clientB.userId);

    const auth = await loginApi(request, clientA.email, clientA.password);
    const res = await request.delete(apiUrl(`/users/${clientB.userId}`), {
      headers: { Authorization: `Bearer ${auth.token}` },
    });
    expect(res.status()).toBe(403);
  });

  test('unauthenticated request to protected route returns 401', async ({ request }) => {
    const res = await request.get(apiUrl('/users?page=1&pageSize=10'));
    expect(res.status()).toBe(401);
  });
});

// ---------------------------------------------------------------------------
// 4. Password reset (forgot-password flow)
// ---------------------------------------------------------------------------

test.describe('Password reset', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('forgot-password for unknown email returns generic message (no enumeration)', async ({
    request,
  }) => {
    const res = await request.post(apiUrl('/auth/forgot-password'), {
      headers: { 'Content-Type': 'application/json' },
      data: { email: 'no-such-user-e2e@example.com' },
    });
    expect(res.ok()).toBeTruthy();
    const body = await res.json() as { message?: string; resetToken?: string; email?: string };
    expect(body.message).toMatch(/account exists/i);
    // Must not reveal whether the account exists (null or absent — both acceptable).
    expect(body.resetToken).toBeFalsy();
    expect(body.email).toBeFalsy();
  });

  test('reset-password-with-token with expired/invalid token is rejected (400)', async ({
    request,
  }) => {
    const res = await request.post(apiUrl('/auth/reset-password-with-token'), {
      headers: { 'Content-Type': 'application/json' },
      data: {
        email: 'any@example.com',
        token: 'invalid-token-that-does-not-exist',
        newPassword: 'NewPassword@123',
        confirmPassword: 'NewPassword@123',
      },
    });
    expect(res.status()).toBe(400);
  });

  test('full reset-password-with-token cycle works end-to-end', async ({ request }, testInfo) => {
    // Provision a real user so we can exercise the full token round-trip.
    const adminToken = await getAdminToken(request);
    const designer = await provisionDesignerUser(request, adminToken, testInfo);
    cleanup.push(designer.userId);

    // Trigger forgot-password (Development env returns token in response body).
    const forgotRes = await request.post(apiUrl('/auth/forgot-password'), {
      headers: { 'Content-Type': 'application/json' },
      data: { email: designer.email },
    });
    expect(forgotRes.ok()).toBeTruthy();
    const forgotBody = await forgotRes.json() as {
      message?: string;
      resetToken?: string;
      email?: string;
    };

    // In non-dev CI the token is not returned — skip the round-trip but confirm generic message.
    if (!forgotBody.resetToken) {
      expect(forgotBody.message).toMatch(/account exists/i);
      return;
    }

    // Dev path: reset the password with the returned token.
    const newPassword = 'NewSecure@2026';
    const resetRes = await request.post(apiUrl('/auth/reset-password-with-token'), {
      headers: { 'Content-Type': 'application/json' },
      data: {
        email: designer.email,
        token: forgotBody.resetToken,
        newPassword,
        confirmPassword: newPassword,
      },
    });
    expect(resetRes.ok()).toBeTruthy();

    // Old password must no longer work.
    const oldLoginRes = await request.post(apiUrl('/auth/login'), {
      headers: { 'Content-Type': 'application/json' },
      data: { email: designer.email, password: designer.password },
    });
    expect(oldLoginRes.status()).toBe(401);

    // New password must work.
    const newLoginRes = await request.post(apiUrl('/auth/login'), {
      headers: { 'Content-Type': 'application/json' },
      data: { email: designer.email, password: newPassword },
    });
    expect(newLoginRes.ok()).toBeTruthy();
  });

  test('password change invalidates the previous refresh token', async ({ request }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const designer = await provisionDesignerUser(request, adminToken, testInfo);
    cleanup.push(designer.userId);

    const auth = await loginApi(request, designer.email, designer.password);
    const oldRefreshToken = auth.refreshToken;
    expect(oldRefreshToken).toBeTruthy();

    // Change password.
    const changeRes = await request.post(apiUrl('/auth/change-password'), {
      headers: {
        Authorization: `Bearer ${auth.token}`,
        'Content-Type': 'application/json',
      },
      data: {
        currentPassword: designer.password,
        newPassword: 'Changed@2026',
        confirmPassword: 'Changed@2026',
      },
    });
    expect(changeRes.ok()).toBeTruthy();

    // Old refresh token must be invalidated.
    const refreshRes = await request.post(apiUrl('/auth/refresh-token'), {
      headers: { 'Content-Type': 'application/json' },
      data: { token: auth.token, refreshToken: oldRefreshToken },
    });
    expect(refreshRes.status()).toBe(401);
  });
});

// ---------------------------------------------------------------------------
// 5. Core portal flows — order creation and Designer assignment
// ---------------------------------------------------------------------------

test.describe('Core portal — order and assignment flow', () => {
  const cleanup: string[] = [];

  test.afterAll(async ({ request }) => {
    await teardownUsers(request, cleanup);
  });

  test('Client can create an order and it appears in the order list', async ({
    request,
  }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const client = await provisionClientUser(request, adminToken, testInfo);
    cleanup.push(client.userId);

    const clientAuth = await loginApi(request, client.email, client.password);
    const createRes = await request.post(apiUrl('/orders'), {
      headers: {
        Authorization: `Bearer ${clientAuth.token}`,
        'Content-Type': 'application/json',
      },
      data: {
        title: `E2E Order ${uniqueSuffix(testInfo)}`,
        description: 'Auto-generated by critical-flows E2E',
        price: 0,
      },
    });
    expect(createRes.ok()).toBeTruthy();
    const order = await createRes.json() as { id: string; status: string };
    expect(order.id).toBeTruthy();

    const listRes = await request.get(apiUrl('/orders/my-orders'), {
      headers: { Authorization: `Bearer ${clientAuth.token}` },
    });
    expect(listRes.ok()).toBeTruthy();
    const body = await listRes.json();
    const items: Array<{ id: string }> = body?.data?.items ?? body?.items ?? body ?? [];
    expect(items.some((o) => o.id === order.id)).toBeTruthy();
  });

  test('Admin can approve an order and assign a Designer', async ({
    request,
  }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const client = await provisionClientUser(request, adminToken, testInfo);
    const designer = await provisionDesignerUser(request, adminToken, testInfo);
    cleanup.push(client.userId, designer.userId);

    const clientAuth = await loginApi(request, client.email, client.password);

    // Client creates order.
    const createRes = await request.post(apiUrl('/orders'), {
      headers: {
        Authorization: `Bearer ${clientAuth.token}`,
        'Content-Type': 'application/json',
      },
      data: {
        title: `E2E Assign Order ${uniqueSuffix(testInfo)}`,
        description: 'Auto-generated by critical-flows E2E',
        price: 0,
      },
    });
    expect(createRes.ok()).toBeTruthy();
    const order = await createRes.json() as { id: string };

    // Admin approves.
    const approveRes = await request.post(apiUrl(`/orders/${order.id}/approve`), {
      headers: {
        Authorization: `Bearer ${adminToken}`,
        'Content-Type': 'application/json',
      },
      data: {},
    });
    expect(approveRes.ok()).toBeTruthy();

    // Admin assigns Designer.
    const assignRes = await request.post(apiUrl(`/orders/${order.id}/assign`), {
      headers: {
        Authorization: `Bearer ${adminToken}`,
        'Content-Type': 'application/json',
      },
      data: { designerId: designer.userId },
    });
    expect(assignRes.ok()).toBeTruthy();
    const assigned = await assignRes.json() as { status: string };
    expect(assigned.status).toMatch(/InProgress|Assigned/i);
  });

  test('Designer cannot see orders from other clients (IDOR check)', async ({
    request,
  }, testInfo) => {
    const adminToken = await getAdminToken(request);
    const clientA = await provisionClientUser(request, adminToken, testInfo);
    const designer = await provisionDesignerUser(request, adminToken, testInfo);
    cleanup.push(clientA.userId, designer.userId);

    // clientA creates an order.
    const clientAuth = await loginApi(request, clientA.email, clientA.password);
    const createRes = await request.post(apiUrl('/orders'), {
      headers: {
        Authorization: `Bearer ${clientAuth.token}`,
        'Content-Type': 'application/json',
      },
      data: {
        title: `E2E IDOR Check ${uniqueSuffix(testInfo)}`,
        description: 'IDOR test',
        price: 0,
      },
    });
    expect(createRes.ok()).toBeTruthy();
    const order = await createRes.json() as { id: string };

    // Designer should not be able to access clientA's order details
    // (before being assigned). Backend returns 403 or 404 for unassigned orders.
    const designerAuth = await loginApi(request, designer.email, designer.password);
    const fetchRes = await request.get(apiUrl(`/orders/${order.id}`), {
      headers: { Authorization: `Bearer ${designerAuth.token}` },
    });
    expect([403, 404]).toContain(fetchRes.status());
  });
});
