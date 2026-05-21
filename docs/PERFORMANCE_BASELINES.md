# Performance Baselines (Week 3 — pre-staging targets)

Establish actual numbers after first staging k6 run. Targets below are **goals** for single-region, 2× API + Redis + MySQL.

## API latency (p95)

| Endpoint | Target p95 |
|----------|------------|
| `POST /api/auth/login` | < 800 ms |
| `GET /api/orders?page=1` | < 1200 ms |
| `GET /api/orders/{id}` | < 600 ms |
| `GET /api/analytics/overview` (cached) | < 400 ms |
| `GET /api/analytics/overview` (cold) | < 2500 ms |
| `GET /api/invoices?page=1` | < 1500 ms |
| File upload (5 MB) | < 5000 ms |

## Throughput

| Workload | Target |
|----------|--------|
| Concurrent read VUs | 40–60 sustained |
| Concurrent order status updates | 20 without conflict storms |
| Auth spike | 50 VUs, < 5% errors |

## Safe concurrent users (estimate)

| Topology | Authenticated users |
|----------|---------------------|
| 1× API, no Redis, local files | 15–30 |
| 2× API + Redis + shared/NAS files | 80–150 |
| 2× API + Redis + S3 + CDN | 150–300 |

Refine after load test with real hardware.

## Database

- Slow query log: 0 sustained > 2s under baseline load
- Connection pool: monitor; default EF pool usually sufficient at < 100 VUs
