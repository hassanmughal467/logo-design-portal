# Architecture Evolution Plan

**Constraint:** Remain modular monolith — **no microservices migration**.

## Current maturity: **B+**

Strengths: clear layers, state machine, security middleware, Redis scale-out path, comprehensive integration tests.

## Phase 1 (0–3 months) — Launch stability

- Harden production config (CORS, hosts, Redis HA)
- Complete PayPal webhook processing
- CSRF + cookie auth test coverage
- Remove legacy backend folder from repo
- CI artifact publish for IIS deploy

## Phase 2 (3–6 months) — Maintainability

- Split `OrderService` into orchestrator + queries + commands
- Angular 16→17 upgrade
- Enable enforcing CSP
- Cloud storage adapter behind `IFileStorageProvider` (optional)

## Phase 3 (6–12 months) — Scale

- Read replicas for reporting queries
- CDN for static frontend + public assets
- Horizontal API instances (2–4) with shared Redis + file storage
- Observability: App Insights / OTel with SLO dashboards

## Anti-patterns to avoid

- Duplicating authorization in controllers only
- Bypassing `OrderStatusStateMachine`
- Per-controller upload validation
- Second API codebase outside `Backend/src/`

## Decision log template

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-05 | Invoice access uses `PaymentInvoiceAccessHelper` | Single rule for read + payment |

## Alignment with Week 1–3

- Week 1 security → validator, upload helper, privacy tests
- Week 2 QA → integration gate, Playwright catalog
- Week 3 performance → Redis, indexes, caching epochs
- Week 4 production → config hardening, DR docs, invoice IDOR fix
