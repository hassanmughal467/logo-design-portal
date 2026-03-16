# End-to-End Integration Test Plan — Logo Design Portal

**Version:** 1.0  
**Last Updated:** March 11, 2025  
**Purpose:** Comprehensive plan for automated and manual end-to-end integration testing across Frontend (Angular), Backend (ASP.NET Core API), and Database (MySQL).

---

## Table of Contents

1. [Overview](#1-overview)
2. [Test Environment](#2-test-environment)
3. [Test Scope & Boundaries](#3-test-scope--boundaries)
4. [Critical User Journeys](#4-critical-user-journeys)
5. [API Integration Tests](#5-api-integration-tests)
6. [UI E2E Tests](#6-ui-e2e-tests)
7. [Test Data Strategy](#7-test-data-strategy)
8. [Implementation Roadmap](#8-implementation-roadmap)
9. [Appendix](#9-appendix)

---

## 1. Overview

### 1.1 Architecture Under Test

```
┌─────────────────┐     HTTP/HTTPS      ┌─────────────────────┐     EF Core      ┌──────────────┐
│  Angular 15     │ ◄─────────────────► │  ASP.NET Core 8 API │ ◄──────────────► │   MySQL      │
│  (Frontend)     │     REST + SignalR   │  (Backend)          │                  │   Database   │
└─────────────────┘                     └─────────────────────┘                  └──────────────┘
         │                                        │
         │                                        │
         └──────────── JWT Auth ──────────────────┘
```

### 1.2 Test Types in This Plan

| Type | Scope | Tooling | Purpose |
|------|-------|---------|---------|
| **API Integration** | Backend API ↔ DB | xUnit, WebApplicationFactory | Verify API contracts, auth, DB persistence |
| **UI E2E** | Browser → API → DB | Cypress or Playwright | Verify full user flows in real browser |
| **Cross-Cutting** | Auth, SignalR, Files | Both | Verify auth flow, real-time, file uploads |

### 1.3 Relationship to Existing Tests

- **Unit Tests** (`LogoDesignPortal.Application.Tests`): Service logic with mocks — already present.
- **QA Testing Guideline** (`docs/QA-TESTING-GUIDELINE.md`): Manual QA checklist — complements this plan.
- **This Plan**: Automated E2E/integration tests — fills the gap between unit tests and manual QA.

---

## 2. Test Environment

### 2.1 Requirements

| Component | Requirement |
|-----------|-------------|
| **Backend** | .NET 8, running on `http://localhost:5000` (or configurable) |
| **Frontend** | Angular 15, `ng serve` on `http://localhost:4200` |
| **Database** | MySQL test instance OR SQLite/InMemory for isolated API tests |
| **Node.js** | v18+ for Cypress/Playwright |
| **Browser** | Chrome (headless) for CI |

### 2.2 Environment Configuration

Create `Frontend/src/environments/environment.e2e.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000',  // Local backend for E2E
  apiVersion: ''
};
```

### 2.3 CI/CD Considerations

- Run API integration tests in CI (no browser required).
- Run UI E2E tests in CI with headless Chrome.
- Use test database with seed data; reset between runs if needed.
- Avoid shared state; use unique test data (e.g., timestamps in emails).

---

## 3. Test Scope & Boundaries

### 3.1 In Scope

| Area | What to Test |
|------|--------------|
| **Authentication** | Login, logout, token refresh, protected routes, role-based access |
| **Order Lifecycle** | Create → Approve → Assign → Preview → Revision → Complete |
| **File Operations** | Upload (reference, preview), download, temporary vs permanent |
| **Invoices** | Create, list, mark paid |
| **Notifications** | Creation, delivery, real-time (SignalR) |
| **Role-Based Routing** | AuthGuard, RoleGuard, permission-based UI |
| **API Contracts** | Request/response shapes, error codes, validation |

### 3.2 Out of Scope (or Lower Priority)

| Area | Reason |
|------|--------|
| Third-party payment gateways | Use mocks or sandbox |
| Email delivery | Mock SMTP or skip |
| Performance/load | Separate performance test plan |
| Visual regression | Optional; separate tooling |

---

## 4. Critical User Journeys

### 4.1 Journey 1: Client Creates Order (P0)

**Actors:** Client  
**Flow:** Login → Create Order (with files) → Order appears in list → Status = WaitingForAdminApproval

| Step | Action | Expected Result | API/DB Assertion |
|------|--------|-----------------|------------------|
| 1 | POST `/api/auth/login` | 200, tokens returned | — |
| 2 | POST `/api/orders` or `with-files` | 201, order ID | Order in DB, status = WaitingForAdminApproval |
| 3 | GET `/api/orders/my-orders` | Order in list | — |
| 4 | GET `/api/orders/{id}` | Order detail matches | — |

**UI E2E:** Login → Navigate to Create Order → Fill form → Upload file → Submit → Redirect to order detail.

---

### 4.2 Journey 2: Admin Approves & Assigns Order (P0)

**Actors:** Admin  
**Flow:** View new order → Approve price → Assign designer → Status = InProgress

| Step | Action | Expected Result | API/DB Assertion |
|------|--------|-----------------|------------------|
| 1 | Login as Admin | — | — |
| 2 | POST `/api/orders/{id}/request-price-approval` | 200 | Status = PriceApprovalPending |
| 3 | (Client) POST `/api/orders/{id}/approve-price` | 200 | Status = InProgress (or next) |
| 4 | POST `/api/orders/{id}/assign` (designerId) | 200 | Order.DesignerId set |
| 5 | GET `/api/orders/assigned-orders` (Designer) | Order in list | — |

**UI E2E:** Admin login → Orders → Open order → Request price approval → Client approves → Admin assigns designer.

---

### 4.3 Journey 3: Designer Delivers Preview (P0)

**Actors:** Designer  
**Flow:** View assigned order → Upload preview files → Status = PreviewDelivered

| Step | Action | Expected Result | API/DB Assertion |
|------|--------|-----------------|------------------|
| 1 | Login as Designer | — | — |
| 2 | GET `/api/orders/assigned-orders` | Order visible | — |
| 3 | POST `/api/files/upload` (preview) | 200, file ID | File linked to order |
| 4 | PUT `/api/orders/{id}/status` → PreviewDelivered | 200 | Status = PreviewDelivered |

**UI E2E:** Designer login → Orders → Open order → Upload preview → Update status.

---

### 4.4 Journey 4: Client Requests Revision (P1)

**Actors:** Client, Designer  
**Flow:** Client requests revision → Designer uploads new preview → Client approves

| Step | Action | Expected Result | API/DB Assertion |
|------|--------|-----------------|------------------|
| 1 | PUT `/api/orders/{id}/status` → RevisionRequested | 200 | Status = RevisionRequested |
| 2 | POST `/api/comments` (revision feedback) | 201 | Comment saved |
| 3 | Designer uploads new preview | 200 | New revision file |
| 4 | PUT `/api/orders/{id}/status` → PreviewDelivered | 200 | — |
| 5 | PUT `/api/orders/{id}/status` → ClientApproved | 200 | Status = ClientApproved |

---

### 4.5 Journey 5: Order Completed (P0)

**Actors:** Admin  
**Flow:** Admin marks order as Completed → Client sees in gallery (if applicable)

| Step | Action | Expected Result | API/DB Assertion |
|------|--------|-----------------|------------------|
| 1 | PUT `/api/orders/{id}/status` → Completed | 200 | Status = Completed |
| 2 | GET `/api/gallery` (Client) | Approved logo in gallery | GalleryItem exists |

---

### 4.6 Journey 6: Invoice Creation & Payment (P1)

**Actors:** Admin, Client  
**Flow:** Admin creates invoice → Client views → Mark paid

| Step | Action | Expected Result | API/DB Assertion |
|------|--------|-----------------|------------------|
| 1 | POST `/api/invoices` | 201, invoice ID | Invoice in DB |
| 2 | GET `/api/invoices` (Client) | Invoice in list | — |
| 3 | POST `/api/payments` or mark paid | 200 | Invoice status updated |

---

### 4.7 Journey 7: Real-Time Notifications (P2)

**Actors:** Client, Admin  
**Flow:** Client creates order → Admin receives notification via SignalR

| Step | Action | Expected Result | API/DB Assertion |
|------|--------|-----------------|------------------|
| 1 | Admin connects to `/hubs/notifications` | Connection established | — |
| 2 | Client creates order | — | Notification created in DB |
| 3 | Admin receives SignalR message | Notification payload | — |
| 4 | GET `/api/notifications` | Unread count updated | — |

---

## 5. API Integration Tests

### 5.1 Approach

Use **WebApplicationFactory** to host the real API in-process with a test database (SQLite or MySQL test instance). No browser required.

### 5.2 Project Structure

```
Backend/src/
├── LogoDesignPortal.API/
├── LogoDesignPortal.API.IntegrationTests/   ← NEW
│   ├── LogoDesignPortal.API.IntegrationTests.csproj
│   ├── TestWebApplicationFactory.cs
│   ├── Helpers/
│   │   ├── AuthHelper.cs
│   │   └── SeedDataHelper.cs
│   └── Controllers/
│       ├── AuthControllerTests.cs
│       ├── OrdersControllerTests.cs
│       ├── InvoicesControllerTests.cs
│       └── FilesControllerTests.cs
```

### 5.3 Test Cases (API Level)

#### AuthController

| Test | Description | Assertions |
|------|-------------|------------|
| `Login_ValidCredentials_ReturnsTokens` | POST login with valid user | 200, access_token, refresh_token |
| `Login_InvalidPassword_Returns401` | Wrong password | 401 |
| `Login_InactiveUser_Returns401` | Inactive user | 401 |
| `Login_Lockout_Returns429` | 5 failed attempts | 429, lockout message |
| `RefreshToken_ValidToken_ReturnsNewAccessToken` | POST refresh | 200, new access_token |
| `RefreshToken_InvalidToken_Returns401` | Expired/invalid refresh | 401 |
| `ProtectedEndpoint_NoToken_Returns401` | GET /api/orders without auth | 401 |
| `ProtectedEndpoint_ValidToken_Returns200` | GET with Bearer token | 200 |
| `RoleGuard_ClientAccessingAdminEndpoint_Returns403` | Client → GET /api/users | 403 |

#### OrdersController

| Test | Description | Assertions |
|------|-------------|------------|
| `CreateOrder_AsClient_Succeeds` | POST order as Client | 201, order in DB |
| `CreateOrder_AsDesigner_Returns403` | POST order as Designer | 403 |
| `GetMyOrders_AsClient_ReturnsOnlyOwn` | GET my-orders | Only client's orders |
| `GetAssignedOrders_AsDesigner_ReturnsAssigned` | GET assigned-orders | Only assigned |
| `GetAllOrders_AsAdmin_ReturnsAll` | GET (ViewAllOrders) | All orders |
| `AssignDesigner_AsAdmin_Succeeds` | POST assign | DesignerId set |
| `AssignDesigner_AsClient_Returns403` | POST assign as Client | 403 |
| `UpdateStatus_ValidTransition_Succeeds` | PUT status | Status updated |
| `UpdateStatus_InvalidTransition_Returns400` | Invalid transition | 400 |
| `ApprovePrice_AsClient_Succeeds` | POST approve-price | Status updated |

#### FilesController

| Test | Description | Assertions |
|------|-------------|------------|
| `Upload_ReferenceFile_AsClient_Succeeds` | Upload to own order | 201, file in DB |
| `Upload_ToOthersOrder_Returns403` | Upload to non-own order | 403 |
| `Download_AuthorizedUser_Succeeds` | GET file | 200, content |
| `Download_Unauthorized_Returns403` | GET file without access | 403 |

#### InvoicesController

| Test | Description | Assertions |
|------|-------------|------------|
| `CreateInvoice_AsAdmin_Succeeds` | POST invoice | 201 |
| `GetInvoices_AsClient_ReturnsOwn` | GET as Client | Only client's invoices |
| `GetInvoices_AsAdmin_ReturnsAll` | GET as Admin | All invoices |

### 5.4 Dependencies for API Integration Tests

```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
<!-- Or use Testcontainers.MySql for real MySQL -->
```

---

## 6. UI E2E Tests

### 6.1 Tool Recommendation

| Tool | Pros | Cons |
|------|------|------|
| **Cypress** | Great DX, time-travel, stable selectors | Single tab, different from Playwright |
| **Playwright** | Multi-browser, fast, API testing | Newer, smaller ecosystem |

**Recommendation:** Playwright for new projects (multi-browser, API + UI). Cypress if team already uses it.

### 6.2 Project Structure (Playwright)

```
Frontend/
├── e2e/
│   ├── playwright.config.ts
│   ├── tests/
│   │   ├── auth/
│   │   │   ├── login.spec.ts
│   │   │   └── logout.spec.ts
│   │   ├── orders/
│   │   │   ├── create-order.spec.ts
│   │   │   ├── order-list.spec.ts
│   │   │   └── order-detail.spec.ts
│   │   ├── invoices/
│   │   │   └── invoice-list.spec.ts
│   │   └── fixtures/
│   │       └── auth.fixture.ts
│   └── page-objects/
│       ├── login.page.ts
│       ├── dashboard.page.ts
│       └── order-create.page.ts
```

### 6.3 Test Cases (UI Level)

#### Auth

| Test | Description | Key Steps |
|------|-------------|-----------|
| `Login_ValidCredentials_RedirectsToDashboard` | Full login flow | Fill form → Submit → URL = /dashboard |
| `Login_InvalidCredentials_ShowsError` | Wrong credentials | Submit → Error message visible |
| `Logout_ClearsSession_RedirectsToLogin` | Logout flow | Click logout → URL = /auth/login |
| `Unauthenticated_AccessProtectedRoute_RedirectsToLogin` | AuthGuard | Visit /dashboard → Redirect |

#### Orders

| Test | Description | Key Steps |
|------|-------------|-----------|
| `Client_CreateOrder_Success` | Create order with files | Login as Client → Create → Fill → Upload → Submit → Order detail |
| `Client_ViewMyOrders_OnlyOwnOrders` | Order list filtered | Login as Client → Orders → Verify list |
| `Admin_AssignDesigner_Success` | Assign flow | Login as Admin → Order → Assign → Verify |
| `Designer_ViewAssignedOrders_OnlyAssigned` | Designer list | Login as Designer → Orders → Verify |

#### Role-Based Access

| Test | Description | Key Steps |
|------|-------------|-----------|
| `Client_AccessUsers_RedirectedOr403` | RoleGuard | Login as Client → Visit /users → Blocked |
| `Admin_AccessUsers_Success` | Admin access | Login as Admin → Visit /users → List loads |
| `Client_AccessGallery_Success` | Client gallery | Login as Client → Visit /gallery → Loads |
| `Admin_AccessGallery_Blocked` | Gallery Client-only | Login as Admin → Visit /gallery → Blocked |

### 6.4 Environment Setup for E2E

```bash
# Terminal 1: Start backend
cd Backend/src/LogoDesignPortal.API && dotnet run

# Terminal 2: Start frontend (E2E config)
cd Frontend && ng serve --configuration=e2e

# Terminal 3: Run E2E tests
cd Frontend && npx playwright test
```

---

## 7. Test Data Strategy

### 7.1 Seed Data

| Role | Email (example) | Password | Use Case |
|------|-----------------|----------|----------|
| SuperAdmin | superadmin@logodesign.com | SuperAdmin@123 | Full access tests |
| Admin | admin@test.com | Admin@123 | Admin flows |
| Client | client@test.com | Client@123 | Client order creation |
| Designer | designer@test.com | Designer@123 | Designer flows |

**Note:** Use unique emails per test run (e.g., `client-{Guid}@test.com`) to avoid conflicts in parallel runs.

### 7.2 Test Data Isolation

- **API Integration:** Use in-memory SQLite or Testcontainers; reset DB before each test class.
- **UI E2E:** Use API to create test data (e.g., create order via API, then assert in UI) OR seed once per run.
- **Avoid:** Shared mutable state; hardcoded IDs that may change.

### 7.3 Order Status Test Matrix

| From Status | To Status | Allowed Roles |
|-------------|-----------|---------------|
| WaitingForAdminApproval | PriceApprovalPending | Admin |
| PriceApprovalPending | InProgress | Admin |
| InProgress | PreviewDelivered | Designer, Admin |
| PreviewDelivered | RevisionRequested | Client |
| PreviewDelivered | ClientApproved | Client |
| RevisionRequested | PreviewDelivered | Designer |
| ClientApproved | Completed | Admin |
| * | Cancelled, CancelledByUser, CancelledByAdmin | Varies |

---

## 8. Implementation Roadmap

### Phase 1: API Integration Tests (2–3 days)

1. Create `LogoDesignPortal.API.IntegrationTests` project.
2. Implement `TestWebApplicationFactory` with test DB.
3. Add `AuthHelper` for JWT tokens.
4. Implement AuthController and OrdersController tests.
5. Run in CI.

### Phase 2: UI E2E Setup (1–2 days)

1. Add Playwright (or Cypress) to Frontend.
2. Create `environment.e2e.ts`.
3. Implement auth fixture and login page object.
4. Implement 3–5 critical E2E tests (login, create order, order list).
5. Add npm script: `e2e`.

### Phase 3: Expand Coverage (3–5 days)

1. Add Files, Invoices, Notifications API tests.
2. Add role-based E2E tests.
3. Add order lifecycle E2E (full flow).
4. Document flaky test handling.

### Phase 4: CI Integration (1 day)

1. Add GitHub Actions (or existing CI) job for API integration tests.
2. Add job for UI E2E (with backend + frontend services).
3. Fail PR on test failure.

---

## 9. Appendix

### A. Order Status Flow Diagram

```
WaitingForAdminApproval
        │
        ▼
PriceApprovalPending
        │
        ▼
   InProgress
        │
        ▼
PreviewDelivered
        │
        ├──────────────────┐
        ▼                  ▼
RevisionRequested    ClientApproved
        │                  │
        └──────┬───────────┘
               ▼
         Completed
```

### B. API Base URLs

| Environment | API URL |
|-------------|---------|
| Local | http://localhost:5000 |
| E2E | http://localhost:5000 |
| Staging | http://api.hawkmerchandising.com |
| Production | https://api.hawkmerchandising.com |

### C. Key Frontend Routes

| Route | Access | Module |
|-------|--------|--------|
| /auth/login | Public | Auth |
| /dashboard | All authenticated | Dashboard |
| /orders | All | Orders |
| /orders/create | Client | Orders |
| /users | SuperAdmin, Admin | Users |
| /clients | SuperAdmin, Admin | Clients |
| /designers | SuperAdmin, Admin | Designers |
| /invoices | SuperAdmin, Admin, Client | Invoices |
| /gallery | Client | Gallery |
| /permissions | SuperAdmin | Permissions |
| /settings | All | Settings |
| /notifications | All | Notifications |

### D. References

- [QA Testing Guideline](./QA-TESTING-GUIDELINE.md) — Manual QA checklist
- [Analytics Data Map](./ANALYTICS_DATA_MAP.md)
- [Notification Troubleshooting](./NOTIFICATION_TROUBLESHOOTING.md)
- [ASP.NET Core Integration Testing](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Playwright Documentation](https://playwright.dev/)
- [Cypress Documentation](https://docs.cypress.io/)

---

*End of E2E Integration Test Plan*
