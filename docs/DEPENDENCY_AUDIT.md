# Frontend Dependency Audit — May 2026

## Core framework

| Package | Current | Target (18 LTS) | Notes |
|---------|---------|-----------------|-------|
| @angular/* | 15.2.10 | 18.2.x | Step through 16, 17 |
| typescript | 4.9.5 | 5.4+ | Blocker for 18 |
| zone.js | 0.12.0 | 0.14.x | Bundled with Angular |
| rxjs | 7.5.7 | 7.8.x | Compatible; audit `subscribe` leaks |

## UI

| Package | Current | Target | Risk |
|---------|---------|--------|------|
| primeng | 15.4.0 | 18.x | **High** — template breaking changes |
| primeicons | 6.0.1 | 7.x | Low |
| @angular/cdk | 15.2.9 | 18.x | Align patch with core |

## Charts / realtime

| Package | Current | Risk on upgrade |
|---------|---------|-----------------|
| chart.js | 3.9.1 | Verify Angular 18 wrappers |
| ng-apexcharts | 1.7.0 | Check peer deps |
| apexcharts | 5.10.3 | Independent |
| @microsoft/signalr | 10.0.0 | Low — framework agnostic |

## Testing

| Package | Current | Recommendation |
|---------|---------|----------------|
| @playwright/test | 1.58.2 | Keep current; update after Angular stable |
| karma/jasmine | 6.4 / 4.6 | Consider Vitest at Angular 18+ |
| @stryker-mutator/* | 9.6.0 | Re-run after upgrade |

## Security advisories

Run before each release:

```bash
cd Frontend && npm audit --production
```

Address **high/critical** in `package-lock.json`; document accepted risks for moderate.

## Backend (reference)

- .NET 8 — current LTS, no migration required
- EF Core 8 — aligned
- MySQL provider — Pomelo/Oracle per project file

## Conflicts to watch

- PrimeNG major + Angular major must match vendor matrix
- `ng-apexcharts@1.7` may lag Angular 18 — check GitHub issues before bump
- Do not upgrade TypeScript before Angular CLI supports it
