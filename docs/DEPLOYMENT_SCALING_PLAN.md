# Deployment Scaling Plan

## Phase 1 — Single production (current)

- 1× IIS API + MySQL + local `Files/`
- Redis recommended even for 1× (Hangfire + cache consistency)

## Phase 2 — Active/active API (target)

- 2+ API instances behind load balancer
- **Mandatory:** Redis (`ConnectionStrings__Redis`, `AllowInMemoryFallback=false`)
- **Mandatory:** Shared file storage OR S3
- SignalR backplane auto-enabled when Redis connects
- Run DB migration before traffic shift

## Phase 3 — Autoscale

- CPU > 70% for 5 min → +1 instance (max 4)
- Scale in after 15 min below 40%
- Stateless API; all state in MySQL/Redis/storage

## Deploy sequence

1. Maintenance window or drain LB
2. `dotnet ef database update` on staging → prod
3. Deploy API binaries to all nodes
4. Smoke: `/health/ready`, login, order list, SignalR join
5. Restore traffic; watch p95 and error rate

## Rollback

- Previous API build on all nodes
- DB rollback only if migration is reversible (index-only migrations are safe to keep)

## Load test gate

No prod scale-out until staging k6 passes [LOAD_TEST_RESULTS_TEMPLATE.md](./LOAD_TEST_RESULTS_TEMPLATE.md).
