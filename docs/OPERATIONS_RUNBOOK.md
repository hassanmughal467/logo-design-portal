# Operations Runbook

## Daily operations

| Task | Action |
|------|--------|
| Health | Check `/health/ready` (automate via monitor) |
| Logs | Review Serilog errors; stdout rotation under `api\logs` |
| Hangfire | Open `/hangfire` (auth required) — failed jobs &lt; 5 |
| Disk | File storage volume &gt; 20% free |
| Redis | Memory usage &lt; 80% |

## Weekly

- Review slow-query warnings (investigate &gt; 2s queries)
- Confirm MySQL backup job succeeded
- Confirm file backup sync completed
- Review open security advisories (`npm audit`, `dotnet list package --vulnerable`)

## Monthly

- DR restore drill on staging
- Rotate review for service accounts
- Dependency patch window (non-breaking)

## Key endpoints

| Endpoint | Purpose |
|----------|---------|
| `GET /health/live` | Process alive |
| `GET /health/ready` | DB, Redis, Hangfire, files |
| `GET /health` | Full detail (restrict access) |
| `/hangfire` | Background jobs |

## Configuration changes

1. Document current IIS env vars
2. Apply change in staging first
3. Recycle app pool
4. Run `POST_DEPLOY_VALIDATION.md`

## Scaling actions

| Symptom | Action |
|---------|--------|
| High CPU on API | Add instance + Redis backplane (see `DEPLOYMENT_SCALING_PLAN.md`) |
| Slow reports | Read replica / cache warming |
| Upload disk full | Expand volume or archive old files |
| Redis memory | Increase maxmemory; review TTL |

## Support escalation

See `INCIDENT_RESPONSE_GUIDE.md`.

## Related

- `MONITORING_SETUP_GUIDE.md`
- `ALERTING_RECOMMENDATIONS.md`
- `docs/PRODUCTION-TROUBLESHOOTING.md`
