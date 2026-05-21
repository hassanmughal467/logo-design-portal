# Testing Standards

Canonical agent rules also live in `.cursor/rules/testing.mdc`.

## Required test types (per feature change)

| Type | Purpose |
|------|---------|
| Unit | Pure logic, helpers, service branches (AAA) |
| Integration | API + DB, auth, permissions (`[Collection("Integration")]`) |
| Playwright E2E | Critical journeys, cross-role flows |
| Permission | 401/403 by role; never rely on UI-only guards |
| Security | IDOR, upload abuse, CSRF/cookies, webhooks |
| Negative | Invalid state, bad payloads, wrong tenant |
| Concurrency | Parallel mutations on same entity where relevant |

## Critical modules

Auth, orders, invoices/payments, uploads, SignalR, role masking, state machine transitions.

## Never skip

Unauthorized access, invalid state transitions, edge cases, retry scenarios (where applicable), duplicate actions (e.g. double mark-paid, duplicate upload window).

## Quality bar

Tests must be **deterministic**, **isolated**, and **production-grade** (assert HTTP status + meaningful body; use factories/`IntegrationDatabaseHelper`).

Module-specific checklists: `docs/QA_*_MODULE.md` and `docs/TESTING_*_MODULE.md`.

## Test Pyramid
- Unit tests are the default for pure logic and service behavior.
- Integration tests validate repository, API contracts, and persistence boundaries.
- E2E tests validate critical business journeys and cross-role workflows.
- Keep E2E focused on high-value paths; push edge permutations down to unit/integration.

## Coverage Rules
- Backend line coverage gate remains at 80%+.
- Frontend unit coverage gate remains enforced in CI.
- New code must not reduce baseline coverage in touched modules.

## Week 1 security test locations

| Area | Tests |
|------|-------|
| Upload abuse | `UploadSecurityHelperTests`, `FilesControllerUploadSecurityIntegrationTests` |
| Auth / JWT | `AuthSecurityIntegrationTests`, `AuthControllerTests` |
| Swagger exposure | `SwaggerExposureIntegrationTests` |
| Playwright smoke | CI runs `login`, `unauthorized`, `rbac-and-token` specs |

See `docs/WEEK1_SECURITY_HARDENING.md` and `docs/FILE_UPLOAD_SECURITY.md`.

## Week 2 QA expansion

| Guide | Topic |
|-------|--------|
| `docs/PLAYWRIGHT_TESTING_GUIDE.md` | E2E layout, projects, CI |
| `docs/SECURITY_TESTING_GUIDE.md` | Security automation matrix |
| `docs/OBSERVABILITY_SETUP.md` | Serilog, OTel, health, alerts |
| `docs/PERFORMANCE_TESTING_GUIDE.md` | k6, SLO, bottlenecks |
| `docs/REGRESSION_TESTING_STRATEGY.md` | Suite tiers, high-risk areas |
| `docs/LOAD_TESTING_PLAN.md` | Staged load rollout |

E2E folders: `auth/`, `workflows/`, `invoices/`, `uploads/`, `permissions/`, `realtime/`, `security/`, `smoke/`, `regression/`.

Health endpoints: `/health/live` (liveness), `/health/ready` (readiness).

Load scripts: `load-tests/k6/`.
- Coverage exceptions require explicit reviewer approval in PR.

## Naming Conventions
- Unit tests: `<unit>.<method-or-scenario>.spec`.
- Integration tests: `<feature>.integration.spec` or existing project naming standard.
- E2E tests: `<business-flow>.spec.ts` with descriptive scenario titles.
- Test names must include expected behavior, not implementation detail.

## Unit vs E2E Decision Rules
- Write unit tests when behavior is deterministic and isolated.
- Write integration tests when crossing DB, API, queue, or external boundaries.
- Write E2E tests when validating role transitions, business lifecycle, or UX resilience.
- Avoid duplicating the same assertion at every pyramid layer.

## Flaky Test Policy
- Flaky tests are production defects in the delivery pipeline.
- Any flaky test must be triaged immediately and fixed within 24 hours.
- Do not skip tests in CI (`test.skip`, ignore lists, or allow-failure patterns are prohibited).
- If a failure is non-deterministic, quarantine only with explicit QA owner and 24h SLA.

## E2E Reliability Rules
- Use API factories for deterministic setup and teardown.
- Use `browser.newContext()` for multi-user role isolation.
- Validate both UI and API state for critical lifecycle checkpoints.
- Add failure-mode scenarios (latency, transient 5xx, retry/recovery) for key workflows.

## Mutation Testing Baseline
- Backend mutation command: `dotnet stryker --config-file Backend/stryker-config.json`.
- Frontend mutation command: `npm run mutation --prefix Frontend`.
- Mutation score thresholds are enforced in config and should trend upward sprint-over-sprint.
