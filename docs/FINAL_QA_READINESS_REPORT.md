# Final QA Readiness Report — Week 4

## Maturity scores

| Dimension | Score | Notes |
|-----------|-------|-------|
| Backend unit (Domain + Application) | **4.5 / 5** | State machine, upload helper, access tests |
| Backend integration | **4.5 / 5** | 80% line gate; security, concurrency, regression |
| E2E breadth | **3.5 / 5** | 46 specs; PR runs 3 smoke only |
| Frontend unit | **2 / 5** | 15 specs / ~89 components |
| Security QA | **4 / 5** | Strong API; CSRF undertested |
| Realtime QA | **1.5 / 5** | Health string only |
| **Overall QA** | **3.8 / 5** | Production-viable backend; FE pyramid thin |

## Critical workflows — status

| Workflow | Backend | E2E | Status |
|----------|---------|-----|--------|
| Login / refresh / lockout | Yes | Partial | PASS |
| Order lifecycle + state machine | Yes | Yes | PASS |
| Assign / concurrency | Yes | Partial | PASS |
| File upload abuse | Yes | API spec | PASS |
| Invoice privacy | Yes (fixed) | API spec | PASS |
| Mark-paid duplicate | Yes | Yes | PASS |
| PayPal webhook signature | Yes | — | PARTIAL (events TODO) |
| SignalR reconnect | No | Health only | GAP |
| CSRF cookie session | Yes | Partial | PASS (API) |

## CI enforcement

| Workflow | Scope |
|----------|-------|
| `pr-validation.yml` | Fast gates |
| `test.yml` | Full backend + prod Angular build + 3 Playwright smoke |
| `e2e-playwright.yml` | Full suite, MySQL service |

## Week 4 test additions

- `ProductionConfigurationValidationTests`
- `DatabaseInitializationSafetyTests`
- `InvoicesControllerPrivacyTests` — designer denial
- `InvoiceServiceAccessTests` — designer empty

## Flaky test policy

- Documented tension: retries in Playwright vs `TESTING_STANDARDS.md` no-retry policy
- **Recommendation:** Retries=0 on PR smoke; retries=2 only on nightly full suite

## Launch QA gate

- [ ] Full `dotnet test` on release branch
- [ ] `e2e-playwright.yml` green on release candidate
- [ ] Manual smoke: `docs/MANUAL_USER_TESTING_GUIDE.md` (4 roles, 2h)
- [ ] Security checklist signed

See `TEST_COVERAGE_GAP_REPORT.md`, `REGRESSION_PROTECTION_STATUS.md`.
