# Resilience Strategy

## Principles

1. **Fail closed on auth** — never fail open on authorization.
2. **Fail open on cache** (current) — availability over freshness; monitor staleness.
3. **Fail closed on multi-instance** without Redis — refuse silent in-memory fallback in Production.
4. **Idempotent writes** where money/status involved.

## Transient failures

- Database: built-in retry + 120s command timeout
- Redis cache: `DistributedJsonCache` swallows errors → uncached path
- Rate limiter: Redis errors allow traffic (documented risk)

## Background jobs

- Hangfire on Redis storage — single scheduler across instances
- Jobs: orphan file cleanup, billing maintenance
- **Do not** run duplicate schedulers on memory storage in multi-instance

## Upload pipeline

1. Validate → temp file
2. Transaction: DB + move to permanent
3. On failure: temp remains until orphan cleanup

## Week 4 hardening

- Fail-closed rate limit for login/upload when Redis unavailable
- Move overdue invoice updates to scheduled job
- Dead-letter queue for failed emails
- Circuit breaker for external SMTP

## Health endpoints

- `/health/ready` — DB, Redis (if configured), files, Hangfire, SignalR registration
- Use for load balancer drain before deploy
