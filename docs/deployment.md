# Deployment

How this system is built, released, deployed, and rolled back. Verified against `.github/workflows/`, GitHub branch history, `ops/`, `deploy-to-staging-iis.ps1`, and `Backend/scripts/`. Companion runbooks: `ops/DEPLOYMENT_CHECKLIST.md`, `docs/DEPLOYMENT_WORKFLOW.md`, `docs/RELEASE_PROCESS.md`, `docs/ROLLBACK_GUIDE.md`, `docs/POST_DEPLOY_VALIDATION.md`.

## GitHub branch model

Repository: **https://github.com/hassanmughal467/logo-design-portal**

| Branch | Purpose | CI on push/PR | Deploy artifacts |
|---|---|---|---|
| **`develop`** | Active feature development | Full CI (`test.yml`, `pr-validation.yml`, path-filtered E2E) | None |
| **`staging-environment`** | Staging-ready code (`develop` + staging fixes) | Full CI **and** `deploy-staging.yml` on push | `LogoDesignPortal-*-Staging.zip` (14-day retention) |
| **`main`** | Intended production default (currently stale — only initial commit on remote until merged) | Same CI when PRs target `main` | Production zips via `v*.*.*` tags only |

Recommended flow:

```mermaid
flowchart LR
    Dev[develop: daily work] -->|merge when staging-ready| Staging[staging-environment]
    Staging -->|push triggers| StagingCI[deploy-staging.yml]
    StagingCI -->|download artifacts| StagingIIS[IIS staging-api / staging-admin]
    Staging -->|after validation| Main[main via PR]
    Main -->|git tag v*.*.*| ProdCI[deploy-artifacts.yml]
    ProdCI -->|download artifacts| ProdIIS[ops/deploy.ps1 → production IIS]
```

