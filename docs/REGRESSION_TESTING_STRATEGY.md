# Regression Testing Strategy

## Goals

Prevent recurrence of Week 1 security and workflow fixes without slowing every PR.

## Suites

| Suite | When | Location |
|-------|------|----------|
| **Smoke** | Every PR | `Frontend/e2e/tests/smoke/` |
| **Security regression** | Every PR | Integration `Security/`, `Regression/`, E2E `security/`, `regression/` |
| **Financial** | Every PR touching billing | `MarkPaidDuplicateRegressionTests`, `InvoicePaymentWorkflowRegressionTests` |
| **Order/file** | Orders/uploads changes | `OrderFileWorkflowRegressionTests`, upload integration + E2E |
| **State machine** | Status transitions | `OrderStatusStateMachineTests`, `OrdersControllerTests` invalid transition |
| **Full workflow** | Nightly | `order-full-lifecycle`, `elite/full-business-lifecycle` |

## High-risk areas (mandatory regression)

1. Order status transitions (`OrderStatusStateMachine`)
2. Invoice mark-paid idempotency
3. File download IDOR + hidden preview visibility
4. Designer/client DTO masking
5. Payment webhooks and mark-paid concurrency
6. Upload duplicate window (1 hour)

## Bug reproduction pattern

1. Add failing integration test (fast, deterministic)
2. Add Playwright API spec if cross-role UI contract matters
3. Reference ticket ID in test name or comment
4. Link test in `docs/QA_*_MODULE.md` if module-specific

## CI gate suggestion

```
dotnet test LogoDesignPortal.API.IntegrationTests --filter "Category!=Slow"
npx playwright test tests/smoke tests/regression tests/security
```
