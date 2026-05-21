# Rollback Guide

## When to rollback

- Health checks failing after deploy
- Error rate spike / 5xx sustained
- Failed migration or data corruption risk
- Auth or payment regression confirmed

**Decision within 15 minutes** of production deploy for critical paths.

---

## API rollback (IIS)

### Prerequisites

- Previous `LogoDesignPortal-API.zip` retained (from `deploy-artifacts.yml` or local publish)
- DB restore point if migration ran

### Steps

1. Stop IIS App Pool for API site
2. Rename current folder → `api_failed_YYYYMMDD`
3. Unzip previous API artifact to site root
4. Restore previous `web.config` if env vars changed
5. **If migration ran:** restore MySQL backup OR run down migration (only if tested)
6. Start App Pool
7. Verify `/health/live` and `/health/ready`
8. Smoke login + one order read

---

## Frontend rollback

1. Stop admin site pool (optional)
2. Replace `wwwroot` with previous `LogoDesignPortal-Frontend.zip`
3. Clear CDN/browser cache if applicable
4. Verify SPA loads and API URL matches backend

---

## Database rollback

| Scenario | Action |
|----------|--------|
| Migration failed mid-flight | Restore pre-migrate backup |
| Migration succeeded, app bad | Restore backup OR hotfix forward (prefer forward if data written) |
| Staging only | Restore staging DB snapshot |

See `BACKUP_STRATEGY.md`, `PRODUCTION-MIGRATION-SAFETY.md`.

---

## Redis / Hangfire

- Rolling back API does not revert Redis job state
- Stuck jobs: inspect `/hangfire` after rollback
- SignalR: clients reconnect automatically

---

## Configuration rollback

If bad env var caused failure:

1. Revert IIS environment variables to last known good
2. Recycle App Pool
3. Do **not** redeploy code if only config was wrong

---

## Post-rollback

- [ ] Incident documented
- [ ] Root cause assigned
- [ ] Fix forward planned on branch
- [ ] Staging re-validated before retry

---

## Prevention

- Always keep **N-1** artifacts
- Test migrations on staging first
- Use `Database:RunAfterStartup: false` in deployed environments
