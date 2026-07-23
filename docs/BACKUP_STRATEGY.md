# Backup Strategy

## MySQL (primary data store)

| Parameter | Recommendation |
|-----------|----------------|
| Frequency | Daily full backup; hourly binlog if RPO &lt; 24h required |
| Retention | 30 days online; 90 days archive |
| Tool | `mysqldump --single-transaction` or managed provider snapshots |
| Encryption | At rest on backup volume |
| Test restore | Monthly to staging |

```bash
mysqldump -u backup_user -p --single-transaction LogoDesignPortalDb > backup_$(date +%Y%m%d).sql
```

## File uploads (`FileStorage:Path`)

| Parameter | Recommendation |
|-----------|----------------|
| Frequency | Daily incremental sync to secondary storage |
| Method | Robocopy / rsync / cloud sync |
| Coherence | Run after DB backup or during low traffic |
| RTO | 4h for full restore |

## Redis

| Content | Backup need |
|---------|-------------|
| Cache epochs | Regenerated — **no backup required** |
| Rate limit counters | Ephemeral |
| SignalR backplane | Ephemeral |
| Hangfire jobs | Recreated; failed jobs in MySQL if persisted |

**Redis:** Optional RDB snapshot for debugging only — not critical for DR.

## Configuration & secrets

- Export IIS environment variable list (encrypted store)
- Document JWT rotation procedure
- Version control `appsettings.Production.json` (no secrets)

## Hangfire / application logs

- Logs: rotate daily; ship to central store (30–90 day retention)
- Not required for business continuity

## Recovery objectives

| Tier | RPO | RTO |
|------|-----|-----|
| Database | 24h (daily) / 1h with binlog | 2–4h |
| Files | 24h | 4h |
| Full platform | 24h | 8h |

See `DISASTER_RECOVERY_PLAN.md`.
