# Cache Strategy (Week 3)

## Model: epoch-based invalidation

No key scanning. Writers call `IReadModelCacheVersions`:

| Bump | Triggered by |
|------|----------------|
| `BumpOrders` | Order/invoice/payment/payout mutations |
| `BumpAnalytics` | User changes (also affects dashboards) |
| `BumpUsers` | User CRUD |
| `BumpRoles` | **Not wired** — role list cached 10 min |

Cache keys embed epoch: `ldp:cache:orders:paged:e{OrdersEpoch}:...`

**Helper:** `DistributedJsonCache` — `GetSafeAsync` / `SetSafeAsync` (swallow Redis errors, fail-open).

## TTLs (approximate)

| Key prefix | TTL |
|------------|-----|
| `analytics:overview` | 7 min |
| `analytics:orders` | 7 min |
| `orders:paged` | 7 min |
| `users` | 60 s |
| `roles` | 10 min |

## Invalidation gaps

1. **`BumpRoles` never called** — stale role list until TTL after role CRUD (future).
2. **Financial analytics** uses `OrdersEpoch` only — user-only changes may leave financial cache warm until TTL.
3. **In-memory fallback** — per-node epochs (split brain).

## Multi-instance rules

- Production **must** use Redis for `IDistributedCache` + epochs.
- Do not rely on TTL alone for security-sensitive data (permissions revalidated on API).

## Stale data acceptance

- Order list: max ~7 min stale for non-critical fields; mutations bump epoch immediately on writer node.
- Analytics: acceptable for executive dashboard; not for real-time ops (use SignalR for events).

## Week 4 improvements

- Wire `BumpRoles` on role mutation APIs.
- Include `AnalyticsEpoch` in financial cache keys.
- Optional hot-key purge on bump for admin-forced refresh.
