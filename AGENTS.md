# AGENTS.md — Hawk Merchandising Web Portal

Guidance for AI coding agents (Cursor, Codex, Claude, etc.) and new engineers working in this repository. Everything here is verified against the codebase; where something is unknown it is marked `TODO`.

## What this project is

A B2B portal for outsourced logo and merchandising design. Clients submit orders and quote requests, admins coordinate designers, and billing runs in client currencies (USD default) with designer payouts in PKR.

- **Backend:** ASP.NET Core (.NET 8), Clean Architecture, under `Backend/src/` only
- **Frontend:** Angular 17 + PrimeNG 17, under `Frontend/`
- **Database:** MySQL 8 (Pomelo EF Core provider); SQLite/InMemory for tests
- **Infra:** Redis (cache, rate limiting, Hangfire storage, SignalR backplane), Cloudflare R2 file storage (staging/prod), IIS hosting

Detailed docs live in `docs/` — start with [docs/architecture.md](docs/architecture.md) and [docs/module-map.md](docs/module-map.md).

## Repository layout (the parts that matter)

```
Web Portal/
├── Backend/
│   ├── LogoDesignPortal.sln
│   ├── src/                              # THE ONLY BACKEND CODE THAT COUNTS
│   │   ├── LogoDesignPortal.Domain/          # Entities, enums, OrderStatusStateMachine
│   │   ├── LogoDesignPortal.Application/     # Services, DTOs, interfaces, helpers
│   │   ├── LogoDesignPortal.Infrastructure/  # EF Core, migrations, JWT, Redis, storage, currency
│   │   ├── LogoDesignPortal.API/             # Controllers, middleware, hubs, Program.cs
│   │   ├── LogoDesignPortal.Domain.Tests/
│   │   ├── LogoDesignPortal.Application.Tests/
│   │   └── LogoDesignPortal.API.IntegrationTests/
│   ├── LogoDesignPortal.Domain|Application|Infrastructure|API/   # LEGACY — do not touch
│   └── scripts/                          # Manual SQL migration/seed scripts
├── Frontend/
│   ├── src/app/                          # Lazy-loaded feature modules + core/ + shared/
│   ├── src/environments/                 # Per-environment config
│   └── e2e/                              # Playwright tests + config
├── ops/                                  # IIS deploy, backup, smoke-test scripts (PowerShell)
├── docs/                                 # Documentation (this set + older reports)
├── load-tests/k6/                        # k6 load test scripts
├── scripts/run-pre-go-live-tests.ps1     # Tiered pre-release test orchestrator
└── .github/workflows/                    # CI (build/test/artifacts — no auto-deploy)
```

Historical reports from earlier phases are archived in `docs/archive/`; `api/`, `pages/`, `fixtures/`, `tests/`, `utils/` at the root are also artifacts of earlier phases. Prefer `docs/` (index at `docs/README.md`) for current documentation.

## Golden rules (never break)

1. **Clean Architecture boundaries.** Domain has zero project references. Application references Domain only. Infrastructure references Application + Domain. API references all. Never import Infrastructure into Domain/Application.
2. **`OrderStatusStateMachine` is authoritative.** Every order status change goes through `OrderStatusTransitionHelper.Apply()` which validates against `Backend/src/LogoDesignPortal.Domain/OrderStatusStateMachine.cs`. Never set `order.Status` directly.
3. **Server-side role masking.** Designer identity is never exposed to clients (they see "Design Team"); client PII is never exposed to designers. Masking happens in Application-layer DTO mapping — Angular guards are UX only.
4. **Backend re-validates all authorization.** `[Authorize]`, role checks, `[RequirePermission]`, and service-level access checks (e.g. `PaymentInvoiceAccessHelper`) are mandatory. Never trust frontend claims.
5. **No secrets in code or committed appsettings.** JWT keys, connection strings, R2 keys, SMTP creds come from environment variables or User Secrets. Flag any secret you find committed.
6. **Production migrations are manual.** `Database:RunAfterStartup` is `false` in Staging and Production. Never make migrations automatic in those environments, and never weaken this.
7. **PrimeNG only.** No Angular Material, no ng-bootstrap, no NgRx, no MediatR/CQRS.
8. **Controllers stay thin.** Parse request → call Application service → return response.
9. **Uploads are validated.** `UploadSecurityHelper` (extension, MIME, magic bytes, path traversal, duplicate window) on every upload path.
10. **Tests are required** for touched business logic — see [docs/testing-plan.md](docs/testing-plan.md).

## Environments

| Environment | ASPNETCORE_ENVIRONMENT | Angular build | Database | Redis | Migrations | Storage |
|---|---|---|---|---|---|---|
| Local dev | Development | `npm start` | Local MySQL (or SQLite) | Optional (in-memory fallback allowed) | Auto on startup | Local `Files_Dev` |
| CI / integration tests | Testing | `build:testing` / `e2e` | EF InMemory / SQLite | Skipped | `EnsureCreated` | Local `Files_Test` |
| Staging | Staging | `build:staging` | Staging MySQL | Required | **Manual** | Cloudflare R2 |
| Production | Production | `build:production` | Production MySQL | Required | **Manual** | Cloudflare R2 |

Staging/production differences that matter:

- Staging has `ProductionSafety` kill-switches enabled for billing generation and designer payout, and `Payments:UseTestMode=true`. Production has neither.
- Production JWT access tokens expire in 30 minutes vs 60 in staging.
- Swagger is Development-only (registered and mapped only when `IsDevelopment()`).

## Everyday commands

