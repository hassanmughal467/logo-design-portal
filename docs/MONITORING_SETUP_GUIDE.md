# Monitoring Setup Guide

## Built-in observability (API)

| Feature | Config | Default |
|---------|--------|---------|
| Serilog structured JSON | `appsettings.json` | Console |
| Correlation ID | `CorrelationIdMiddleware` | On |
| Request logging | UserId, role, route ids | On |
| Health checks | `/health`, `/health/ready`, `/health/live` | On |
| OpenTelemetry | `Observability:Enabled` | **false** |

## Enable Application Insights / OTel

1. Set in Production environment:
   ```json
   "Observability": {
     "Enabled": true,
     "Exporter": "ApplicationInsights",
     "ApplicationInsights": {
       "ConnectionString": "<from-secret>"
     }
   }
   ```
2. Verify traces in portal after deploy
3. Wire dependency tracking for MySQL + Redis + HTTP outbound

Reference: `docs/OBSERVABILITY_SETUP.md`, `Hosting/ObservabilityServiceRegistration.cs`

## Health check monitoring

External monitor (UptimeRobot, Pingdom, Azure Monitor):

- URL: `https://api.<domain>/health/ready`
- Interval: 1–5 min
- Alert if non-200 for 3 consecutive checks

**Restrict** public detail on `/health` — use ready/live only externally.

## Log aggregation

| Option | Setup |
|--------|-------|
| File + Logtail/Datadog agent | Ship `api\logs\*.log` |
| Windows Event Log | Optional wrapper |
| IIS stdout | Already in `web.config` — rotate daily |

## Metrics to track

| Metric | Source |
|--------|--------|
| Request rate / 5xx | Reverse proxy or App Insights |
| p95 latency | App Insights / OTel |
| DB connection pool | Custom health / MySQL metrics |
| Hangfire failed jobs | Dashboard API or SQL |
| Redis memory | `INFO memory` |
| Disk free (Files path) | `file_storage` health check |

## Dashboards (recommended panels)

1. API availability (ready probe)
2. Error rate (5xx / exceptions)
3. Auth failures (401/403 spike)
4. Order creation rate
5. Upload volume / failures

## Frontend monitoring (optional)

- Real User Monitoring (RUM) after Angular upgrade
- Playwright synthetic checks every 15 min from CI (future)

See `ALERTING_RECOMMENDATIONS.md`.
