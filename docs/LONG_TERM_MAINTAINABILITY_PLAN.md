# Long-Term Maintainability Plan

## Current state

- Layered backend: Domain → Application → Infrastructure → API (**healthy**)
- Large application services: `OrderService`, `InvoiceService`, `FileService` (800–1200 LOC)
- Frontend monolith: NgModule-based, mega `dashboard.component.ts`
- Documentation: 50+ docs in `docs/` — strong but needs index

## Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| God services | Slow onboarding, regression fear | Extract by bounded context (orders, billing) |
| Angular version lag | Security, hiring | `ANGULAR_UPGRADE_PLAN.md` |
| Legacy backend folder | Wrong deploy | Delete/archive `Backend/LogoDesignPortal.API` |
| Missing deploy automation | Human error | CI publish workflow |
| Architecture drift | Inconsistent patterns | `ARCHITECTURE_EVOLUTION_PLAN.md` |

## 12-month maintainability goals

1. No service file &gt; 600 LOC without documented split plan
2. Angular 18 LTS on production
3. Onboarding doc: new engineer productive in 3 days
4. Single `docs/README.md` index to all module guides
5. Automated dependency updates (Dependabot) with CI gates

## Service boundary recommendations

| Context | Owns |
|---------|------|
| Order workflow | Status machine, assign, comments |
| Billing | Invoices, payments, PayPal |
| Content | Files, revisions, uploads |
| Identity | Auth, users, permissions |
| Analytics | Read-only reporting services |

Keep single deployable — extract **classes/namespaces**, not microservices.

## Documentation gaps to close

- [ ] `docs/README.md` master index
- [ ] Update root `DEPLOYMENT_CHECKLIST.md` pointer to `docs/DEPLOYMENT_CHECKLIST.md`
- [ ] API changelog per release

## Onboarding path

1. Read `docs/UPDATED_ARCHITECTURE_ONBOARDING.md`
2. Run local stack `ENVIRONMENT_SETUP_GUIDE.md`
3. Run tests `TESTING_STANDARDS.md`
4. Pick module guide for first task area

See `ENGINEERING_STANDARDS_GUIDE.md`.
