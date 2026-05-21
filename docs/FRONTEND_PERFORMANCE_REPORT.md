# Frontend Performance Report (Week 3)

**Stack:** Angular, PrimeNG, SignalR client, lazy-loaded feature modules.

## Strengths

- **Lazy routes** for dashboard, orders, users, invoices, etc. (`app-routing.module.ts`)
- Single `MainLayoutComponent` shell (avoids sidebar remount on navigation)
- Realtime with reconnect + polling fallback
- Production bundle budgets: initial 500kb warn / 1mb error

## Issues identified

| Area | Finding | Severity |
|------|---------|----------|
| Dashboard | Multiple chart widgets + analytics API calls on load | High |
| PrimeNG | Full module imports in feature modules inflate chunks | Medium |
| Change detection | Default strategy on large tables (orders/invoices) | Medium |
| RxJS | Potential duplicate subscriptions if not `async` pipe / `takeUntilDestroyed` | Medium |
| Images | Logo/assets not always lazy-loaded | Low |
| Bundle | PrimeNG + charts dominate initial lazy chunk | Medium |

## Recommendations (Week 3 doc only — code in Week 4)

1. **OnPush** on list/detail components with immutable inputs.
2. **PrimeNG:** import only used modules per feature (`TableModule`, not entire `PrimengModule` barrel).
3. **Dashboard:** single combined analytics endpoint or parallel `forkJoin` with skeleton UI.
4. **Virtual scroll** for order/invoice tables > 50 rows.
5. **Defer chart render** until tab visible (`IntersectionObserver`).
6. Run `ng build --stats-json` and analyze with `webpack-bundle-analyzer`.

## Metrics to capture (staging)

| Metric | Target |
|--------|--------|
| LCP (dashboard) | < 2.5s |
| Initial JS (lazy dashboard route) | < 400kb gzip |
| Time to interactive | < 3.5s |

See [ANGULAR_OPTIMIZATION_GUIDE.md](./ANGULAR_OPTIMIZATION_GUIDE.md).
