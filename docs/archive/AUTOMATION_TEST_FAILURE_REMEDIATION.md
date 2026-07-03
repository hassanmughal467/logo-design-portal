# Automation Test Failure Remediation Guide

**Generated:** March 7, 2026  
**Updated:** March 7, 2026 (remediation applied)  
**Purpose:** Root cause analysis and fix recommendations for Playwright automation tests

---

## Executive Summary

The failures cluster into **5 root causes**:

| Category | Count | Root Cause |
|----------|-------|------------|
| **1. Authentication / Session** | ~45 | E2E fixtures not persisting auth; users redirected to `/auth/login` |
| **2. API 403 Forbidden** | ~25 | Admin/Designer credentials or permissions mismatch |
| **3. Auth API (Refresh Token)** | 1 | Invalid/expired refresh token in auth test |
| **4. Permissions API (Bad Request)** | 3 | Wrong request format for `GET /api/permissions/role/{roleId}` |
| **5. UI Selectors / Timing** | ~8 | Locators outdated or page not loaded |

### Applied Fixes (March 7, 2026)

| Fix | File(s) | Description |
|-----|---------|-------------|
| Order status string | `orders.api.spec.ts`, `revisions.spec.ts` | API returns `status` as string (e.g. `"InProgress"`, `"Completed"`); tests updated to expect strings |
| Permissions role GUID | `permissions.api.spec.ts` | Use Admin role GUID `22222222-2222-2222-2222-222222222222` instead of numeric ID |
| Request revision form | `api-client.ts` | Send `Instructions` as form data instead of JSON `feedback` |
| SendFilesToClient body | `api-client.ts`, `order-workflow.ts` | Send `fileIds` array in request body; removed call after client approval (order locked) |
| Auth storage role | `auth.setup.ts` | Use `roleName` from API response (UserDto.RoleName) for Angular RoleGuard |
| Orders Create button | `orders.spec.ts` | Use `p-dialog` scoped selector and wait for button enabled |
| Orders page visibility | `orders.spec.ts` | Use `.orders-page` only to avoid strict mode |
| Notifications bell | `notifications.spec.ts` | Use `.notification-btn` for unique header bell |

---

## 1. Authentication & Session Failures

### Symptoms
- `expect(page).toHaveURL(/\/invoices/)` fails – received `http://localhost:4200/auth/login`
- Affects: invoices, messages, notifications, role-permissions, orders
- All `authenticatedAdminPage`, `authenticatedClientPage`, `authenticatedDesignerPage`, `authenticatedSuperAdminPage` fixtures

### Root Cause
The authenticated page fixtures are not correctly establishing or persisting auth state. When tests navigate to protected routes, the app redirects to login.

### Fixes

