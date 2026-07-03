# Architecture Decisions and Technical Debt

**Related:** [SYSTEM_OVERVIEW.md](./SYSTEM_OVERVIEW.md) · `docs/TECHNICAL_DEBT_REPORT.md` · `docs/ARCHITECTURE_EVOLUTION_PLAN.md`

---

## 1. Current strengths

| Area | Strength | Why it matters |
|------|----------|----------------|
| **Layered backend** | Clear Domain / Application / Infrastructure / API split | Testable business logic; onboarding clarity |
| **State machine** | `OrderStatusStateMachine` in Domain with tests | Prevents illegal workflow corruption |
| **Security depth** | Cookie + CSRF + JWT + permission filter + service tenant checks | Defense in depth for B2B portal |
| **DTO masking** | Server-side in services | Real IDOR/privacy protection |
| **Upload pipeline** | Centralized `UploadSecurityHelper` | Consistent file policy |
| **Scalability hooks** | Redis for cache, SignalR, Hangfire, rate limits | Multi-instance path exists |
| **CI maturity** | Matrix tests, 80% integration coverage gate, Playwright, release artifacts | Regression protection |
| **Ops documentation** | Extensive `docs/` runbooks | Production operability |
| **Kill switches** | `ProductionSafetyOptions` | Safe staging deploys |

---

## 2. Scalability strengths

- Stateless API with Redis-backed shared infrastructure
- Read-model caching with explicit cache version epochs
- EF indexes added for analytics hot paths
- Optimistic concurrency on orders/invoices
- Hangfire Redis storage avoids duplicate recurring jobs per node

---

## 3. Maintainability strengths

- Single canonical backend (`Backend/src/`)
- Explicit service registration (no magic DI scanning)
- Integration test factory + shared auth helpers
- Module QA docs per business area
- Cursor rules for architecture and testing

---

## 4. Technical debt inventory

| ID | Area | Debt | Risk | Evidence |
|----|------|------|------|----------|
| TD-01 | Application | `IRepository<T>` unused | Confusion for new devs | Registered but services use `IApplicationDbContext` |
| TD-02 | API | `InputSanitizationMiddleware` disabled | XSS/injection reliance on other layers | Commented in `Program.cs` |
| TD-03 | Files | `NullFileUploadScanHook` default | Malware in uploads | No production scanner wired |
| TD-04 | Storage | Local disk `FileStorage:Path` | Multi-node broken without shared volume | Per-server files |
| TD-05 | Frontend | Large `order-detail` component | Hard to test/change | Single large TS file |
| TD-06 | Frontend | `PermissionGuard` not routed | UX/API mismatch | Guard exists, unused |
| TD-07 | Auth | Symmetric JWT HMAC | Key rotation coupling | Single shared secret |
| TD-08 | Frontend | Angular 15 | EOL security patches | `package.json` 15.2.x |
| TD-09 | Realtime | Limited automated SignalR tests | Regressions in hub auth | Manual verification |
| TD-10 | Deploy | IIS manual deploy | Human error in releases | Artifacts only in CI |
| TD-11 | CQRS | None — large service methods | `OrderService` complexity | God-service tendency |
| TD-12 | Payments | Webhook edge cases partial | Financial discrepancies | Coverage gap report |

---

## 5. Risky areas

| Area | Risk | Mitigation in flight |
|------|------|---------------------|
| Financial workflows | Double invoice, mark-paid race | Concurrency tokens + regression tests |
| Cross-tenant IDOR | Data leak between clients | Integration IDOR suite — extend on new endpoints |
| Cookie auth + CORS | Misconfigured origins break login | `EnvironmentConfigurationValidator` |
| Staging vs prod config drift | Wrong kill switch | Staging checklist docs |
| Redis outage in prod | Total rate limit/cache failure | Health ready probe; ops runbook |

---

## 6. Coupling issues

| Coupling | Impact | Recommendation |
|----------|--------|----------------|
| Application → EF via `IApplicationDbContext` | Hard to swap persistence | Acceptable for current scale; introduce specifications if queries proliferate |
| Invoice PDF logo sync MSBuild | Build depends on Frontend path | Embed committed logo or NuGet asset |
| Notification + email in same service | Email failure affects notification path | Already async via Hangfire — keep enqueue idempotent |
| PrimeNG version locked to Angular 15 | Upgrade batch required | Follow `docs/ANGULAR_UPGRADE_PLAN.md` |

