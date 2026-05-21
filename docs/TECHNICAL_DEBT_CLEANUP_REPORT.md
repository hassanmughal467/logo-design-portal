# Technical Debt Cleanup Report — Week 4

## Actions taken (safe refactors)

| Item | Action | Risk |
|------|--------|------|
| Designer invoice IDOR | `InvoiceService` aligned with `PaymentInvoiceAccessHelper` | Low — tests added |
| Staging exception leakage | `IncludeExceptionDetailsInProduction: false` + validator extended to Staging | Low |
| Production auto-migrate | `Database:RunAfterStartup: false` in Production JSON | Low — manual migrate required |
| Production email links | `Email:FrontendUrl` → production admin HTTPS | Low |
| Production config tests | `ProductionConfigurationValidationTests`, `DatabaseInitializationSafetyTests` | None |

## Prioritized backlog (not changed — document only)

### P0 — Pre-launch

| Debt | Location | Recommendation |
|------|----------|----------------|
| Broad production CORS | `appsettings.Production.json` | Remove localhost/LAN/IP entries via env override |
| Tokens in auth JSON | `AuthController` / `AuthResponseDto` | Cookie-only for browser in Phase 2 |
| PayPal webhook handlers | `PaymentsController` TODO | Implement + integration tests |

### P1 — First 30 days

| Debt | Location | Recommendation |
|------|----------|----------------|
| `OrderService` / `InvoiceService` size (1000+ LOC) | Application/Services | Extract query builders / command handlers incrementally |
| `dashboard.component.ts` (~1000+ LOC) | Frontend | Split into smart/dumb components |
| Disabled `InputSanitizationMiddleware` | `Program.cs` | Re-enable when stream-safe |
| Slow-query log redaction | `SlowQueryLoggingInterceptor` | Strip sensitive column names |
| Legacy API folder | `Backend/LogoDesignPortal.API` | Archive or delete after operator confirmation |

### P2 — 90 days

| Debt | Location | Recommendation |
|------|----------|----------------|
| Duplicate JSON options in controllers | `OrdersController`, `QuotesController` | Shared cached `JsonSerializerOptions` |
| `loadClientLegacy` / paginated invoice fallbacks | Frontend | Remove after API version stable |
| Open client registration | `AuthController` | Policy flag |
| SVG upload policy | `UploadSecurityHelper` | Disallow or sandbox serve |

### Stale TODOs (Backend/src)

| File | TODO |
|------|------|
| `InvoiceService.cs` | Email sending |
| `PaymentsController.cs` | Webhook event processing |
| `SettingsController.cs` | Logo upload to storage |

### Dead / unused (verified in use)

- `CookieCredentialsInterceptor`, `CsrfInterceptor` — **registered** in `app.module.ts` (not dead; **undertested**)
- No unused Angular guards found blocking routes

## Duplication hotspots

- Invoice access rules — **consolidated** to `PaymentInvoiceAccessHelper` for read paths
- CORS origin resolution — `CorsAllowedOrigins` + config union (documented; avoid duplicating in `Program.cs`)

## Naming / consistency

- Canonical backend: `Backend/src/LogoDesignPortal.*`
- Test naming follows `TESTING_STANDARDS.md` — consistent

See `SAFE_REFACTORING_SUMMARY.md` for test evidence.
