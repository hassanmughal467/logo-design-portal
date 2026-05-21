# Disaster Recovery Plan

## Scenarios

| Scenario | Impact | Recovery |
|----------|--------|----------|
| API server failure | Portal down | Deploy to standby IIS / restore VM |
| MySQL corruption/loss | All data at risk | Restore latest `mysqldump` + binlogs |
| File storage loss | Uploads missing | Restore from file backup sync |
| Redis down | Rate limits, SignalR, Hangfire affected | Restore Redis VM; API may fail start in prod |
| Region/provider outage | Full outage | Secondary region (future); manual comms |
| Bad deploy | Errors, partial migration | `ROLLBACK_PROCEDURES.md` |

## Recovery order

1. Restore **MySQL** to last known good backup
2. Restore **file storage** to matching date (minimize orphan files)
3. Deploy **known-good API + frontend** artifacts
4. Start **Redis**
5. Run `POST_DEPLOY_VALIDATION.md`
6. Verify Hangfire recurring jobs

## Redis failure (production)

- App requires Redis when `AllowInMemoryFallback: false`
- **Mitigation:** HA Redis (sentinel/cluster) or managed Redis with SLA
- Temporary: single-node restore from snapshot (jobs may be lost)

## Environment recovery

| Component | Source |
|-----------|--------|
| API binaries | Last release tag artifact / publish backup |
| Frontend | `dist` backup paired with API tag |
| Secrets | Secret manager export |
| IIS config | Documented in `PRODUCTION_CONFIGURATION_GUIDE.md` |

## Communication

- Status page / email to clients for RTO &gt; 1h
- Internal Slack/email per `INCIDENT_RESPONSE_GUIDE.md`

## DR drill (quarterly)

1. Restore DB backup to staging MySQL
2. Restore file backup to staging path
3. Point staging API at restored data
4. Run integration test subset
5. Record actual RTO/RPO

## Gaps (Week 4)

- No automated cross-region failover
- No Docker/K8s redeploy path
- Manual IIS recovery only

**Target maturity:** Month 3 — automated backup verification job; Month 6 — warm standby API.
