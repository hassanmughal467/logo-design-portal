# Production Readiness Checklist

Use before every production release. All **Blockers** must be green. Companion docs: [deployment.md](deployment.md), [incident-response.md](incident-response.md), `docs/PRODUCTION-MIGRATION-SAFETY.md`, `docs/POST_DEPLOY_VALIDATION.md`, `ops/DEPLOYMENT_CHECKLIST.md`.

---

## Pre-release gates (CI)

- [ ] **Blocker** `test.yml` green on the release commit (unit + integration tests, integration line coverage ≥ 80%)
- [ ] **Blocker** `deploy-artifacts.yml` produced `LogoDesignPortal-API.zip` + `LogoDesignPortal-Frontend.zip` from the release tag (`v*.*.*`)
- [ ] Playwright E2E (`e2e-playwright.yml` or `npm run test:portal:standard`) green for changed areas
- [ ] Release validated on staging first (`docs/STAGING_DEPLOYMENT_CHECKLIST.md`)

---

## Configuration & secrets

- [ ] **Blocker** `Jwt__Key` set via environment (≥32 chars), not default appsettings value
- [ ] **Blocker** `ConnectionStrings__DefaultConnection` — real password, no placeholders
- [ ] **Blocker** `ConnectionStrings__Redis` configured; `Scalability:AllowInMemoryFallback` = false
- [ ] **Blocker** `Database:RunAfterStartup` = false (production never auto-migrates)
- [ ] `Cors:AllowedOrigins` lists only production SPA origins (`https://admin.hawkmerchandising.com`)
- [ ] `Storage:Provider` = `R2` with `Storage__R2__*` credentials set via environment
- [ ] If local file storage is used anywhere: path on durable disk with IIS App Pool write ACL
- [ ] Email SMTP credentials in secrets (not committed)
- [ ] `ExchangeRate__ApiKey` set (or accept fallback-rate behavior consciously)
- [ ] `Payments:UseTestMode` = false in production (true in staging)
- [ ] `ASPNETCORE_ENVIRONMENT=Production`
- [ ] No secrets present in any committed appsettings/web.config

---

## Security

- [ ] Swagger disabled in Production (Development only)
- [ ] HTTPS termination + `UseForwardedHeaders` behind reverse proxy
- [ ] Security headers active (`SecurityHeadersMiddleware`)
- [ ] Rate limiting enabled (`DISABLE_RATE_LIMIT` not set)
- [ ] Hangfire dashboard restricted (Admin read-only / SuperAdmin) — verify `/hangfire` returns 403 for non-admins
- [ ] CSRF validation active for cookie-authenticated mutations (`CsrfValidationMiddleware`)
- [ ] ProductionSafety flags reviewed (staging disables billing/payout; production intentionally has none — confirm this is still desired)
- [ ] File download IDOR fix deployed (`IsVisibleToClient` on client downloads)
- [ ] JWT key rotation state clean (`Jwt__PreviousKey` removed after rotation window)
- [ ] `/health` full-detail endpoint restricted at infrastructure level (only `/health/ready`, `/health/live`, `/api/system/health` public)

---

## Database

- [ ] **Blocker** Migrations tested on staging copy (`docs/PRODUCTION-MIGRATION-SAFETY.md`)
- [ ] **Blocker** Backup taken immediately before migrate (`ops/backup-mysql.ps1`) and verified restorable
- [ ] Migration SQL reviewed (prefer `Backend/scripts/ApplyAllMigrations.sql`, additive-first)
- [ ] Rollback plan documented (forward-fix preferred once data is written)
- [ ] Nightly backup schedule confirmed running (Task Scheduler 02:00 → R2 `hawk-backups/daily/`)
- [ ] Last monthly restore drill within 30 days (`ops/BACKUP_RUNBOOK.md`)

---

## Observability

- [ ] Serilog sink configured (file/App Insights/etc.)
- [ ] `/health` and `/health/ready` monitored externally (1–5 min interval)
- [ ] Alerts: 5xx rate, DB down, Redis down, disk space on file storage, Hangfire failed jobs
- [ ] Correlation IDs visible in logs for support tickets
- [ ] OpenTelemetry (`Observability:Enabled`) state deliberate — on with an OTLP endpoint, or off

---

## Scalability

- [ ] Redis for SignalR + Hangfire + rate limit (multi-instance)
- [ ] File storage sized for peak upload volume (100MB/file, 500MB/request)
- [ ] MySQL connection pool sized for expected concurrency
- [ ] Load-test baselines reviewed if traffic profile changed (`load-tests/k6/`, `docs/PERFORMANCE_BASELINES.md`)

---

## Frontend

- [ ] `environment.production.ts` API URL correct (`https://api.hawkmerchandising.com`)
- [ ] Built with `npm run build:production` (env replacement + hashing + budgets)
- [ ] Production build passes budgets (initial 500kb warn / 1mb error) or budgets adjusted intentionally
- [ ] E2E smoke on staging (`npm run e2e:smoke` / `test:portal:staging`)

---

## Operational

- [ ] Runbooks current: [incident-response.md](incident-response.md), `docs/PRODUCTION-TROUBLESHOOTING.md`
- [ ] On-call knows kill-switch flags (`ProductionSafety`) and how to apply them (config + app pool recycle)
- [ ] Incident contacts filled in (`docs/INCIDENT_RESPONSE_GUIDE.md` contact table — currently TODO)
- [ ] Rollback command verified: `.\ops\deploy.ps1 -Rollback`; previous 2 releases still on disk
- [ ] Post-deploy validation plan ready (`docs/POST_DEPLOY_VALIDATION.md`) with 30–60 min monitoring window

---

## Rollback process

1. Stop IIS site / drain load balancer (or rely on `deploy.ps1` junction swap).
2. `.\ops\deploy.ps1 -Rollback` (or deploy previous API build artifact manually).
3. **Do not** roll back DB unless migration is reversible and tested — forward-fix preferred.
4. Re-enable traffic; verify `/health/ready` and `/api/system/health`.
5. If JWT key was rotated, users must re-login.
6. Check `/hangfire` for jobs referencing rolled-back behavior.

---

## Sign-off

| Role | Name | Date |
|------|------|------|
| Engineering | | |
| QA | | |
| DevOps | | |
