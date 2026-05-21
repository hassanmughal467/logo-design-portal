# Invoice, Payment & Revision Module — Test Engineering Guide

## Test pyramid

```
Playwright E2E (Frontend/e2e/tests/security|workflows)
        ▲
API Integration (LogoDesignPortal.API.IntegrationTests)
        ▲
Application unit (LogoDesignPortal.Application.Tests)
```

---

## Naming conventions

| Layer | Pattern | Example |
|-------|---------|---------|
| Integration | `{Controller}_{Scenario}_{Expected}` | `GetInvoiceById_AsOtherClient_Returns404` |
| Application | `{Service}_{Behavior}_{Condition}` | `GetPaymentByIdWithAccess_OtherClient_ReturnsNull` |
| Playwright | `{area} — {behavior}` | `client cannot mark invoice paid` |
| Regression | `{Area}WorkflowRegression_{Flow}` | `InvoicePaymentWorkflowRegression_CreateMarkPaid` |

---

## Directory structure

```
Backend/src/
  LogoDesignPortal.Application.Tests/
    Helpers/RevisionLimitHelperTests.cs
    Services/InvoiceServiceAccessTests.cs
    Services/PaymentServiceAccessTests.cs
    Services/RevisionWorkflowTests.cs          (existing)
    Services/InvoiceServiceTests.cs            (existing)
  LogoDesignPortal.API.IntegrationTests/
    Controllers/
      InvoicesControllerTests.cs               (existing)
      InvoicesControllerAuthorizationTests.cs
      InvoicesControllerPrivacyTests.cs
      PaymentsControllerAuthorizationTests.cs
      PaymentsControllerPrivacyTests.cs
      PaymentsControllerWebhookSecurityTests.cs
      RevisionsControllerAuthorizationTests.cs
      RevisionsControllerWorkflowTests.cs
    Regression/
      InvoicePaymentWorkflowRegressionTests.cs
    IntegrationDatabaseHelper.cs
    Support/Factories/TestOrderFactory.cs

Frontend/e2e/tests/
  workflows/invoice-payment-permissions.api.spec.ts
  security/invoice-payment-revision.security.spec.ts
```

---

## Fixtures & shared setup

### Integration (`[Collection("Integration")]`)

- **Factory:** `TestWebApplicationFactory` — in-memory DB, seeded users.
- **Auth:** `AuthHelper` tokens (client, designer, superadmin).
- **Billing setup:** `IntegrationDatabaseHelper.InsertCompletedBillableOrderAsync`, `InsertOtherClientInvoiceAsync`, `InsertPreviewDeliveredOrderAsync`.
- **JSON:** `IntegrationTestJson.Options`.
- **Revision multipart:** `MultipartTestHelper.CreateRevisionRequestForm`.

### Application unit

- In-memory `ApplicationDbContext` for access-control tests on `InvoiceService` / `PaymentService`.
- `RevisionLimitHelperTests` — pure logic, no DB.

---

## Running tests locally

```bash
# All integration tests for this module
dotnet test Backend/src/LogoDesignPortal.API.IntegrationTests \
  --filter "FullyQualifiedName~InvoicesController|FullyQualifiedName~PaymentsController|FullyQualifiedName~RevisionsController|FullyQualifiedName~InvoicePayment"

# Application unit (revision limits + financial access)
dotnet test Backend/src/LogoDesignPortal.Application.Tests \
  --filter "FullyQualifiedName~RevisionLimit|FullyQualifiedName~InvoiceServiceAccess|FullyQualifiedName~PaymentServiceAccess"

# Playwright (API workflow specs)
cd Frontend/e2e && npx playwright test invoice-payment
```

---

## CI

Same workflows as order/file module: `.github/workflows/test.yml` runs backend test projects; `e2e-playwright.yml` runs Playwright when configured with API URL + seeded credentials.

---

## Known gaps / follow-ups

1. **SEC-PAY-02 (fixed):** `PaymentInvoiceAccessHelper` enforces ownership on create/link/process; designers receive 403.
2. **Designer invoice visibility (client billing):** Non-client roles can read any client invoice — confirm product intent.
3. **PayPal webhook processing:** Signature verified but event handling is TODO — add tests when implemented.

Designer payout module: see `docs/TESTING_DESIGNER_PAYOUT_MODULE.md`.
