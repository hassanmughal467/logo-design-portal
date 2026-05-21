# Angular Upgrade Plan (15 → 18/19 LTS)

**Current:** Angular 15.2.10, PrimeNG 15.4.0, TypeScript 4.9.5, RxJS 7.5.7  
**Target:** Angular 18 or 19 LTS with matching PrimeNG  
**Approach:** Incremental — **do not** single-hop upgrade

---

## Upgrade path

```
15.2 → 16.x → 17.x → 18.x (or 19.x)
```

Each hop: `ng update @angular/core@N @angular/cli@N` + fix breaking changes + full test suite.

---

## Phase 0 — Preparation (Week 4, no version bump)

- [ ] Pin dependency versions in `package-lock.json`
- [ ] Document all PrimeNG component usage (`p-dropdown`, `p-calendar`, `p-tabView`, etc.)
- [ ] Enable CI production build on every PR (already in `test.yml`)
- [ ] Expand Playwright smoke on PR (recommended)
- [ ] Create upgrade branch `feature/angular-16` — no merge until hop 1 green

## Phase 1 — Angular 16 (est. 1–2 weeks)

- Update TypeScript to 5.0+
- Run `ng update @angular/core@16`
- Fix deprecated APIs flagged by CLI
- Verify Karma + Playwright green

## Phase 2 — Angular 17 (est. 2–3 weeks)

- Migrate `angular.json` toward `application` builder (can defer to 18)
- Optional: begin standalone bootstrap for `AppComponent` only
- PrimeNG 16/17 alignment research

## Phase 3 — Angular 18 (est. 3–4 weeks)

- TypeScript 5.4+
- Full `application` builder migration
- PrimeNG 18 template selector migrations (largest FE effort)
- Update `@angular/cdk`, `ng-apexcharts`, `chart.js` peers
- SignalR client — verify compatibility (no change expected)

## Phase 4 — Angular 19 (optional, after 18 stable)

- Zoneless evaluation (optional, not required for launch)
- Vitest migration (optional)

---

## Breaking changes to expect

| Area | Impact |
|------|--------|
| PrimeNG 17+ | Component renames (`p-select`, `p-datepicker`, `p-tabs`) |
| NgModules | 65 modules — gradual standalone migration or keep modules through 17 |
| `platformBrowserDynamic` | Replace with `bootstrapApplication` |
| HttpClient | `provideHttpClient(withInterceptorsFromDi())` |
| TypeScript strictness | May surface new template errors |
| E2E POMs | Update selectors in `e2e/page-objects/` |

---

## Risk controls

- One major version per release branch
- Full `test.yml` + `e2e-playwright.yml` before merge
- Staging soak 48h after each hop
- Rollback: redeploy previous frontend artifact (no DB impact)

---

## Do NOT do before production launch

- Full standalone migration
- Zoneless change
- PrimeNG major bump without dedicated QA sprint

See `DEPENDENCY_AUDIT.md` and `FRONTEND_MODERNIZATION_ROADMAP.md`.
