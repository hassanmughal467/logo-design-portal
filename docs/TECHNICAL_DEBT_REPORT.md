# Technical Debt Report

**Last updated:** May 2026 (Week 3 performance pass)

---

## Week 3 additions

| Priority | Item | Notes |
|----------|------|-------|
| **HIGH** | Local file storage on load-balanced API | Blocks horizontal scale; `IFileStorageProvider` added, migration pending |
| **HIGH** | Analytics/workflow duplicate DB round-trips | Overview fixed; workflow/client analytics remain |
| **HIGH** | Invoice overdue update on list read | Side-effect in `GetInvoicesAsync` |
| **MEDIUM** | `AllowInMemoryFallback: true` in prod | Fixed to `false` in `appsettings.Production.json` |
| **MEDIUM** | `BumpRoles` never called | Stale role cache up to 10 min |
| **MEDIUM** | Rate limiter fail-open on Redis errors | DoS risk under Redis outage |
| **MEDIUM** | Large Angular dashboard + PrimeNG bundles | See FRONTEND_PERFORMANCE_REPORT.md |
| **LOW** | Duplicate `GetAdminAndSuperAdminUserIdsAsync` | 4 services |

---

## Priority legend

| Priority | Meaning |
|----------|---------|
| P0 | Launch blocker |
| P1 | High — security or reliability |
| P2 | Medium — maintainability / scale |
| P3 | Low — cleanup |

---

## P0 — Launch blockers

| ID | Item | Files | Effort |
|----|------|-------|--------|
| TD-001 | JWT secrets in committed config | `appsettings*.json` | S |
| TD-002 | Redis required for multi-instance prod | `Program.cs`, deploy docs | S |

---

## P1 — High priority

| ID | Item | Business impact | Security | Scalability | Fix | Effort |
|----|------|-----------------|----------|-------------|-----|--------|
| TD-010 | State machine not enforced on all transitions | Wrong order states | Medium | Low | Wrap workflow status changes with `ValidateTransition` | M |
| TD-011 | Permission system decorative | Admin over-provision | High | Low | Wire `[RequirePermission]` on mutations | M |
| TD-012 | localStorage JWT | Session hijack on XSS | High | — | httpOnly cookie auth | L |
| TD-013 | Monolithic OrderService (~2.3k LOC) | Slow feature delivery | Low | Medium | Split by workflow area | L |
| TD-014 | Unused `IRepository<T>` | Confusion | Low | Low | Remove or adopt UoW | S |

---

## P2 — Medium priority

| ID | Item | Files | Effort |
|----|------|-------|--------|
| TD-020 | `PermissionGuard` unused | `permission.guard.ts` | S |
| TD-021 | `HasPermissionDirective` unused | `has-permission.directive.ts` | S |
| TD-022 | `ErrorInterceptor` dead | `error.interceptor.ts` | S |
| TD-023 | Missing `/profile` route | `main-layout.component.ts` | S |
| TD-024 | Order detail nested `:id` routing | `orders-routing.module.ts` | S |
| TD-025 | Application ↔ ASP.NET coupling (`IFormFile`) | `FileService` | M |
| TD-026 | Infrastructure → Application dependency inversion | Project refs | L |
| TD-027 | AutoMapper CVE NU1903 | `.csproj` | S |
| TD-028 | Service ctor subscriptions without teardown | FE core services | S |

---

## P3 — Low priority

| ID | Item | Effort |
|----|------|--------|
| TD-030 | Unify naming: LogoFile vs File DTOs | S |
| TD-031 | Karma coverage excludes most features | M |
| TD-032 | Wildcard route always → dashboard | S |
| TD-033 | `CancelledByUser` not in state machine table | S |

---

## Improvement plan summary

### Implemented (this audit)

- Client file download IDOR fix + tests
- Swagger production guard
- Security headers middleware
- Upload sanitization, per-file size cap, scan hook interface, duplicate window
- Frontend 401 interceptor race fix

### Next sprint (recommended)

1. Env-based JWT + secret scanning in CI
2. Permission wiring on Files/Orders/Invoices controllers
3. State machine audit on `OrderService` / `RevisionService`
4. Split dashboard component + aggregate API
5. Cookie-based auth spike

---

## Migration risk notes

| Change | Risk |
|--------|------|
| Stricter file download | Low — correct behavior |
| Per-file 100MB limit | Medium — notify users uploading >100MB single files |
| Swagger off in prod | Low |
| Cookie auth | High — requires coordinated FE/BE deploy |
