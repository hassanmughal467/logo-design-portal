# Order, File & Auth Module — Test Engineering Guide

## Test pyramid

```
Playwright E2E (Frontend/e2e/tests/security|workflows)
        ▲
API Integration (LogoDesignPortal.API.IntegrationTests)
        ▲
Application unit (LogoDesignPortal.Application.Tests)
        ▲
Domain unit (LogoDesignPortal.Domain.Tests)
```

---

## Naming conventions

| Layer | Pattern | Example |
|-------|---------|---------|
| Integration | `{Controller}_{Scenario}_{Expected}` | `Download_HiddenPreview_AsClient_Returns403` |
| Application | `{Service}_{Behavior}_{Condition}` | `UploadFile_DuplicateWithinWindow_Throws` |
| Domain | `ValidateTransition_{Case}` | `ValidateTransition_Disallowed_ThrowsInvalidOperation` |
| Playwright | `{area} — {behavior}` / `test('...')` | `client cannot download hidden preview via API` |
| Regression | `{Area}Regression_{Flow}` | `OrderFileWorkflowRegression_CreateAssignUpload_Passes` |

Prefix security tests with context: `Security_`, `Authorization_`, `UploadAbuse_`.

---

## Directory structure

```
Backend/src/
  LogoDesignPortal.Domain.Tests/
    OrderStatusStateMachineTests.cs
    OrderStatusStateMachineRegressionTests.cs
  LogoDesignPortal.Application.Tests/
    Services/FileServiceUploadAbuseTests.cs
    Regression/OrderStatusTransitionRegressionTests.cs
    TestHelpers/FormFileTestHelper.cs
  LogoDesignPortal.API.IntegrationTests/
    Controllers/
      OrdersControllerTests.cs              (existing)
      OrdersControllerAuthorizationTests.cs
      OrdersControllerPrivacyTests.cs
      OrdersControllerConcurrencyTests.cs
      FilesControllerDownloadSecurityTests.cs
      FilesControllerUploadSecurityIntegrationTests.cs
    Regression/OrderFileWorkflowRegressionTests.cs
    Helpers/
      AuthHelper.cs
      IntegrationTestJson.cs
      MultipartTestHelper.cs
    Support/Factories/
      TestOrderFactory.cs
      TestLogoFileFactory.cs

Frontend/e2e/tests/
  security/order-file-auth.security.spec.ts
  workflows/order-file-permissions.api.spec.ts
```

---

## Fixtures & shared setup

### Integration (`[Collection("Integration")]`)

- **Factory:** `TestWebApplicationFactory` — in-memory DB, `Testing` environment, seeded users.
- **Auth:** `AuthHelper.GetClientTokenAsync` / `GetDesignerTokenAsync` / `GetSuperAdminTokenAsync`.
- **Data:** `TestDataIds`, `TestOrderFactory`, `TestLogoFileFactory`.
- **JSON:** `IntegrationTestJson.Options` (camelCase + enum strings).
- **Multipart:** `MultipartTestHelper.CreatePngUpload`.

Each test class should use **its own `HttpClient`** from `factory.CreateClient()` when parallel auth headers differ.

### Application unit

- In-memory `ApplicationDbContext` + temp file directory per test class (`IDisposable`).
- `FormFileTestHelper.Create` for uploads.
- Moq: `INotificationService`, `IRealtimeEntityUpdateSender`, `IFileUploadScanHook`, `IDesignerPayoutService`.

### Playwright

- `request` fixture + `apiUrl()` / `loginApi()`.
- `provisionClientUser` + `teardownUsers` for disposable users.
- Env: `Frontend/e2e/.env` from `.env.example` (`E2E_API_URL`, admin credentials).

---

## Mocks (application layer)

Do **not** mock `OrderStatusStateMachine` — use real domain rules.

| Dependency | Mock strategy |
|------------|----------------|
| `INotificationService` | `Mock.Of<>` — ignore callbacks |
| `IRealtimeEntityUpdateSender` | `Mock.Of<>` |
| `IFileUploadScanHook` | `Mock.Of<>` or throw to test scan failure |
| `IDesignerPayoutService` | Setup only when preview upload triggers pricing |

---

## Local setup

### Backend tests

```powershell
cd Backend
dotnet test src/LogoDesignPortal.Domain.Tests/LogoDesignPortal.Domain.Tests.csproj -c Release
dotnet test src/LogoDesignPortal.Application.Tests/LogoDesignPortal.Application.Tests.csproj -c Release
dotnet test src/LogoDesignPortal.API.IntegrationTests/LogoDesignPortal.API.IntegrationTests.csproj -c Release
```

Filter module tests:

```powershell
dotnet test src/LogoDesignPortal.API.IntegrationTests/LogoDesignPortal.API.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~OrdersController|FullyQualifiedName~FilesController|FullyQualifiedName~OrderFile"
```

### Playwright

```powershell
cd Frontend/e2e
copy .env.example .env
# Set E2E_API_URL=https://localhost:44398 and admin credentials
npx playwright test tests/security/order-file-auth.security.spec.ts tests/workflows/order-file-permissions.api.spec.ts
```

---

## CI integration

Already wired in `.github/workflows/test.yml`:

1. **backend-tests** matrix runs Domain, Application, Integration with Coverlet ≥80%.
2. **e2e-playwright.yml** runs Playwright against configured API URL.

To gate PRs on module tests only (optional job step):

```yaml
- name: Order/File security integration tests
  working-directory: Backend
  run: |
    dotnet test src/LogoDesignPortal.API.IntegrationTests/LogoDesignPortal.API.IntegrationTests.csproj -c Release --no-build \
      --filter "FullyQualifiedName~Authorization|FullyQualifiedName~Privacy|FullyQualifiedName~UploadSecurity|FullyQualifiedName~DownloadSecurity|FullyQualifiedName~Concurrency|FullyQualifiedName~OrderFileWorkflow"
```

Add Playwright project tag in `playwright.config.ts` if you split projects:

```typescript
{ name: 'security-api', testMatch: /tests\/(security|workflows)\/.*\.api\.spec\.ts/ }
```

---

## Negative testing requirements

Every happy-path integration test should have at least one of:

- Missing auth (401)
- Wrong role (403)
- Invalid input (400)
- Invalid state transition (400)
- IDOR (403)

Upload abuse must include: bad extension, magic-byte mismatch, oversize (unit), duplicate window.

---

## Regression scope

Minimum regression suite (`OrderFileWorkflowRegressionTests`):

1. Client create order → WaitingForAdminApproval  
2. Admin assign → InProgress  
3. Invalid status jump blocked  
4. Hidden preview download blocked for client  
5. Client cancel → CancelledByUser  

Re-run on any change to `OrderService`, `FileService`, `OrderStatusStateMachine`, or permission attributes.
