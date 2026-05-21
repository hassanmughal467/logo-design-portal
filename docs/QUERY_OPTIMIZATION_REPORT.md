# Query Optimization Report (Week 3)

## Patterns applied

### 1. Single aggregate for dashboard overview

**Before:** ~10–15 `CountAsync`/`SumAsync` on `LogoOrders` per cache miss.  
**After:** One `GroupBy(_ => 1)` projection with conditional counts and revenue sums.

**File:** `AnalyticsService.cs` — `BuildOverviewUncachedAsync`

### 2. Batched file counts (avoid N+1 and heavy includes)

**Pattern:** `ApplyFileCountsAsync` — `GroupBy(OrderId)` on `LogoFiles` with `Total` + `Visible` counts.

**Applied to:** `GetOrderByIdAsync` (removed `Include(o => o.Files)`).

### 3. Admin notification recipient lookup

**Before:** Two `Roles` queries + user filter.  
**After:** One `Roles` query (`Admin` OR `SuperAdmin`) + one `Users` query.

**File:** `OrderService.GetAdminAndSuperAdminUserIdsAsync`

### 4. Read-only queries

Add `.AsNoTracking()` on all read-only service methods that do not call `SaveChangesAsync` immediately after load.

**Priority queue:** `MessageService`, `NotificationService` (legacy full list), `UserService`, `CommentService`, `FileService` list/download.

## Pagination (already strong)

| Endpoint | Pattern |
|----------|---------|
| Orders paged | ID page → hydrate includes → cache key with epoch |
| Invoices paged | ID page → includes → `LoadInvoiceOrderLineInfoAsync` |
| Billing queue | `Select` projection on page rows |

## Projection opportunities (not yet implemented)

| Service | Change |
|---------|--------|
| `UserService.GetUsersPagedAsync` | `Select` → DTO instead of 3× `Include` + map |
| `FileService.GetAllFilesPagedAsync` | Project `FileResponseDto`; drop duplicate `Include(Order)` |
| `ClientAnalyticsService.GetTopClientsAsync` | Aggregate by `ClientId`, join names in second query |

## Query splitting

Add `.AsSplitQuery()` when retaining multiple collection includes:

```csharp
await query.AsSplitQuery().ToListAsync();
```

**Candidates:** `DesignerPayoutService.GetDesignerInvoiceByIdAsync`, paged order hydrate, message threads.

## Caching interaction

Read-model keys embed `OrdersEpoch` / `AnalyticsEpoch`. Query optimizations reduce DB time; cache TTL (7–15 min) still caps load on hot dashboards.