---

## 7. Performance risks

| Risk | Symptom | Direction |
|------|---------|-----------|
| Uncached heavy analytics | Slow admin dashboard | Already versioned cache — monitor hit rate |
| Large order includes | N+1 queries | Query splitting / projections |
| File download through API | Bandwidth on app server | CDN/direct signed URLs (cloud migration guide) |
| Integration test collection serial | Slow CI | Split collections carefully if parallelizing |

---

## 8. Security improvement opportunities

1. Production **malware scanning** hook
2. Re-enable **input sanitization** safely
3. **PermissionGuard** on admin routes
4. **Asymmetric JWT** for multi-service future
5. **WAF** + IIS hardening checklist completion
6. **SVG** serving policy (sanitize or disallow client download)

---

## 9. Architecture inconsistencies

| Inconsistency | Resolution |
|---------------|------------|
| Repository pattern registered but unused | Remove or adopt — document says unused |
| Two health endpoints (`/health` vs `api/system/health`) | Document ops preference; consolidate long-term |
| E2E seed emails differ from integration seed | Align test credentials in docs/CI env |
| Swagger comment says staging excluded — code is Development-only only | Docs now match code |

---

## 10. Missing abstractions

| Abstraction | Would help |
|-------------|------------|
| `IOrderWorkflowService` | Split `OrderService` by workflow phase |
| `IFileStorageProvider` | S3/Azure swap without `FileService` rewrite |
| `IDomainEvent` dispatch | Decouple notifications from every mutation |
| Shared **pagination** contract document | Frontend/backend alignment |

---

## 11. Future scaling bottlenecks

| Bottleneck | Threshold | Evolution |
|------------|-----------|-----------|
| MySQL write primary | High order volume | Read replicas, archival |
| Local Files folder | >1 API instance | Object storage |
| Redis memory | Large cache entries | TTL tuning, smaller DTOs in cache |
| Hangfire workers | Email backlog | Dedicated worker role |
| SignalR connections | Thousands concurrent | Azure SignalR Service |

---

## 12. Prioritized improvement roadmap

### Short-term (0–3 months)

| Priority | Item | Effort |
|----------|------|--------|
| P0 | Wire `IFileUploadScanHook` in production | Medium |
| P0 | Complete POST_DEPLOY validation automation | Low |
| P1 | Split critical paths out of `OrderService` (assign, status, price) | Medium |
| P1 | Enable `PermissionGuard` on financial/admin routes | Low |
| P1 | Align E2E and integration test credentials | Low |
| P2 | Decompose `order-detail` into presentational components | Medium |

### Medium-term (3–9 months)

| Priority | Item | Effort |
|----------|------|--------|
| P1 | Shared file storage (SMB min, S3 ideal) | High |
| P1 | Angular 17+ upgrade path | High |
| P2 | Re-enable input sanitization middleware | Medium |
| P2 | SignalR integration test harness | Medium |
| P2 | IIS → automated deploy pipeline | High |
| P3 | Remove or implement repository pattern | Low |

### Long-term (9–18 months)

| Priority | Item | Effort |
|----------|------|--------|
| P1 | Container/Kubernetes or App Service hosting | High |
| P2 | Domain events + outbox for notifications/billing | High |
| P2 | Asymmetric JWT / external IdP option | High |
| P3 | Read replica analytics DB | Medium |
| P3 | Azure SignalR / managed Redis | Medium |

---

## 13. Architecture decision records (implicit)

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Auth transport | HttpOnly cookies + CSRF for SPA | Reduce XSS token theft vs localStorage |
| Workflow enforcement | Domain state machine | Single source of truth |
| Permissions | DB-seeded + attribute filter | Change without redeploy |
| Background work | Hangfire + Redis | Mature scheduling vs raw `IHostedService` duplicates |
| No CQRS | Service classes | Team velocity; smaller codebase |
| EF direct in services | No repository | Simpler queries; accepted coupling |
| PrimeNG | UI library | Enterprise components for admin portal |
| MySQL | Primary RDBMS | Existing ops expertise / hosting |

---

## 14. References

- `docs/TECHNICAL_DEBT_REPORT.md` — detailed debt items
- `docs/REFACTORING_ROADMAP.md` — engineering plan
- `docs/FRONTEND_MODERNIZATION_ROADMAP.md` — Angular upgrade
- `docs/ARCHITECTURE_EVOLUTION_PLAN.md` — strategic evolution
