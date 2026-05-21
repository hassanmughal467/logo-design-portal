# Performance Optimization Report

**Last updated:** May 2026

---

## 1. Backend query performance

### Strengths
- Analytics indexes on `LogoOrder` (migrations `AddAnalyticsIndexes`, `AddClientAnalyticsIndex`)
- `PreviewBatchId` index on `LogoFile`
- `AsNoTracking` on read-heavy queries in several services
- Paged APIs (`page`, `pageSize` capped at 100)
- `ExecuteInTransactionAsync` with MySQL retry strategy

### Issues & recommendations

| Issue | Impact | Files | Fix | Effort |
|-------|--------|-------|-----|--------|
| N+1 includes on order lists | High | `OrderService` list methods | Projections / split queries | M |
| `GetAllFiles` loads large graphs | Medium | `FileService` | Pagination + selective columns | S |
| Dashboard aggregates multiple round-trips | High | `AnalyticsService`, FE `dashboard.service` | Server-side aggregate endpoint | M |
| Missing composite index `(ClientId, Status, IsDeleted)` | Medium | Migrations | Add index migration | S |
| EF tracking on long workflows | Low | Various | `AsNoTracking` + explicit updates | S |

---

## 2. Caching strategy

| Layer | Current | Recommended |
|-------|---------|-------------|
| Redis | Distributed cache when configured | Cache dashboard KPIs (60s TTL) |
| HTTP | None on API | `Cache-Control` on static file downloads |
| Frontend | `shareReplay` on users/orders lists | Invalidate on SignalR events (partial) |

**Do not cache** per-user order detail with designer/client fields — masking is role-specific.

---

## 3. SignalR optimization

- Debounce `orderUpdates$` in list components (300ms) to avoid grid thrash.
- Emit only changed fields in `SignalRRealtimeEntityUpdateSender` payloads.
- Avoid broadcasting to all admins for client-only events.

---

## 4. Frontend bundle & rendering

| Issue | Impact | Recommendation |
|-------|--------|----------------|
| Full PrimeNG theme in `styles.scss` | Initial CSS large | Switch to PrimeNG treeshake / preset |
| `dashboard` 1.7k TS + 1.6k SCSS | Change detection cost | OnPush + split widgets |
| `order-detail` monolith | Lazy chunk bloat | Extract tabs into sub-components |
| No route preloading | First nav delay | `PreloadAllModules` for core routes |
| Budget 1MB initial | CI warnings | Lazy-load charts (ApexCharts) |

---

## 5. File I/O

- Stream downloads instead of `ReadAllBytesAsync` for large finals (future).
- Orphan cleanup job: `OrphanFileCleanupService` (Hangfire recurring).

---

## 6. Production capacity estimate (order of magnitude)

Assumptions: 2 API instances, Redis, MySQL 4 vCPU, local SSD file storage.

| Metric | Estimate |
|--------|----------|
| Concurrent authenticated users | 150–300 |
| Order mutations / minute | ~200 (rate limit 120/user bucket) |
| File uploads / minute | ~60 cluster-wide (30/min per user) |
| SignalR connections | 500 with Redis backplane |

**Scaling blockers:** Single disk file storage, monolithic dashboard API pattern, no CDN for downloads.

---

## 7. Monitoring for performance

- `SlowRequestPerformanceMiddleware` — threshold alerts
- MySQL slow query log
- Redis latency
- Frontend: Lighthouse on dashboard route quarterly
