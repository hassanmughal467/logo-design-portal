# Incident Response

Consolidated process for production incidents. Companion runbooks (kept, still valid): `docs/INCIDENT_RESPONSE_GUIDE.md`, `docs/FAILURE_RECOVERY_GUIDE.md`, `docs/OPERATIONS_RUNBOOK.md`, `docs/PRODUCTION-TROUBLESHOOTING.md`, `ops/redis-restart.md`, `ops/BACKUP_RUNBOOK.md`, `docs/DISASTER_RECOVERY_PLAN.md`.

## Severity levels and response targets

| Severity | Examples | Response time |
|---|---|---|
| SEV1 | Production down, data breach, data loss | 15 minutes |
| SEV2 | Auth broken, payments failing, uploads failing platform-wide | 1 hour |
| SEV3 | Degraded performance, a feature broken for a subset of users | 4 hours |
| SEV4 | Minor bug, cosmetic issue | Next sprint |

## First 15 minutes (any SEV1/SEV2)

1. **Acknowledge** — take ownership, note the start time.
2. **Assess** — in order:
   - `GET https://api.hawkmerchandising.com/api/system/health` (anonymous: database/signalr/storage)
   - `GET /health/ready` (full readiness: DB schema, Hangfire, storage, disk, SMTP, memory, SignalR; Redis failure shows Degraded)
   - Serilog output / IIS stdout logs (`.\logs\stdout`), filtered by correlation ID if a specific request is failing
   - "Did we just deploy?" — check the `current` junction target and recent release folders
3. **Mitigate** — pick the least destructive option that stops the bleeding:
   - Recent deploy suspected → `.\ops\deploy.ps1 -Rollback`
   - Risky subsystem misbehaving → flip the relevant `ProductionSafety` kill-switch (`DisableBillingGeneration`, `DisableDesignerPayout`, `DisableFileUploads`, `DisableInvoiceEditing`) and recycle the app pool
   - Redis down → API stays up degraded; restart Redis per `ops/redis-restart.md`; only set `Scalability__EmergencyFallbackEnabled=true` for single-instance emergencies
4. **Communicate** — notify stakeholders with impact, ETA, and next update time.
5. **Preserve evidence** — export relevant logs before they rotate; don't restart services that hold the only evidence unless needed for mitigation.

## Common failure playbooks

| Symptom | Likely cause | Action |
|---|---|---|
| API 500s after deploy | Pending migrations not applied (`RunAfterStartup=false`) | Apply `Backend/scripts/ApplyAllMigrations.sql` (backup first) or roll back the deploy; see `docs/PRODUCTION-TROUBLESHOOTING.md` |
| Logins failing platform-wide | JWT key/env var missing or rotated wrong | Verify `Jwt__Key` (≥32 chars) and `Jwt__PreviousKey`; startup validates these — check stdout logs |
| CORS errors from the admin SPA | `Cors:AllowedOrigins` mismatch after host change | Verify origins in environment config; recycle app pool |
| Redis outage | Service down | `/health/ready` shows Degraded but API serves; restart Redis (`ops/redis-restart.md`); rate limiting and Hangfire recover on reconnect |
| Hangfire jobs stuck/failed | Redis blip, SMTP down, job exception | Inspect `/hangfire` (SuperAdmin can requeue; Admin is read-only); >5 failed jobs is the daily-ops alarm threshold |
| Uploads failing | Storage (R2 creds/limits), kill switch, or order lock rules | Check `file_storage` health check, R2 credentials, `ProductionSafety:DisableFileUploads` |
| Orphaned temp files accumulating | Cleanup job not running | Verify `OrphanFileCleanupService` recurring job in `/hangfire` |
| SignalR not delivering | Backplane/Redis or auth | Frontend auto-falls back to 60 s polling; check `signalr` health check and Redis |
| Emails not sending | SMTP creds/network | `smtp` health check; jobs retry via Hangfire |
| DB slow / timeouts | Slow queries, missing index | `SlowQueryLoggingInterceptor` output; `docs/QUERY_OPTIMIZATION_REPORT.md`, `docs/INDEX_RECOMMENDATIONS.md` |

## Security incidents

- **Suspected token compromise**: rotate JWT — set new `Jwt__Key`, move the old one to `Jwt__PreviousKey` for the overlap window, then remove it; production access tokens expire in 30 minutes. Invalidate refresh tokens by clearing `RefreshToken` columns for affected users.
- **Account abuse**: lockout is automatic (5 failures → 30 min); auth endpoints are rate-limited to 5/min.
- **Data exposure**: check `AuditLogs`, `OrderLogs`, and `InvoiceLogs` tables plus Serilog to scope access; preserve evidence before remediation.
- Never disable CSRF, rate limiting, or authorization as a mitigation step.

## Data incidents

1. Stop writes if corruption is spreading (kill switches, or stop the IIS site as a last resort).
2. Assess whether restore or forward-fix is safer: restore (`ops/restore-mysql.ps1`, backups in R2 `hawk-backups/daily/`) loses everything after the backup; forward-fix is preferred once real data has been written.
3. Verify after restore: script checks row counts on `LogoOrders`, `Invoices`, `Users`.
4. Reconcile Hangfire state (Redis) with the restored DB — jobs may reference entities that no longer exist.

## Routine operations (prevention)

From `docs/OPERATIONS_RUNBOOK.md`:

- **Daily**: `/health/ready` green, Serilog error scan, Hangfire failed jobs < 5, disk > 20% free, Redis memory < 80%
- **Weekly**: slow-query review, backup success verification, `npm audit`/dependency vulnerabilities
- **Monthly**: disaster-recovery restore drill, service account/credential rotation review, dependency patching

## Post-incident

- Export relevant Serilog output and record the cache epoch/Hangfire state.
- Re-run the integration suite against a staging clone before declaring resolution for data-affecting incidents.
- Write a blameless post-mortem: timeline, root cause, mitigation, and follow-up actions (add missing tests, alerts, or runbook entries).

## Contacts and escalation

Fill this table in and keep it current — it is the single most important part of the incident process. Values are intentionally blank in version control (TODO for the team); consider mirroring it somewhere reachable when the portal itself is down.

| Role | Name | Contact (phone / chat) | Escalate after |
|---|---|---|---|
| Primary on-call engineer | TODO | TODO | — |
| Secondary on-call engineer | TODO | TODO | 15 min unacknowledged |
| Database owner (MySQL/backups) | TODO | TODO | SEV1 data incidents |
| Infrastructure owner (IIS/Redis/R2/DNS) | TODO | TODO | SEV1 infra incidents |
| Security owner | TODO | TODO | Any suspected breach — immediately |
| Product owner / client communication | TODO | TODO | Any SEV1/SEV2 > 30 min |

Escalation path: on-call engineer → secondary → area owner → product owner. For suspected breaches, involve the security owner immediately regardless of severity.

TODO: no alerting/paging integration is recorded in the repo (external monitor on `/health/ready` is recommended in `docs/MONITORING_SETUP_GUIDE.md` but the chosen provider isn't documented). Record the provider and alert routing here once chosen.
