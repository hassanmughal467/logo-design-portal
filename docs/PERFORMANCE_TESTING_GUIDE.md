# Performance Testing Guide

## Initial load testing (k6)

Scripts under `load-tests/k6/`:

| Script | Scenario |
|--------|----------|
| `auth-spike.js` | Ramp login attempts (rate-limit validation) |
| `concurrent-orders.js` | Parallel order creation (needs `K6_CLIENT_TOKEN`) |
| `dashboard-read.js` | Admin analytics + list endpoints |

### Run

```bash
k6 run load-tests/k6/auth-spike.js
K6_CLIENT_TOKEN=<jwt> k6 run load-tests/k6/concurrent-orders.js
K6_ADMIN_TOKEN=<jwt> k6 run load-tests/k6/dashboard-read.js
```

Set `API_BASE_URL` for non-local targets.

## API SLO signals (built-in)

- `SlowRequestPerformanceMiddleware` warns above **500ms**
- Serilog request template includes elapsed ms

## Known bottleneck candidates

| Area | Risk |
|------|------|
| Order list + analytics | Heavy joins; watch p95 on `/api/admin/analytics/overview` |
| File upload | Disk I/O + validation; limit concurrent uploads in k6 |
| Invoice batch create | DB writes per order line |
| SignalR + Redis | Connection count under multi-tab load |
| Angular dashboard | Multiple parallel API calls — profile Network tab |

## Concurrency test plan (integration)

- `OrdersControllerConcurrencyTests` — parallel status updates
- Add staging runs before release with k6 + DB slow-query log

## Frontend bundle

- Run `ng build --stats-json` and analyze with webpack-bundle-analyzer
- Lazy-load large PrimeNG feature modules where missing

See `docs/LOAD_TESTING_PLAN.md` for staged rollout.
