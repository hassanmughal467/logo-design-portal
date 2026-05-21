# Regression Protection Status

## Automated regression suites

| Suite | Location | Stability | CI |
|-------|----------|-----------|-----|
| Order + file workflow | `Regression/OrderFileWorkflowRegressionTests.cs` | High | Yes |
| Invoice + payment | `Regression/InvoicePaymentWorkflowRegressionTests.cs` | High | Yes |
| Mark-paid duplicate | `Regression/MarkPaidDuplicateRegressionTests.cs` | High | Yes |
| Order status state machine | `Domain.Tests/OrderStatusStateMachineTests.cs` | High | Yes |
| Auth security | `Security/AuthSecurityIntegrationTests.cs` | High | Yes |
| Refresh replay | `Security/RefreshTokenAndReplayIntegrationTests.cs` | High | Yes |
| Upload security | `FilesControllerUploadSecurityIntegrationTests.cs` | High | Yes |
| Order concurrency | `OrdersControllerConcurrencyTests.cs` | Medium-High | Yes |
| Playwright security API | `e2e/tests/security/*.api.spec.ts` | High | Partial (PR smoke) |

## Protection mechanisms

1. **Integration collection fixture** — isolated in-memory DB per factory
2. **Deterministic seed** — `TestDataSeeder`, `IntegrationDatabaseHelper`
3. **AAA pattern** — enforced in module guides
4. **80% line coverage gate** — integration project matrix
5. **Production config tests** — prevent unsafe appsettings regression

## Weak points

| Risk | Mitigation |
|------|------------|
| PR only runs 3 Playwright tests | Expand smoke or require nightly green |
| `test.skip` in IDOR E2E | Remove skip when seed stable |
| Karma `random: true` | Set `random: false` in CI |
| PayPal webhook body untested | Add when TODO implemented |

## Concurrency protection

- `OrdersControllerConcurrencyTests` — assign/status races
- Invoice mark-paid duplicate regression
- File upload duplicate window — `FileServiceUploadAbuseTests`

## Recommendation

**Regression protection: ADEQUATE for backend launch.**  
Frontend UI regressions rely primarily on manual QA and API-level E2E — increase component tests in Phase 2.
