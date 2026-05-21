# Refactoring Roadmap (Week 3 → Week 6)

## HIGH priority

| Item | Effort | Outcome |
|------|--------|---------|
| Shared/S3 file storage | 3–5 d | Multi-instance safe uploads |
| Overdue invoice Hangfire job | 1 d | Remove write-on-read |
| `MessageService` AsNoTracking + split queries | 2 d | Thread load reduction |
| Frontend dashboard API consolidation | 2 d | Fewer round-trips |
| Staging k6 + baselines filled | 2 d | Capacity proof |

## MEDIUM priority

| Item | Effort | Outcome |
|------|--------|---------|
| Migrate `FileService` to `IFileStorageProvider` | 3 d | Cloud-ready |
| Collapse workflow analytics queries | 1 d | Dashboard p95 |
| `BumpRoles` on role CRUD | 0.5 d | Cache correctness |
| PrimeNG tree-shaking per module | 2 d | Smaller bundles |
| SignalR health + Redis check | 1 d | Deploy confidence |

## LOW priority

| Item | Effort | Outcome |
|------|--------|---------|
| Shared `GetAdminAndSuperAdminUserIdsAsync` helper | 0.5 d | DRY |
| Compiled queries (if APM proves need) | 1–2 d | Marginal CPU |
| Docker compose dev stack | 2 d | Onboarding |

## Services to split (when touching)

- `OrderService` (~2400 lines) — extract notification/pricing helpers
- `FileService` — extract authorization vs storage
- `AnalyticsService` — extract SQL aggregate builder

Do not big-bang refactor; extract on next feature touch.
