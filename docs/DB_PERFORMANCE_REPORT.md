# Database Performance Report (Week 3)

**Scope:** `Backend/src/` — EF Core, `ApplicationDbContext`, Application services.  
**Date:** 2026-05-21

## Executive summary

The data layer is production-capable with good pagination patterns on orders/invoices and batched file counts on order lists. Primary risks are **analytics duplicate round-trips**, **tracked read paths without `AsNoTracking`**, **cartesian includes on order detail**, and **missing composite indexes** on billing/overdue/file-count queries.

## Findings by severity

### High

| Issue | Location | Impact |
|-------|----------|--------|
| Analytics overview ~15 `CountAsync`/`SumAsync` per cache miss | `AnalyticsService.BuildOverviewUncachedAsync` | Dashboard latency under load |
| Order detail loads full `Files` collection | `OrderService.GetOrderByIdAsync` | Row multiplication, memory |
| Invoice list hydration tracked (overdue side-effect) | `InvoiceService.GetInvoicesAsync` | Extra memory + write on read |
| No `AsSplitQuery` on multi-include graphs | `DesignerPayoutService`, `MessageService`, `FileService` | Cartesian explosion |

### Medium

| Issue | Location | Impact |
|-------|----------|--------|
| `ClientAnalyticsService` no `AsNoTracking` | All methods | Tracking overhead |
| Duplicate admin role lookups (2× `Roles`) | `OrderService`, `RevisionService`, `FileService` | Extra round-trips |
| `Include` inside `GroupBy` | `ClientAnalyticsService.GetTopClientsAsync` | Provider translation risk |
| Per-invoice log in overdue batch | `InvoiceService.BatchUpdateOverdueInvoicesAsync` | N inserts before save |

### Low

| Issue | Location | Impact |
|-------|----------|--------|
| `Repository<T>` already `AsNoTracking` on reads | Infrastructure | Good baseline |
| Order list ID-first paging + `ApplyFileCountsAsync` | `OrderService` | Good pattern |
| Slow query interceptor + 120s timeout | `DependencyInjection` | Observability |

## Implemented (Week 3)

- Consolidated analytics overview into **one aggregate query** (+ delivery/revision aux queries).
- Removed `Include(Files)` from order detail; **batched file counts** via `ApplyFileCountsAsync`.
- Single-query admin/super-admin role resolution in `OrderService`.
- `AsNoTracking` on `ClientAnalyticsService` overview paths and analytics aux queries.
- Migration `AddWeek3QueryPerformanceIndexes` (billing, deadline, file, invoice due-date indexes).
- `IFileStorageProvider` + `LocalFileStorageProvider` for cloud migration prep.

## Remaining recommendations

1. Add `AsNoTracking` + `AsSplitQuery` to `MessageService`, `UserService.GetUsersPagedAsync`, `DesignerPayoutService` list reads.
2. Move overdue invoice marking to **Hangfire** job (remove write-on-read from list API).
3. Collapse `AnalyticsService.GetWorkflowAnalyticsAsync` counts (same pattern as overview).
4. Compiled queries only if hot paths show repeated plan compilation in APM traces.

## Monitoring

- Enable slow-query logs (`SlowQueryLoggingInterceptor`) — threshold review in staging.
- Track p95 for `/api/analytics/*`, `/api/orders`, `/api/invoices`.

See also: [QUERY_OPTIMIZATION_REPORT.md](./QUERY_OPTIMIZATION_REPORT.md), [INDEX_RECOMMENDATIONS.md](./INDEX_RECOMMENDATIONS.md), [MIGRATION_RECOMMENDATIONS.md](./MIGRATION_RECOMMENDATIONS.md).
