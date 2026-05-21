# Deployment Checklist (Week 4 — Updated)

**Status:** Ready for controlled production launch with operator sign-off.  
**Canonical API path:** `Backend/src/LogoDesignPortal.API`

---

## Pre-deploy (blocking)

- [ ] Confirm deploy target is **`Backend/src/LogoDesignPortal.API`** (not legacy `Backend/LogoDesignPortal.API`)
- [ ] MySQL backup completed (`mysqldump`) — see `docs/PRODUCTION-MIGRATION-SAFETY.md`
- [ ] EF migration reviewed; SQL script generated if not using auto-migrate
- [ ] `Jwt__Key` set (≥32 chars, not placeholder) — app will not start otherwise
- [ ] `ConnectionStrings__DefaultConnection` set (not `REPLACE_IN_*`)
- [ ] `ConnectionStrings__Redis` set (**required** in Production)
- [ ] `IncludeExceptionDetailsInProduction` = false
- [ ] `Database:RunAfterStartup` = false (default in Production JSON)
- [ ] `Scalability:AllowInMemoryFallback` = false
- [ ] CORS trimmed to HTTPS admin origin only
- [ ] `AllowedHosts` set to production hostnames (recommended)
- [ ] `Frontend` production build (`npm run build:production`) — `environment.production.ts` `apiUrl` matches live API
- [ ] `Email:FrontendUrl` matches admin SPA URL
- [ ] Security checklist: `SECURITY_VERIFICATION_CHECKLIST.md`

## Build & publish

- [ ] `dotnet publish Backend/src/LogoDesignPortal.API -c Release -o <publishDir>`
- [ ] `cd Frontend && npm ci && ng build --configuration production`
- [ ] Copy `Frontend/dist/*` to IIS wwwroot
- [ ] Copy `Frontend/web.config` to wwwroot
- [ ] IIS: application pool **No Managed Code**, integrated pipeline
- [ ] Create `api\logs`, `api\Files` (production upload root) with app pool write ACLs
- [ ] `dotnet test` config validation: `--filter "EnvironmentAppSettings|ProductionConfiguration"`

## IIS environment variables (examples)

- `ConnectionStrings__DefaultConnection`
- `ConnectionStrings__Redis`
- `Jwt__Key`
- `Email__SmtpPassword` (if applicable)

## Deploy execution

- [ ] Stop or drain traffic (optional blue/green)
- [ ] Deploy API binaries + `web.config`
- [ ] Apply database migration (manual/scripted)
- [ ] Deploy frontend static files
- [ ] Recycle application pool

## Post-deploy (do not use Swagger in Production)

- [ ] `GET /health/live` → healthy
- [ ] `GET /health/ready` → DB, Hangfire, file_storage healthy
- [ ] Login + cookie/CSRF from production admin origin
- [ ] Create order smoke (admin) — optional
- [ ] SignalR notification smoke
- [ ] **Change SuperAdmin default password** if first deploy
- [ ] Monitor logs 24–48h — no stack traces in API responses

## Staging (before production)

- [ ] Complete `STAGING_DEPLOYMENT_CHECKLIST.md`
- [ ] Staging uses `Files_Staging`, unique `Jwt__Key`, Redis instance/index isolated from production

## Rollback readiness

- [ ] Previous publish folder retained
- [ ] DB rollback script or restore point documented — `ROLLBACK_PROCEDURES.md`
- [ ] Rollback decision owner identified

## CI gates (pre-merge to release branch)

- [ ] `pr-validation.yml` green
- [ ] `test.yml` green (unit + integration ≥80% line coverage)
- [ ] E2E smoke or full `e2e-playwright.yml` on release candidate

## Sign-off

| Role | Date | Signature |
|------|------|-----------|
| Engineering Lead | | |
| Operations | | |
