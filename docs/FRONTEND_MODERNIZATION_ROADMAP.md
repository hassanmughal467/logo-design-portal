# Frontend Modernization Roadmap

## Vision

Maintain Angular monolith with improved test pyramid, cookie-first auth, and incremental framework upgrades — **no micro-frontends**.

## 3-month roadmap

| Month | Focus | Deliverables |
|-------|-------|--------------|
| M1 | Test hardening | CSRF integration tests; expand PR Playwright smoke; 5 component tests (order-detail, invoice-list, file-upload) |
| M2 | Angular 16 hop | TS 5.0, `ng update`, PrimeNG compatibility pass |
| M3 | Ops + UX | Dashboard component split; remove legacy API response fallbacks |

## 6-month roadmap

| Month | Focus |
|-------|-------|
| M4–M5 | Angular 17–18, application builder, PrimeNG template migration |
| M6 | SignalR reconnect E2E; optional Vitest pilot |

## 12-month roadmap

- Angular 19 LTS evaluation
- Optional zoneless pilot on low-traffic routes
- Cloud storage upload UX (if backend migrates per `CLOUD_STORAGE_MIGRATION_GUIDE.md`)

## Technical themes

1. **Security:** Cookie + CSRF as primary browser auth; reduce token-in-body
2. **Performance:** Lazy routes audit; bundle analysis post-upgrade
3. **Maintainability:** Break mega-components; shared POM library for E2E
4. **Accessibility:** PrimeNG upgrade enables newer a11y patterns

## Success metrics

- PR Playwright: ≥8 specs (auth, RBAC, upload, invoice API)
- Frontend unit: ≥25% line coverage on `src/app` (from ~15 files today)
- Zero `test.skip(true)` in CI for security specs
- Production bundle size within 10% of baseline post-upgrade
