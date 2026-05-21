# Production Readiness Checklist

Use before every production release. All **Blockers** must be green.

---

## Configuration & secrets

- [ ] **Blocker** `Jwt__Key` set via environment (≥32 chars), not default appsettings value
- [ ] **Blocker** `ConnectionStrings__DefaultConnection` — real password, no placeholders
- [ ] **Blocker** `ConnectionStrings__Redis` configured; `Scalability:AllowInMemoryFallback` = false
- [ ] `Cors:AllowedOrigins` lists only production SPA origins
- [ ] `FileStorage__Path` on durable disk with IIS App Pool write ACL
- [ ] Email SMTP credentials in secrets (not committed)
- [ ] `ASPNETCORE_ENVIRONMENT=Production`

---

## Security

- [ ] Swagger disabled in Production (Development/Staging only)
- [ ] HTTPS termination + `UseForwardedHeaders` behind reverse proxy
- [ ] Security headers active (`SecurityHeadersMiddleware`)
- [ ] Rate limiting enabled (`DISABLE_RATE_LIMIT` not set)
- [ ] Hangfire dashboard restricted or disabled
- [ ] ProductionSafety flags reviewed (staging may disable billing)
- [ ] File download IDOR fix deployed (`IsVisibleToClient` on client downloads)

---

## Database

- [ ] Migrations tested on staging copy (`docs/PRODUCTION-MIGRATION-SAFETY.md`)
- [ ] Backup taken before migrate
- [ ] Rollback plan documented (forward-fix preferred)

---

## Observability

- [ ] Serilog sink configured (file/App Insights/etc.)
- [ ] `/health` and `/health/ready` monitored
- [ ] Alerts: 5xx rate, DB down, Redis down, disk space on file storage
- [ ] Correlation IDs visible in logs for support tickets

---

## Scalability

- [ ] Redis for SignalR + Hangfire + rate limit (multi-instance)
- [ ] File storage sized for peak upload volume (100MB/file, 500MB/request)
- [ ] MySQL connection pool sized for expected concurrency

---

## Frontend

- [ ] `environment.prod.ts` API URL correct
- [ ] Production build passes budgets (or budgets adjusted intentionally)
- [ ] E2E smoke on staging

---

## Operational

- [ ] Runbook: `docs/PRODUCTION-TROUBLESHOOTING.md`
- [ ] On-call knows kill-switch flags (`ProductionSafety`)
- [ ] Incident contacts + rollback command documented

---

## Rollback process

1. Stop IIS site / drain load balancer.
2. Deploy previous API build artifact.
3. **Do not** roll back DB unless migration is reversible and tested.
4. Re-enable traffic; verify `/health/ready`.
5. If JWT key was rotated, users must re-login.

---

## Sign-off

| Role | Name | Date |
|------|------|------|
| Engineering | | |
| QA | | |
| DevOps | | |