**A. Verify fixture implementation (in `C:\Users\MuhammadHassan\Desktop\Automation\tests\`)**

1. **Storage state:** Ensure `storageState` is set after login and reused:
   ```ts
   // playwright.config.ts or fixtures
   test.extend({
     authenticatedAdminPage: async ({ page }, use) => {
       await page.goto('/auth/login');
       await page.fill('[name="email"]', process.env.ADMIN_EMAIL || 'admin@logodesign.com');
       await page.fill('[name="password"]', process.env.ADMIN_PASSWORD || 'Admin@123');
       await page.click('button[type="submit"]');
       await expect(page).toHaveURL(/\/(dashboard|orders)/);  // Wait for redirect
       await page.context().storageState({ path: 'auth-admin.json' });
       await use(page);
     },
   });
   ```

2. **Reuse storage state:** For subsequent tests, use `storageState: 'auth-admin.json'` in project config so new contexts start already logged in.

3. **Base URL:** Ensure `baseURL: 'http://localhost:4200'` and that the frontend is running before tests.

**B. Verify test user credentials exist in the database**

From `QUICK_START_TESTING.md`, the seeded SuperAdmin is:
- Email: `superadmin@logodesign.com`
- Password: `SuperAdmin@123`

Ensure your Automation project has **matching test users** for Admin, Designer, and Client. If they don't exist, seed them or create via API before tests.

---

## 2. API 403 Forbidden Failures

### Symptoms
- `ApiError: Forbidden` on:
  - `ApiClient.assignDesigner` (POST orders/{id}/assign)
  - `ApiClient.get` (GET orders, GET designer-profiles)
  - Orders API, Files API, Users API, Revisions E2E, Order Workflow E2E

### Root Cause
The API client is using tokens for users that either:
1. Don't have the required permissions (Admin needs `AssignOrder`, `ViewAllOrders`, `ViewDesignerProfiles` granted by SuperAdmin)
2. Use wrong credentials (email/password)
3. Use expired or invalid tokens

### Fixes

**A. Grant Admin permissions**

Per `AUTOMATION_TESTING_SYSTEM_ANALYSIS.md` §2.2:
> *Admin requires permission granted by SuperAdmin via RolePermission.*

Before running tests:
1. Log in as SuperAdmin (`superadmin@logodesign.com` / `SuperAdmin@123`)
2. Go to **Permissions** (`/permissions`)
3. Assign to **Admin** role:
   - `ViewAllOrders`
   - `AssignOrder`
   - `ViewDesignerProfiles`
   - `UpdateDesignerProfile`
   - `CreateDesignerProfile`
   - `UpdateOrderStatus`

**B. Verify API auth in `utils/auth.ts` and `utils/api-client.ts`**

1. **Login before each API test** (or use a valid token fixture):
   ```ts
   const { accessToken } = await loginViaApi(adminEmail, adminPassword);
   apiClient.setToken(accessToken);
   ```

2. **Use correct credentials** – ensure env vars or defaults match seeded users:
   - `ADMIN_EMAIL`, `ADMIN_PASSWORD`
   - `DESIGNER_EMAIL`, `DESIGNER_PASSWORD`
   - `CLIENT_EMAIL`, `CLIENT_PASSWORD`

3. **Token refresh:** If tests run longer than 1 hour (per `appsettings.json` AccessTokenExpiryHours), refresh the token or re-login.

---

## 3. Auth API – Refresh Token Failure

### Symptom
- `Refresh token returns new access token` – `401 - {"error":"Invalid refresh token."}`

### Root Cause
The test uses a refresh token that is expired, already consumed (single-use), or malformed.

### Fix
1. **Fresh login before refresh test:** Call `POST /api/auth/login` to get a new `refreshToken`, then immediately call `POST /api/auth/refresh-token` with that token.
2. **Don't reuse refresh tokens:** Each refresh consumes the token; use a new login for each refresh test run.

---

## 4. Permissions API – Bad Request

### Symptom
- `SuperAdmin can get role permissions` – `ApiError: Bad Request`

### Root Cause
The Permissions API expects `GET /api/permissions/role/{roleId}`. The test may be:
- Using a role name instead of role ID (GUID)
- Using wrong query/body format

### Fix
1. Resolve role ID first: `GET /api/permissions` or `GET /api/users` to get role IDs.
2. Use GUID: `GET /api/permissions/role/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
3. Check backend `PermissionsController` for exact route and parameter type.

---

## 5. UI Selector & Timing Failures

### Symptoms
- `TimeoutError: locator.click: Timeout 15000ms exceeded` – e.g. `getByRole('button', { name: 'Create Order' })`
- `tr:has-text("E2E File Upload ...")` not found
- `.orders-page`, `.page-title`, `p-table` not found

### Root Cause
1. **Not authenticated:** Page shows login instead of orders (see §1).
2. **Selectors changed:** Button text or structure may have changed (e.g. "Create Order" vs "New Order").
3. **Data not present:** Order not created or not visible (filtering, pagination).

### Fixes

**A. Fix auth first** – Most of these occur because the user is on the login page.

**B. Update selectors** – Inspect the live app:
- `order-list.component.html` – actual button text and structure
- `order-list` – table classes (e.g. `p-datatable` vs `p-table`)

**C. Wait for data:**
```ts
await page.goto('/orders');
await page.waitForSelector('p-table, .p-datatable, .orders-page', { timeout: 15000 });
```

---

## Checklist: Pre-Run Verification

Before running automation tests:

| Step | Action |
|------|--------|
| 1 | Backend running on `http://localhost:5000` (or configured API URL) |
| 2 | Frontend running on `http://localhost:4200` |
| 3 | Database migrated and seeded (SuperAdmin exists) |
| 4 | Admin role has AssignOrder, ViewAllOrders, ViewDesignerProfiles (via SuperAdmin in Permissions) |
| 5 | Test users exist: Admin, Designer, Client (or create via API/seed) |
| 6 | `playwright.config.ts` baseURL = `http://localhost:4200` |
| 7 | API base URL in Automation project matches backend (e.g. `http://localhost:5000`) |
| 8 | Env vars for credentials set (or defaults match seeded users) |

---

## Recommended Fix Order

1. **Fix auth fixtures** – Ensure `authenticated*Page` fixtures correctly log in and persist `storageState`.
2. **Fix API credentials** – Use valid Admin/Designer/Client credentials; grant Admin permissions.
3. **Fix refresh token test** – Use fresh login → immediate refresh.
4. **Fix Permissions API** – Use role GUID, correct route.
5. **Re-run** – Most E2E failures should resolve once auth works.
6. **Update selectors** – For any remaining UI failures, align with current frontend markup.

---

## Environment Variables (Suggested)

Add to `.env` or `playwright.config.ts`:

```env
BASE_URL=http://localhost:4200
API_URL=http://localhost:5000
SUPERADMIN_EMAIL=superadmin@logodesign.com
SUPERADMIN_PASSWORD=SuperAdmin@123
ADMIN_EMAIL=admin@logodesign.com
ADMIN_PASSWORD=Admin@123
DESIGNER_EMAIL=designer@logodesign.com
DESIGNER_PASSWORD=Designer@123
CLIENT_EMAIL=client@logodesign.com
CLIENT_PASSWORD=Client@123
```

Ensure these users exist in the database and Admin has the required permissions.
