# Playwright E2E Testing Guide

## Layout (Week 2)

| Folder | Purpose |
|--------|---------|
| `Frontend/e2e/tests/auth/` | Login, logout, token, password reset, redirects |
| `Frontend/e2e/tests/workflows/` | Order lifecycle, assign, revisions |
| `Frontend/e2e/tests/invoices/` | Invoice create, mark-paid, duplicates |
| `Frontend/e2e/tests/uploads/` | Valid/invalid/oversized uploads |
| `Frontend/e2e/tests/permissions/` | Role restrictions, SuperAdmin bypass |
| `Frontend/e2e/tests/realtime/` | SignalR / health readiness |
| `Frontend/e2e/tests/security/` | RBAC, IDOR-style API, state machine |
| `Frontend/e2e/tests/smoke/` | Critical paths (CI gate) |
| `Frontend/e2e/tests/regression/` | Week 1 fix prevention |
| `Frontend/e2e/commands/` | Reusable auth/API commands |
| `Frontend/e2e/fixtures/` | Upload buffers and binary fixtures |
| `Frontend/e2e/factories/` | `test-data.factory.ts` composable builders |

## Running locally

```bash
cd Frontend/e2e
cp .env.example .env   # set E2E_API_URL, credentials
npx playwright test
npx playwright test tests/smoke   # smoke only
npx playwright test --project=mobile-chrome
```

API must be running at `E2E_API_URL` (default `http://localhost:5000`). UI tests also need Angular on `E2E_BASE_URL` (4200) unless using API-only specs.

## Projects

- **setup** — admin `storageState` (`.auth/admin.json`)
- **admin-chromium** — `roles/admin/*`
- **chromium** — default parallel suite
- **mobile-chrome** — smoke on Pixel 5 viewport

## Patterns

- **AAA**: arrange users via `provisionClientUser` / factories, act via API or POM, assert status + body.
- **Isolation**: unique emails per test (`uniqueSuffix(testInfo)`), `teardownUsers` in `afterAll`.
- **Determinism**: prefer API helpers in `utils/api-client.ts` for workflow stability; UI for smoke/login only.
- **Skip gracefully**: `test.skip` when backend/seed unavailable (document in CI env).

## CI recommendations

- Run `tests/smoke`, `tests/auth`, `tests/security/rbac-and-token` on every PR.
- Nightly: full `workflows`, `invoices`, `uploads`, `regression`.
- Set `PW_WORKERS=4`, `E2E_RETRIES=2` on shared runners.

See also `docs/TESTING_STANDARDS.md` and `docs/QA_TESTING_STRATEGY.md`.
