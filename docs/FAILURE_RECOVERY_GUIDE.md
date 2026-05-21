# Failure Recovery Guide (Week 3)

## Failure modes

| Component | Symptom | Recovery |
|-----------|---------|----------|
| MySQL transient | 500, retry succeeds | EF retry policy (enabled); client retry |
| Redis outage | Cache miss, rate limit open | Restore Redis; epochs resync on bump |
| Hangfire stuck | Jobs not running | Check Redis storage; restart API |
| Partial upload | File on disk, DB rollback | Orphan cleanup job (`OrphanFileCleanupService`) |
| SMTP down | Email queue fails | Hangfire retry; alert ops |
| SignalR disconnect | UI stale | Auto-reconnect + 60s poll fallback |

## Idempotency

- Invoice generation: guard duplicate invoice per order set
- Order status: `RowVersion` concurrency token
- Payments: access checks + status machine

## Retry strategy

| Layer | Policy |
|-------|--------|
| MySQL (EF) | Connection retry (Infrastructure DI) |
| HTTP clients | Polly not global — add for external webhooks Week 4 |
| Hangfire | Automatic retries on job failure |
| Upload | User must retry; server rejects invalid state |

## Manual recovery

1. **Stuck order:** Admin status transition via allowed path only (`OrderStatusStateMachine`).
2. **Orphan files:** Wait for daily cleanup or run maintenance endpoint/job.
3. **Overdue invoices:** Batch job preferred over list-triggered updates.

## Post-incident

- Export Serilog logs + slow query log
- Record epoch/cache version at incident time
- Run integration tests on restored staging clone

See [RESILIENCE_STRATEGY.md](./RESILIENCE_STRATEGY.md).
