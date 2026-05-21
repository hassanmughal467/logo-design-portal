# Angular Optimization Guide

## Route splitting

Already lazy-loaded. Ensure new features use:

```typescript
loadChildren: () => import('./feature/feature.module').then(m => m.FeatureModule)
```

## Change detection

```typescript
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
})
```

Use `async` pipe in templates; avoid manual `subscribe` without teardown.

## RxJS

- Prefer `switchMap` for search/autocomplete.
- `shareReplay(1)` for cached reference data (roles, settings) with TTL in service.
- `takeUntilDestroyed()` (Angular 16+) in components.

## API call deduplication

- Centralize dashboard loads in one resolver or facade service.
- Guard duplicate calls with `if (this.loading$)` or `exhaustMap`.

## PrimeNG tables

- `[lazy]="true"` for server-side paging (matches API paged endpoints).
- `rows` ≤ 50 aligned with API `pageSize` cap.

## Charts

- Destroy chart on `ngOnDestroy`.
- Update datasets immutably to work with OnPush.

## Build

```bash
cd Frontend
ng build --configuration=production --stats-json
```

Review `dist/stats.json` for largest modules.
