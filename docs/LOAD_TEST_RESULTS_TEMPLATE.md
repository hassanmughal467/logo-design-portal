# Load Test Results Template

**Run ID:** _____________  
**Date:** _____________  
**Environment:** Staging / Production-like  
**API base:** _____________  
**Redis:** Yes / No  
**Instances:** ___

## k6 scenarios

| Script | VUs | Duration | p95 latency | Error rate | Notes |
|--------|-----|----------|-------------|------------|-------|
| `auth-spike.js` | 50 | 2m | | | |
| `concurrent-orders.js` | 30 | 3m | | | |
| `dashboard-read.js` | 40 | 5m | | | |
| `invoice-spike.js` | 30 | 2m | | | |
| SignalR soak | 100 | 10m | | | See signalr-soak.md |

## Resource metrics (during test)

| Metric | Value |
|--------|-------|
| API CPU avg / max | |
| API memory | |
| MySQL connections | |
| Redis memory | |
| Slow queries (>1s) | |

## Functional checks post-test

- [ ] No duplicate invoices
- [ ] Order status machine intact
- [ ] File downloads succeed
- [ ] SignalR reconnect OK

## Pass/fail

| Threshold | Target | Actual | Pass |
|-----------|--------|--------|------|
| HTTP error rate | < 5% | | |
| p95 API (read) | < 2000ms | | |
| p95 API (write) | < 3000ms | | |

**Sign-off:** _____________
