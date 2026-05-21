# QA Testing Strategy — Logo Design Portal

**Last updated:** May 2026

---

## 1. Test pyramid

```
        ┌─────────────┐
        │  Playwright │  E2E (Frontend/e2e)
        ├─────────────┤
        │ Integration │  API.IntegrationTests
        ├─────────────┤
        │  Unit/xUnit │  Domain + Application.Tests
        └─────────────┘
```

| Layer | Location | CI |
|-------|----------|-----|
| Domain unit | `Backend/src/LogoDesignPortal.Domain.Tests` | `test.yml` |
| Application unit | `Backend/src/LogoDesignPortal.Application.Tests` | `test.yml` |
| API integration | `Backend/src/LogoDesignPortal.API.IntegrationTests` | `test.yml` |
| Angular unit | `Frontend` Karma | `test.yml` |
| E2E | `Frontend/e2e` | `e2e-playwright.yml` |

Coverage gates: ≥80% line coverage per backend project (Coverlet runsettings).

---

## 2. Critical flows (must pass before release)

| Flow | Backend tests | E2E |
|------|---------------|-----|
| Login / refresh | `AuthControllerTests` | `auth/login.spec.ts`, `auth/session.spec.ts` |
| Registration | Auth service tests | Extend E2E |
| Password reset | Auth tests | Manual / E2E |
| Order create | `OrderService` tests | `order-full-lifecycle.spec.ts` |
| Assign designer | Integration orders | `order-assign-designer.spec.ts` |
| Preview upload | `FileServiceUploadAuthorizationTests` | Lifecycle specs |
| Revision / approval | `RevisionWorkflowTests` | Lifecycle specs |
| Invoice generation | `InvoicesControllerTests` | `billing/invoice-list-api.spec.ts` |
| Payout / refunds | `DesignerPayoutServiceTests` | `payments-flow.spec.ts` |
| Realtime notifications | Manual + hub auth | Dashboard/list sync specs |

---

## 3. Invalid / security flows

| Scenario | Test type | Status |
|----------|-----------|--------|
| Unauthorized API access | Integration `401/403` | Partial |
| Client downloads hidden file | `FileServiceDownloadVisibilityTests`, `FilesControllerDownloadSecurityTests` | ✅ Added |
| Cross-role order access | `OrderService` auth tests | Existing |
| Invalid state transition | `OrderStatusStateMachineTests` | Domain |
| Invalid upload (type/size) | `UploadSecurityHelperTests`, form abuse E2E | Partial |
| Rate limit auth | Manual / load test | Documented |
| Concurrent order update | `SystemStressWorkflowTests` | Application |
| Permission escalation | `security/rbac-and-token.spec.ts` | E2E |
| Duplicate upload | FileService duplicate window | ✅ Added |

---

## 4. Permission tests (roadmap)

1. Matrix: Role × Endpoint × Expected status — generate from Swagger + controller attributes.
2. Assert masked DTO fields never contain designer email for Client role.
3. SuperAdmin vs Admin permission grants via `PermissionsController`.

---

## 5. SignalR tests

- Hub: authenticated user can only join own `user-{id}` group.
- Integration: connect with valid/invalid token (extend `TestWebApplicationFactory`).
- E2E: order list updates on `OrderStatusChanged` event.

---

## 6. Invoice / payment integrity

- `DesignerPayoutServiceTests` — pricing approval transitions.
- `InvoicesControllerTests` — generation guards with `ProductionSafety`.
- Financial regression: totals match order line items (add golden-file tests).

---

## 7. Playwright E2E structure

```
Frontend/e2e/
  tests/auth/
  tests/security/
  tests/workflows/
  tests/uat/
  pom/          # Page objects
  utils/        # api-helpers, env
```

Configure via `Frontend/e2e/.env.example` (API URL, credentials).

---

## 8. QA workflow

1. **PR:** CI unit + integration + Angular tests.
2. **Pre-release:** Full Playwright against staging; manual `docs/MANUAL_USER_TESTING_GUIDE.md`.
3. **Post-deploy:** Smoke login, create order, upload, health `/health/ready`.
4. **Regression:** `docs/ENTERPRISE_QA_TEST_SCENARIOS.md` + CSV cases.

---

## 9. Test data

- Integration: `TestDataSeeder` — client/admin/designer/superadmin @ `Test@123`.
- E2E: `test-data.factory.ts`, `example-users.json`.

---

## 10. Gaps to close (priority)

| Gap | Effort |
|-----|--------|
| Full file-upload abuse integration tests | M |
| Webhook/payment replay tests (if external PSP added) | L |
| Permission matrix automation | M |
| SignalR integration tests | M |
| Enable `file-download-idor.spec.ts` with API fixtures | S |