Open PR [#2](https://github.com/hassanmughal467/logo-design-portal/pull/2) merges `staging-environment` → `main` (full project history). Until `main` is updated, treat **`staging-environment` as the staging release branch** and **`develop` as the integration branch**.

## Deployment model

**CI builds and tests; deployment to IIS is manual.** GitHub Actions never SSHs to your servers — it produces zip artifacts (or you build on-server with PowerShell scripts).

```mermaid
flowchart TB
    subgraph ci [GitHub Actions]
        PR[PR / push] --> Gates[pr-validation + test + e2e]
        StagingPush[push staging-environment] --> StagingArt[deploy-staging.yml]
        Tag[tag v*.*.*] --> ProdArt[deploy-artifacts.yml]
    end
    subgraph manual [Manual IIS]
        StagingArt --> StagingZip[API-Staging.zip + Frontend-Staging.zip]
        ProdArt --> ProdZip[API.zip + Frontend.zip]
        OnServer[deploy-to-staging-iis.ps1] --> StagingIIS
        Ops[ops/deploy.ps1 + deploy-frontend.ps1] --> ProdIIS
    end
```

### Hosting

| Environment | API host | Frontend host | IIS paths (documented) |
|---|---|---|---|
| **Staging** | `staging-api.hawkmerchandising.com` | `staging-admin.hawkmerchandising.com` | `C:\inetpub\LogoDesignPortal-Staging\api` + `\wwwroot` (script default) |
| **Production** | `api.hawkmerchandising.com` | `admin.hawkmerchandising.com` | `C:\Sites\HawkPortal\` + `C:\Sites\HawkPortalFrontend\` (versioned junction deploy via `ops/deploy.ps1`) |

- **API**: AspNetCoreModuleV2 in-process (`Backend/src/LogoDesignPortal.API/web.config`).
- **Production versioned deploy**: `ops/deploy.ps1` keeps 3 releases under `C:\Sites\HawkPortal\releases\{version}\`, junction swap on `current`, smoke test, auto-rollback on failure.
- **Staging on-server build**: `deploy-to-staging-iis.ps1` at repo root (publish API + `npm run build:staging` + copy to IIS folders).
- Legacy `deploy-to-iis.ps1` → `C:\inetpub\LogoDesignPortal\` — prefer `ops/deploy.ps1` for production.
- `docs/archive/DIGITALOCEAN_STAGING_DEPLOYMENT_GUIDE.md` is outdated (SQL Server references) — do not use.

## CI workflows (`.github/workflows/`)

| Workflow | Trigger | Does | Deploys? |
|---|---|---|---|
| `pr-validation.yml` | PR → `main`, `develop`, **`staging-environment`** | Format verify, Release build, unit tests; frontend typecheck, Karma, prod build | No |
| `test.yml` | Push/PR → `main`, `develop`, **`staging-environment`** | Test matrix (integration ≥80% coverage gate), publish check, Playwright smoke | No |
| `e2e-playwright.yml` | Path-filtered push/PR + manual; branches include **`staging-environment`** | Full Playwright + MySQL 8 service | No |
| **`deploy-staging.yml`** | Push → **`staging-environment`**, manual | Staging integration test gate → publish API → `build:staging` → upload `LogoDesignPortal-*-Staging.zip` (14-day retention) | Artifacts only |
| `deploy-artifacts.yml` | Tags `v*.*.*`, manual | Production integration test gate → publish API → prod Angular build → upload production zips (30-day retention) | Artifacts only |

### CI requirements (must pass before merge/deploy)

- `dotnet format LogoDesignPortal.sln --verify-no-changes`
- Backend unit + integration tests; integration line coverage ≥ 80%
- Frontend `tsc -p tsconfig.app.json --noEmit` + Karma `test:ci`
- `npm ci` must succeed (peer deps: `apexcharts@^3.45.2` + `ng-apexcharts@~1.10.0`)

## Staging release procedure

1. Merge feature work into **`develop`**, then into **`staging-environment`** when ready for staging validation.
2. **Push** to `staging-environment` → `deploy-staging.yml` builds artifacts (or run on-server script below).
3. **Apply migrations manually** (`Database:RunAfterStartup=false`). Backup first; use `Backend/scripts/ApplyAllMigrations.sql` or `dotnet ef database update` against staging MySQL.
4. **Deploy to IIS** — choose one:
   - **From CI artifacts**: download `LogoDesignPortal-API-Staging.zip` + `LogoDesignPortal-Frontend-Staging.zip` from the Actions run → unzip to staging API/wwwroot sites; configure `web.config` from `web.config.staging.example.xml`; set `ASPNETCORE_ENVIRONMENT=Staging` and env vars (JWT, MySQL, Redis, R2).
   - **On-server build**: `.\deploy-to-staging-iis.ps1` from repo root on the staging Windows host.
5. **Validate**: `docs/STAGING_DEPLOYMENT_CHECKLIST.md` + `docs/POST_DEPLOY_VALIDATION.md` — `/health/live`, login, SignalR, upload smoke test.

## Production release procedure

1. **Validate on staging** first.
2. Merge `staging-environment` → `main` (when `main` is current).
3. **Tag**: `git tag v1.2.3 && git push --tags` → `deploy-artifacts.yml` builds production zips.
4. **Back up**: `.\ops\backup-mysql.ps1` and confirm R2 storage state.
5. **Apply migrations manually** during a maintenance window (same rules as staging).
6. **Deploy API**: place `api-{version}.zip` in `ops\artifacts\`, then `.\ops\deploy.ps1 -Version "1.2.3"` (smoke test + auto-rollback on failure).
7. **Deploy frontend**: `.\ops\deploy-frontend.ps1` with the frontend zip or dist.
8. Monitor Serilog/IIS logs for 30–60 minutes.

## Rollback

| Layer | Procedure |
|---|---|
| API (production) | `.\ops\deploy.ps1 -Rollback` (junction swap); automatic on smoke failure |
| API (staging) | Redeploy previous artifact zip or re-run `deploy-to-staging-iis.ps1` from prior commit |
| Frontend | Redeploy previous frontend zip via `deploy-frontend.ps1` or restage wwwroot |
| Database | Restore pre-migration backup (`ops/restore-mysql.ps1`) only if no meaningful writes since backup; otherwise forward-fix |
| Config only | Revert IIS environment variables, recycle app pool |
| Hangfire/Redis | Not reverted by code rollback — inspect `/hangfire` after rollback |

Decision window: ~15 minutes for critical production failures (`docs/ROLLBACK_GUIDE.md`).

## Configuration and secrets

Hierarchy (highest wins): environment variables → `appsettings.{Environment}.json` → `appsettings.json` → User Secrets (Development only).

| Variable | Purpose |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Staging` / `Production` |
| `ConnectionStrings__DefaultConnection` | MySQL connection string |
| `ConnectionStrings__Redis` | Redis (required staging/prod) |
| `Jwt__Key` | Signing key, ≥32 chars |
| `Jwt__PreviousKey`, `Jwt__KeyVersion` | Key rotation window |
| `Storage__R2__*` | Cloudflare R2 credentials |
| `ExchangeRate__ApiKey` | USD→PKR rate API (optional fallback) |
| `Email__*` | SMTP credentials |

See `docs/ENVIRONMENT_VARIABLE_REFERENCE.md` and `docs/SECRET_MANAGEMENT_GUIDE.md`. **Never commit secrets** — configure via IIS environment variables or host-level secret stores.

## Staging vs production differences

| Setting | Staging | Production |
|---|---|---|
| Angular build | `npm run build:staging` | `npm run build:production` |
| `Database:RunAfterStartup` | false (manual migrations) | false (manual migrations) |
| Storage | R2 | R2 |
| JWT access token | 60 min | **30 min** |
| Payments | `UseTestMode=true` | `UseTestMode=false` |
| `ProductionSafety` kill-switches | Billing + designer payout disabled | none configured |
| Redis | Required | Required |

## Production safety rules

- **Never enable `Database:RunAfterStartup` in staging/production.**
- Swagger Development-only; no stack traces to clients.
- `ProductionSafety` kill-switches can freeze billing, payouts, uploads, or invoice edits during incidents — config change + app pool recycle.
- Keep 3 versioned production releases on disk for instant rollback.
- Nightly MySQL backup via `ops/backup-mysql.ps1` (Task Scheduler 02:00).

## TODO / unknowns

- Exact IIS server inventory and TLS certificate renewal process. TODO.
- Production log aggregation destination. TODO.
- Incident on-call contacts — fill `docs/incident-response.md` table. TODO.
