import { test, expect } from '@playwright/test';
import { MainLayoutPage, UsersManagementPage } from '../../../pom';
import { getAdminToken, teardownUsers } from '../../../utils/api-helpers';
import { apiUrl } from '../../../utils/api-client';
import { uniqueSuffix } from '../../../utils/test-data';

/**
 * Admin / SuperAdmin UI coverage using a pre-authenticated session (`auth.setup.ts`).
 *
 * Role mapping in this product:
 * - SuperAdmin / Admin → “Admin” / “Manager” personas for privileged UI.
 * - Client → “User” persona (customer).
 * - Designer → “Staff” persona.
 */

test.describe('Admin — user management', () => {
  test('creates a Designer and row appears in the grid', async ({ page, request }) => {
    const suffix = uniqueSuffix(test.info());
    const email = `e2e-designer-ui-${suffix}@example.com`.toLowerCase();

    const users = new UsersManagementPage(page);
    await users.goto();
    await users.openCreateUserDialog();
    await users.createUserDialog.fillAndSubmitDesigner({
      email,
      firstName: 'Playwright',
      lastName: 'Designer',
      password: 'Test@123',
    });

    await users.expectUserInTable(email);

    const token = await getAdminToken(request);
    const me = await request.get(apiUrl('/users?page=1&pageSize=100'), {
      headers: { Authorization: `Bearer ${token}` },
    });
    expect(me.ok()).toBeTruthy();
    const body = await me.json();
    const items = body?.data?.items ?? body?.items ?? [];
    expect(items.some((u: { email?: string }) => u.email?.toLowerCase() === email)).toBeTruthy();

    const created = items.find((u: { email?: string }) => u.email?.toLowerCase() === email);
    if (created?.id) {
      await teardownUsers(request, [created.id]);
    }
  });

  test('session reflects privileged operator', async ({ page }) => {
    await page.goto('/dashboard');
    await new MainLayoutPage(page).expectUserHeaderVisible();
  });
});
