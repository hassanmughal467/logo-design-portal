# Load Testing Plan

## Phase 1 — Local / CI smoke (Week 2)

- k6 `auth-spike.js` against dev API (validate rate limiting, no 5xx storm)
- Manual: 10 concurrent uploads with distinct orders
- Health endpoints under light parallel GET

## Phase 2 — Staging (Week 3 target)

| Scenario | VUs | Duration | Success criteria |
|----------|-----|----------|------------------|
| Auth spike | 50 | 3m | <5% 5xx; 429 acceptable |
| Order create | 20 | 5m | p95 < 3s |
| Dashboard read | 30 | 5m | p95 < 1.5s |
| SignalR connections | 100 | 10m | reconnect < 5s |

Provision dedicated staging users; never use production credentials.

## Phase 3 — Pre-launch

- Soak test 2h at 50% target VUs
- DB slow-query log review (MySQL `long_query_time`)
- Memory health `degraded` threshold tuning

## Environment variables

| Variable | Purpose |
|----------|---------|
| `API_BASE_URL` | k6 target host |
| `K6_CLIENT_TOKEN` | Bearer for order scripts |
| `K6_ADMIN_TOKEN` | Bearer for dashboard script |

## Bottleneck triage

1. Correlate k6 p95 with Serilog slow-request warnings
2. Enable `Observability:Enabled` + OTLP on staging
3. Compare before/after index migrations

## Out of scope (Week 2)

- Distributed k6 execution
- Production load tests (forbidden without change window)