```bash
# Backend tests (all)
cd Backend && dotnet test LogoDesignPortal.sln

# Backend dev run
cd Backend/src/LogoDesignPortal.API && dotnet run

# Frontend dev
cd Frontend && npm start

# Frontend unit tests / E2E
cd Frontend && npm run test:ci
cd Frontend && npm run e2e          # or e2e:smoke, e2e:elite

# Add EF migration (never hand-write migrations)
cd Backend/src/LogoDesignPortal.Infrastructure
dotnet ef migrations add {Name} --startup-project ../LogoDesignPortal.API

# Format backend
cd Backend && dotnet format LogoDesignPortal.sln
```

Deployment is manual to IIS (`ops/deploy.ps1`, `ops/deploy-frontend.ps1`) — see [docs/deployment.md](docs/deployment.md).

## Where to find the details

| Topic | Document |
|---|---|
| Architecture, layers, middleware pipeline | [docs/architecture.md](docs/architecture.md) |
| Module-by-module map (backend + frontend) | [docs/module-map.md](docs/module-map.md) |
| Full API route map with roles/permissions | [docs/api.md](docs/api.md) |
| Entities, tables, migrations, seeding | [docs/database.md](docs/database.md) |
| Coding standards | [docs/coding-rules.md](docs/coding-rules.md) |
| Testing strategy and commands | [docs/testing-plan.md](docs/testing-plan.md) |
| Deploy/rollback runbooks | [docs/deployment.md](docs/deployment.md) |
| Incidents and recovery | [docs/incident-response.md](docs/incident-response.md) |
| Pre-launch gates | [docs/production-readiness-checklist.md](docs/production-readiness-checklist.md) |

The root `cursorrules` file contains an earlier version of these rules; this file and `docs/` are the maintained source going forward.

## AI Agent Rules

How Cursor, Codex, Claude, or any AI assistant must work safely in this repo:

### Scope and boundaries

- **Only edit backend code under `Backend/src/`.** `Backend/LogoDesignPortal.Domain`, `Backend/LogoDesignPortal.Application`, `Backend/LogoDesignPortal.Infrastructure`, `Backend/LogoDesignPortal.API` (no `src/`) are legacy copies — never modify or reference them.
- Respect layer direction. If a change requires Domain to know about EF Core, HTTP, or configuration, the design is wrong — move it up a layer.
- Do not create new root-level markdown reports; put documentation in `docs/`.

### Security — never weaken

- Never remove or relax `[Authorize]`, role lists, `[RequirePermission]`, CSRF validation, rate limiting, upload validation, or the Hangfire dashboard filters — even "temporarily" or "for testing". Use environment `Testing` behavior that already exists instead.
- Never log or echo secrets, JWTs, password hashes, or connection strings. Never write secrets into appsettings files, test fixtures, or docs.
- Never expose Swagger, detailed exceptions, or the Hangfire dashboard beyond their current environment/role gates.
- Preserve role masking in DTOs when adding fields: ask "can a client see designer data through this?" and vice versa before returning new properties.

### Database safety

- Schema changes go through `dotnet ef migrations add` in `LogoDesignPortal.Infrastructure` — never hand-edit migration files or the model snapshot, and never run raw DDL against staging/production.
- Never enable automatic migrations for Staging/Production (`Database:RunAfterStartup` must stay `false` there).
- Never write destructive data scripts (DELETE/TRUNCATE/DROP) without an explicit human request, a backup step, and a review.
- Soft delete is the norm (`IsDeleted` + global query filters). Do not hard-delete entities that have soft-delete filters unless the existing service already does so deliberately.

### Workflow integrity

- Order status transitions only via `OrderStatusStateMachine` / `OrderStatusTransitionHelper`. If a new transition is needed, change the state machine (and its tests in `Domain.Tests`) — do not bypass it.
- Billing invariants: invoicing requires `Status=Completed` + `BillingEligible=true` + `IsInvoiced=false`; paid invoices are immutable via `EditInvoiceItemsAsync`. Do not create alternate paths around these.
- Locked (terminal) orders reject file/comment/revision mutations via `OrderLockingHelper` — keep new mutation endpoints consistent with this.

### Testing discipline

- Any change to business logic needs unit tests (`Application.Tests` or `Domain.Tests`) and, for API-visible behavior, integration tests (`API.IntegrationTests`, `[Collection("Integration")]`).
- Security-sensitive changes (auth, uploads, invoices, role masking) additionally need negative tests: 401/403, wrong role, other tenant (IDOR), invalid state transitions.
- Never delete or weaken assertions to make a flaky test pass — fix the root cause. Never skip tests in CI.
- Run `dotnet format` and the relevant test projects before declaring work done; CI enforces `dotnet format --verify-no-changes` and ≥80% line coverage on integration tests.

### Operational safety

- Never trigger deployments, run `ops/deploy.ps1`, restore databases, or modify IIS/DNS/Redis from an agent session unless the human explicitly asks, step by step.
- Do not change GitHub Actions workflows to auto-deploy; artifact build → manual IIS deploy is intentional.
- Treat `ProductionSafety` kill-switch flags as operator-controlled — code may read them, agents must not flip them in committed config without instruction.

### When uncertain

- Mark unknowns as `TODO` in docs and code comments rather than inventing modules, endpoints, tables, or commands.
- Prefer reading the actual source (`Program.cs`, controllers, services, migrations) over trusting the historical reports in `docs/archive/`, several of which are outdated (e.g. the DigitalOcean guide references SQL Server; the real stack is MySQL).
