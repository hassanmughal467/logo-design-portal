# Logo Design Portal — Automated Testing Suite Implementation

**Version:** 1.0  
**Last Updated:** March 11, 2026  
**Purpose:** Comprehensive automated testing for production reliability.

---

## Overview

The Logo Design Portal testing suite implements three levels of testing:

| Level | Scope | Framework | Location |
|-------|-------|-----------|----------|
| **1. Unit Tests** | Individual services | xUnit, Moq, EF InMemory | `Backend/src/LogoDesignPortal.Application.Tests/` |
| **2. API Integration Tests** | API endpoints + DB | xUnit, WebApplicationFactory | `Backend/src/LogoDesignPortal.API.IntegrationTests/` |
| **3. E2E Tests** | Full workflow (planned) | Playwright/Cypress | `Frontend/e2e/` (see E2E-INTEGRATION-TEST-PLAN.md) |

---

## 1. Unit Tests (Backend)

### Services Covered

| Service | Test File | Key Tests |
|---------|-----------|-----------|
| **OrderService** | OrderServiceWorkflowTests.cs | CreateOrderAsync, AssignDesignerAsync, RequestPriceApprovalAsync, ApprovePriceAsync, ApproveOrderAsync |
| **OrderService** | OrderServiceStatusTransitionTests.cs | Status transitions, role-based permissions |
| **OrderService** | OrderServiceFullBatchTests.cs | Preview batch delivery rules |
| **BillingService** | BillingServiceTests.cs | GetBillingQueueOverviewAsync, GetEligibleOrdersForClientAsync, CreateInvoiceFromOrdersAsync |
| **DesignerPayoutService** | DesignerPayoutServiceTests.cs | SubmitDesignerPricingAsync, GetDesignPricingInfoAsync, GetDesignerPayoutEligibleOrdersAsync |
| **RevisionService** | RevisionWorkflowTests.cs | Full revision flow |
| **FileService** | FileServiceUploadAuthorizationTests.cs | Upload authorization |
| **InvoiceService** | InvoiceServiceTests.cs | Statistics, MarkInvoiceAsPaid |

### Running Unit Tests

```bash
cd Backend
dotnet test src/LogoDesignPortal.Application.Tests/LogoDesignPortal.Application.Tests.csproj
```

### Coverage Areas

- **Valid inputs** — Happy path scenarios
- **Invalid inputs** — Error handling, validation
- **Edge cases** — Boundary conditions
- **Authorization rules** — Role-based access (Client, Designer, Admin)
- **Status transitions** — Order workflow state machine
- **Financial logic** — Billing eligibility, invoice generation, designer payout

---

## 2. API Integration Tests

### Project Structure

```
Backend/src/LogoDesignPortal.API.IntegrationTests/
├── LogoDesignPortal.API.IntegrationTests.csproj
├── TestWebApplicationFactory.cs      # In-memory DB, test data seeding
├── Helpers/
│   └── AuthHelper.cs                 # JWT token acquisition
└── Controllers/
    └── OrdersControllerTests.cs      # Orders API tests
```

### Test Cases

| Test | Description |
|------|-------------|
| CreateOrder_AsClient_Succeeds | POST /api/orders as Client → 201, order saved |
| CreateOrder_WithoutAuth_Returns401 | POST without token → 401 |
| GetMyOrders_AsClient_ReturnsOnlyOwnOrders | GET my-orders filtered by client |
| GetAssignedOrders_AsDesigner_ReturnsAssignedOrders | GET assigned-orders for designer |
| AssignDesigner_AsAdmin_Succeeds | POST assign → designer assigned, status InProgress |

### Running Integration Tests

```bash
cd Backend
dotnet test src/LogoDesignPortal.API.IntegrationTests/LogoDesignPortal.API.IntegrationTests.csproj
```

### Known Issues

- **Login 401 in integration tests:** The test WebApplicationFactory seeds users in an in-memory database. If the AuthService resolves a different DbContext (e.g., from configuration), login will fail. **Resolution:** Ensure DbContext replacement in `ConfigureWebHost` runs after `AddInfrastructure` and that the same in-memory database is used for all requests.

---

## 3. E2E Tests (Planned)

See [E2E-INTEGRATION-TEST-PLAN.md](./E2E-INTEGRATION-TEST-PLAN.md) for:

- Playwright/Cypress setup
- Full order lifecycle workflow
- Permission tests
- Data integrity tests
- SignalR notification tests

---

## CI/CD Integration

### GitHub Actions (Example)

```yaml
# .github/workflows/test.yml
name: Test Suite

on:
  pull_request:
    branches: [main, develop]
  push:
    branches: [main, develop]

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore Backend/LogoDesignPortal.sln
      - run: dotnet test Backend/src/LogoDesignPortal.Application.Tests/LogoDesignPortal.Application.Tests.csproj --no-build --verbosity normal

  integration-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore Backend/LogoDesignPortal.sln
      - run: dotnet test Backend/src/LogoDesignPortal.API.IntegrationTests/LogoDesignPortal.API.IntegrationTests.csproj --no-build --verbosity normal
```

### Run All Tests

```bash
cd Backend
dotnet test LogoDesignPortal.sln
```

---

## Test Data Management

### Unit Tests

- Use `UseInMemoryDatabase` with unique names per test: `"Db_" + Guid.NewGuid()`
- Seed data in each test or shared fixture
- No cleanup required (in-memory is disposed)

### Integration Tests

- `TestWebApplicationFactory` seeds: SuperAdmin, Admin, Client, Designer
- Passwords: `Test@123` (all test users)
- Emails: `superadmin@test.com`, `admin@test.com`, `client@test.com`, `designer@test.com`

---

## Reporting

### Coverage (with Coverlet)

```bash
dotnet test Backend/src/LogoDesignPortal.Application.Tests/LogoDesignPortal.Application.Tests.csproj \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults
```

### Key Metrics to Track

- **Pass/fail status** — All tests green before merge
- **Workflow coverage** — Order lifecycle, billing, designer payout
- **Failed scenarios** — Highlight impact on order workflow, client billing, designer payouts

---

## References

- [E2E Integration Test Plan](./E2E-INTEGRATION-TEST-PLAN.md)
- [QA Testing Guideline](./QA-TESTING-GUIDELINE.md)
- [Production Readiness Verification Report](./PRODUCTION-READINESS-VERIFICATION-REPORT.md)
