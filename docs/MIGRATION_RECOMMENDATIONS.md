# Migration Recommendations (Week 3)

## Apply in staging first

```bash
cd Backend/src/LogoDesignPortal.Infrastructure
dotnet ef database update --startup-project ../LogoDesignPortal.API
```

**Migration:** `20260521172138_AddWeek3QueryPerformanceIndexes`

Creates non-destructive indexes only (no column drops in final Week 3 revision).

## Pre-deploy checklist

1. Backup database (see `PRODUCTION-MIGRATION-SAFETY.md`).
2. Run migration on staging; verify index creation: `SHOW INDEX FROM LogoOrders`.
3. Monitor replication lag if using read replicas (index build is online on MySQL 8+).
4. Deploy API after migration succeeds (indexes benefit new queries immediately).

## Rollback

`dotnet ef database update <PreviousMigration>` — drops Week 3 indexes only.

## Post-deploy validation

- Analytics overview p95 ↓ (fewer round-trips).
- Order detail payload size stable (no file entities in graph).
- Billing queue filter on `CompletedDate` uses index (EXPLAIN).

## Do not

- Run destructive migrations without domain entity alignment.
- Skip migration on multi-instance deploys (all nodes share one DB).
