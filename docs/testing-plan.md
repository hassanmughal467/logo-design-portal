# Testing Plan

Current testing strategy, verified against the test projects, Playwright suite, and CI workflows. Companion docs: `docs/TESTING_STANDARDS.md`, `docs/QA-TESTING-GUIDELINE.md`, module guides `docs/TESTING_*_MODULE.md` / `docs/QA_*_MODULE.md`.

## Test pyramid

| Layer | Location | Framework | Count (approx.) |
|---|---|---|---|
| Domain unit | `Backend/src/LogoDesignPortal.Domain.Tests` | xUnit | 3 test classes (state machine, domain rules) |
| Application unit | `Backend/src/LogoDesignPortal.Application.Tests` | xUnit + Moq + EF InMemory | 45 test classes |
| API integration | `Backend/src/LogoDesignPortal.API.IntegrationTests` | xUnit + `WebApplicationFactory` | 38 test classes |
| Frontend unit | `Frontend/src/**/*.spec.ts` | Karma + Jasmine | per-component/service specs |
| E2E | `Frontend/e2e/tests/` | Playwright ^1.58 | 47 spec files |
| Load | `load-tests/k6/` | k6 | 5 scripts + SignalR soak notes |
| Mutation | Stryker | `npm run mutation` (frontend), `dotnet stryker` (backend, `Backend/stryker-config.json`) | on demand |

## Running tests

```bash
# All backend tests
cd Backend && dotnet test LogoDesignPortal.sln

# Single project
dotnet test Backend/src/LogoDesignPortal.Application.Tests

# Frontend unit (headless, CI parity)
cd Frontend && npm run test:ci

# E2E (copy Frontend/e2e/.env.example → .env first)
cd Frontend && npm run e2e            # full suite
npm run e2e:smoke                     # smoke (mobile-chrome project)
npm run e2e:elite                     # heavy serial flows
npm run e2e:ui                        # interactive
npm run e2e:report                    # open last HTML report

# Tiered pre-release orchestrator (from Frontend/ or root)
npm run test:portal:quick             # backend only
npm run test:portal:standard          # + Playwright smoke/security
npm run test:portal:full              # + Karma + full Playwright
npm run test:portal:staging           # staging health checks (STAGING_API_URL)

# Load tests (API must be running; API_BASE_URL env var)
k6 run load-tests/k6/auth-spike.js
```

## Integration test infrastructure

- `TestWebApplicationFactory` boots the real API with `ASPNETCORE_ENVIRONMENT=Testing`, swaps `ApplicationDbContext` for a uniquely-named EF InMemory database, and runs `TestDataSeeder` (seeded admin/client/designer users and profiles).
- `[Collection("Integration")]` shares one factory/host per collection (31 of 38 classes). Config-validation tests (appsettings, CORS, Redis options, Hangfire filters) run standalone.
- `IntegrationDatabaseHelper` seeds complex states (completed orders, invoices) directly when HTTP setup is too heavy.
- Testing environment: Hangfire server disabled, Redis skipped, rate limiting off, local `Files_Test` storage, relaxed cookies (`Secure=false`, `SameSite=Lax`).
- Prefer `IntegrationTestJson.Options` for deserialization; own `HttpClient` per auth context.

## What must always be covered ("never skip")

For any change in a critical area — auth, orders/state machine, invoices/payments, uploads, SignalR, role masking — include:

- **Authorization negatives**: no token (401), wrong role (403), other tenant's resource (IDOR → 403/404)
- **Invalid state transitions**: assert `OrderStatusStateMachine` rejections; no illegal jumps
- **Input negatives**: invalid payloads, empty payloads, not-found IDs, limits (file size, revision limits, attachment counts)
- **Duplicate actions**: double invoice creation, duplicate upload inside the 1-hour window, double mark-paid
- **Concurrency**: racing mutations on status updates, mark-paid, assignment (RowVersion conflicts)
- **Security specifics**: CSRF for cookie-authed mutations, upload abuse (bad extension/MIME/magic bytes), webhook handling

## E2E structure (`Frontend/e2e/tests/`)

| Category | Focus |
|---|---|
| `auth/`, `access/` | Login, session, token expiry, unauthorized access |
| `security/` | RBAC, token abuse, file download IDOR, order/file auth, upload security |
| `workflows/` | Order full lifecycle, designer assignment, payments flow |
| `smoke/` | Critical paths + staging health |
| `invoices/`, `billing/`, `permissions/`, `uploads/`, `realtime/` | API-level specs |
| `resilience/`, `uat/`, `regression/`, `visual/` | Failure handling, UX abuse, privacy masking, visual regression |
| `elite/` | Full business lifecycle, serial project |

Playwright config: `Frontend/e2e/playwright.config.ts` (projects: `setup`, `admin-chromium`, `chromium`, `mobile-chrome`, `elite-serial`; baseURL `http://localhost:4200`; auto-starts `ng serve --configuration=e2e` locally). Env vars via `Frontend/e2e/.env`: `E2E_BASE_URL`, `E2E_API_URL`, `E2E_ADMIN_EMAIL`, `E2E_ADMIN_PASSWORD`, `PW_WORKERS`, `E2E_RETRIES`, `E2E_REUSE_SERVER`.

## CI gates (GitHub Actions)

| Workflow | Trigger | Gates |
|---|---|---|
| `pr-validation.yml` | PRs to main/develop | `dotnet format --verify-no-changes`, Release build, Domain+Application unit tests, frontend typecheck, Karma, Angular prod build |
| `test.yml` | Push/PR to main/develop | Matrix (domain/application/integration) with coverage; **integration line coverage ≥ 80% enforced**; publish + prod build; Playwright smoke (3 specs) against Testing-env API |
| `e2e-playwright.yml` | Path-filtered push/PR, manual | Full Playwright suite against real MySQL 8 service container |
| `deploy-artifacts.yml` | `v*.*.*` tags, manual | Fast config/security integration test gate, then builds release zips |

Rules: do not skip tests in CI; do not lower the coverage gate; fix flaky tests at the root cause rather than weakening assertions.

## Test quality standards

- **AAA**: Arrange → Act → Assert, one behavior per test
- **Deterministic**: no time-of-day dependence; seed via factories/`TestWebApplicationFactory`
- **Isolated**: fresh in-memory DB per factory; per-auth-context HTTP clients
- **Production-grade assertions**: status codes and response bodies, not just "doesn't throw"
- Document known gaps in the module QA docs under `docs/`

## Load and stress

k6 scripts target auth spikes, concurrent orders/uploads, dashboard reads, invoice spikes (`API_BASE_URL`, default `http://localhost:5000`). SignalR soak (100 connections × 10 min) is documented in `load-tests/k6/signalr-soak.md` — k6 can't speak SignalR, so use Playwright or a SignalR.Client harness. Plans and baselines: `docs/LOAD_TESTING_PLAN.md`, `docs/PERFORMANCE_BASELINES.md`, `docs/STRESS_TESTING_GUIDE.md`.

## Known gaps / TODO

- Domain test coverage is thin (3 classes) relative to Application/Integration — grow alongside state-machine changes.
- No automated backend mutation-testing job in CI (`dotnet stryker` is manual).
- SignalR soak testing is documented but not automated.
- E2E full suite runs only on path-filtered triggers; smoke subset runs in `test.yml`.
