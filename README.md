# Logo Design Portal (Hawk Merchandising Web Portal)

A B2B web portal for outsourced logo and merchandising design. Clients submit orders and quote requests, admins coordinate designers and billing, designers deliver previews and finals. Client billing runs in the client's currency (USD default); designer payouts run in PKR with a live USD→PKR exchange rate snapshot on invoices.

## Tech stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core on **.NET 8**, Clean Architecture (Domain → Application → Infrastructure → API) |
| ORM / DB | EF Core 8 + Pomelo MySQL provider → **MySQL 8** (SQLite/InMemory for tests) |
| Cache / infra | **Redis** (distributed cache, rate limiting, Hangfire storage, SignalR backplane) |
| Auth | JWT (HMAC-SHA256, key rotation) + HttpOnly cookies (`ldp_access`/`ldp_refresh`) + CSRF double-submit (`ldp_csrf` / `X-XSRF-TOKEN`) |
| Background jobs | Hangfire (Redis storage; memory storage in dev/testing) |
| Real-time | SignalR hub at `/hubs/notifications` (Redis backplane in staging/prod) |
| File storage | Local filesystem (dev) or **Cloudflare R2** via S3 API (staging/prod), selected by `Storage:Provider` |
| PDF | QuestPDF (invoice PDFs) |
| Logging | Serilog structured logging, correlation IDs; optional OpenTelemetry |
| Frontend | **Angular 17** + **PrimeNG 17**, RxJS state (no NgRx), ApexCharts/Chart.js |
| Tests | xUnit + Moq (backend), Karma/Jasmine (frontend unit), **Playwright** (E2E), k6 (load), Stryker (mutation) |
| Hosting | Windows **IIS** (`web.config`, AspNetCoreModuleV2, in-process) |

## Repository structure

```
Web Portal/
├── Backend/
│   ├── LogoDesignPortal.sln
│   ├── src/                                  # Active backend code (only place to edit)
│   │   ├── LogoDesignPortal.Domain/          # Entities, enums, OrderStatusStateMachine
│   │   ├── LogoDesignPortal.Application/     # Services, DTOs, interfaces, helpers
│   │   ├── LogoDesignPortal.Infrastructure/  # EF Core + migrations, JWT, Redis, storage, currency
│   │   ├── LogoDesignPortal.API/             # Controllers, middleware, SignalR hub, Program.cs
│   │   └── *.Tests / *.IntegrationTests      # xUnit test projects
│   └── scripts/                              # Manual SQL migration & seed scripts
├── Frontend/                                 # Angular 17 app
│   ├── src/app/                              # Feature modules, core/, shared/
│   ├── src/environments/                     # Per-environment config
│   └── e2e/                                  # Playwright E2E suite + config
├── ops/                                      # IIS deploy, MySQL backup/restore, smoke tests (PowerShell)
├── docs/                                     # Documentation (see index below)
├── load-tests/k6/                            # k6 load test scripts
├── scripts/run-pre-go-live-tests.ps1         # Tiered pre-release test runner
└── .github/workflows/                        # CI: PR validation, tests, E2E, release artifacts
```

Note: `Backend/LogoDesignPortal.*` folders directly under `Backend/` (outside `src/`) are legacy and must not be edited. Historical reports from earlier phases live in `docs/archive/`; current documentation lives in `docs/` (see `docs/README.md`) and `AGENTS.md`.

## Getting started (local development)

### Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- MySQL 8 (local) — or let the connection string point at SQLite for a lightweight run
- Redis 6+ (optional locally; Development allows in-memory fallback)

### Backend

```bash
cd Backend
dotnet restore
cd src/LogoDesignPortal.API
dotnet run          # Development: Swagger enabled, migrations auto-apply on startup
```

Configuration comes from `appsettings.json` + `appsettings.Development.json` + User Secrets / environment variables. Required secrets (never commit): `ConnectionStrings__DefaultConnection`, `Jwt__Key` (≥32 chars), and optionally `ConnectionStrings__Redis`, `ExchangeRate__ApiKey`, SMTP credentials, R2 keys. See `docs/deployment.md` for the full variable list.

In Development the `DatabaseInitializationHostedService` applies migrations and seeds roles, permissions, and a SuperAdmin user automatically (`Database:RunAfterStartup=true`). A repair mode exists: `dotnet run -- --repair-database`.

### Frontend

```bash
cd Frontend
npm ci
npm start           # ng serve on http://localhost:4200, API at https://localhost:44398
```

Dev environment (`src/environments/environment.development.ts`) uses cookie auth against `https://localhost:44398`.

### Running tests

```bash
# Backend (unit + integration; integration uses in-memory DB)
cd Backend && dotnet test LogoDesignPortal.sln

# Frontend unit
cd Frontend && npm run test:ci

# E2E (Playwright) — copy Frontend/e2e/.env.example to .env first
cd Frontend && npm run e2e          # full suite
cd Frontend && npm run e2e:smoke    # smoke only
```

See `docs/testing-plan.md` for the full strategy (security tests, load tests, mutation testing, pre-go-live tiers).

## Environments

| Environment | Angular build | Database | Redis | Migrations | File storage |
|---|---|---|---|---|---|
| Development | `npm start` | Local MySQL/SQLite | Optional | Automatic on startup | Local `Files_Dev` |
| Testing (CI) | `build:testing` / `e2e` | EF InMemory / SQLite | Skipped | `EnsureCreated` | Local `Files_Test` |
| Staging | `build:staging` | MySQL | Required | **Manual** | Cloudflare R2 |
| Production | `build:production` | MySQL | Required | **Manual** | Cloudflare R2 |

Staging carries `ProductionSafety` kill-switches (billing generation and designer payouts disabled) and test-mode payments; production does not. Production access tokens are shorter-lived (30 min vs 60). Swagger exists only in Development.

## CI/CD and deployment

GitHub Actions builds and tests (PR validation, unit/integration with ≥80% integration coverage gate, Playwright E2E against a real MySQL container) and, on `v*.*.*` tags, publishes API and frontend zip artifacts. **Deployment is manual to IIS** via `ops/deploy.ps1` (versioned releases, junction swap, smoke test, auto-rollback) and `ops/deploy-frontend.ps1`. See `docs/deployment.md`.

## Documentation index

| Document | Contents |
|---|---|
| [AGENTS.md](AGENTS.md) | Rules for engineers and AI agents working in this repo |
| [docs/architecture.md](docs/architecture.md) | Layers, middleware pipeline, auth, infrastructure |
| [docs/module-map.md](docs/module-map.md) | Backend + frontend module map |
| [docs/api.md](docs/api.md) | Full API route map with roles/permissions |
| [docs/database.md](docs/database.md) | Entities, tables, migrations, seeding |
| [docs/coding-rules.md](docs/coding-rules.md) | Coding standards (C#, Angular, general) |
| [docs/testing-plan.md](docs/testing-plan.md) | Testing strategy and commands |
| [docs/deployment.md](docs/deployment.md) | Deploy, rollback, environment variables |
| [docs/incident-response.md](docs/incident-response.md) | Severities, playbooks, recovery |
| [docs/production-readiness-checklist.md](docs/production-readiness-checklist.md) | Pre-launch gates |
