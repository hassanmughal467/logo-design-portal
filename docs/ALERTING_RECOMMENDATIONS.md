# Alerting Recommendations

## Critical (page on-call)

| Alert | Condition | Window |
|-------|-----------|--------|
| API down | `/health/ready` != 200 | 3 × 1 min |
| Database unhealthy | ready check `database` unhealthy | 2 min |
| 5xx spike | &gt; 5% of requests | 5 min |
| Auth outage | login 5xx &gt; 10% | 5 min |

## Warning (ticket next business day)

| Alert | Condition |
|-------|-----------|
| Redis unhealthy | ready check `redis` degraded |
| Hangfire failures | &gt; 10 failed jobs/hour |
| Disk low | file storage &lt; 15% free |
| Slow queries | &gt; 50 slow-query logs/hour |
| Certificate expiry | TLS &lt; 14 days |

## Info (dashboard only)

- Elevated 401 (possible scan)
- SignalR connection count drop (business hours)
- Invoice creation rate anomaly

## Alert routing

| Severity | Channel |
|----------|---------|
| Critical | Pager + SMS |
| Warning | Slack `#ops-alerts` |
| Info | Email digest |

## SLO proposals (post-launch baseline)

| SLO | Target |
|-----|--------|
| Availability | 99.5% monthly |
| Ready endpoint | 99.9% |
| p95 API latency | &lt; 800ms (excl. uploads) |

## False positive controls

- Maintenance window annotation
- Deploy silence window (30 min post-release)
- Ignore `/health` full detail endpoint errors from scanners

## Implementation checklist

- [ ] External uptime on `/health/ready`
- [ ] App Insights alert rules OR Grafana on OTel
- [ ] Log alert on `Fatal` / unhandled exception rate
- [ ] MySQL backup failure alert
- [ ] Disk space on file volume
