# Staging Deployment Checklist

Use this checklist for every staging release. Do not skip security or isolation steps.

---

## Pre-deploy

- [ ] Release branch merged; CI green on `main`/`develop`
- [ ] `dotnet test` integration filter `EnvironmentAppSettings|ProductionConfiguration` passed
- [ ] Staging DB backup taken (if upgrading existing staging)
- [ ] Redis instance/index confirmed **not shared** with production
- [ ] `Jwt__Key` unique to staging (rotated if compromised)
- [ ] MySQL credentials in secret store / IIS env only
- [ ] `Files_Staging` directory empty or migrated intentionally
- [ ] DNS + TLS certificates valid for staging hostnames
- [ ] `ProductionSafety` flags reviewed for UAT scope
- [ ] Payment provider in **sandbox/test** mode only

---

## Deploy API

- [ ] `ASPNETCORE_ENVIRONMENT=Staging` in `web.config`
- [ ] `ConnectionStrings__DefaultConnection` set
- [ ] `ConnectionStrings__Redis` set
- [ ] `Jwt__Key` set (not placeholder)
- [ ] App Pool identity has write access to `Files_Staging` and `logs`
- [ ] WebSockets enabled on IIS site
- [ ] `dotnet ef database update` (or approved SQL) executed
- [ ] Application pool started; no 500.30 in stdout logs

---

## Deploy Frontend

- [ ] `npm run build:staging` used (not production config)
- [ ] Artifact deployed to staging admin site
- [ ] `web.config` SPA rewrite present
- [ ] Browser hits `https://staging-admin...` and API CORS allows origin

---

## Post-deploy validation

- [ ] `GET /health/live` → healthy
- [ ] `GET /health/ready` → healthy (DB + Redis)
- [ ] Login (cookie auth + CSRF) works
- [ ] SignalR connects (`/hubs/notifications`)
- [ ] Upload test file (lands under `Files_Staging`)
- [ ] Hangfire dashboard accessible to Admin only (`/hangfire`)
- [ ] Billing generation **blocked** (kill switch)
- [ ] Rate limiting active (Redis-backed)
- [ ] No stack traces in API error responses
- [ ] Playwright staging smoke (if `STAGING_API_URL` set)

---

## Rollback

- [ ] Previous API + frontend zip retained
- [ ] DB restore point documented
- [ ] See `ROLLBACK_GUIDE.md`

---

## Sign-off

| Role | Name | Date |
|------|------|------|
| Deployer | | |
| QA | | |
| Owner | | |
