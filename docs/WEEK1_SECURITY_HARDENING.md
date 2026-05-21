# Week 1 — Security Hardening Summary

## Scope delivered

| Area | Status | Notes |
|------|--------|-------|
| CI/CD enforcement | Done | `test.yml`, `pr-validation.yml`, `Directory.Build.props`, `.editorconfig` |
| Swagger security | Done | Development-only registration and middleware |
| JWT / auth hardening | Partial | Stricter validation; cookie path exists; frontend still uses localStorage |
| Upload security | Done | Centralized `UploadSecurityHelper`, magic bytes, double-extension decoy detection |
| Input sanitization | Partial | `TextInputSanitizer` for quotes; global JSON middleware still disabled |
| Secret management prep | Done | Placeholders, `ProductionSecretsValidator`, production appsettings cleanup |
| Logging foundations | Done | Correlation IDs, request enrichment; sensitive logging policy documented |
| Health checks | Done | DB schema, Hangfire, Redis, file storage, SMTP (degraded), disk space |
| Tests | Done | Unit + integration + CI Playwright smoke subset |

## Auth security findings

1. **localStorage tokens (HIGH)** — `Frontend/src/app/core/services/auth.service.ts` stores access/refresh tokens in `localStorage` (XSS exfiltration risk).
2. **HttpOnly cookies (ready)** — `AuthCookieService` + `CsrfValidationMiddleware` support cookie mode; SPA can migrate without API contract break.
3. **Refresh rotation** — Refresh tokens stored server-side on `User`; recommend rotation on each refresh (Week 2).
4. **SignalR** — Token via `access_token` query on `/hubs` is required for WebSockets; restrict hub authorization and short access token TTL.
5. **Recommended access token TTL** — Production: **30 minutes** (`Jwt:AccessTokenExpiryMinutes` in `appsettings.Production.json`). Refresh: **7 days** with rotation.

## Migration roadmap (cookies / BFF)

1. Enable `AuthCookies:Enabled` for pilot origins.
2. Frontend: stop writing tokens to `localStorage`; use `withCredentials` + CSRF header.
3. Add BFF or same-site reverse proxy if third-party cookie restrictions block SPA.
4. Deprecate Bearer header for browser clients after soak period.

## Validation checklist

- [ ] Production host returns 404 for `/swagger/*`
- [ ] `Jwt__Key` set only via environment / Key Vault
- [ ] `IncludeExceptionDetailsInProduction` = false
- [ ] Upload `.exe`, `logo.png.exe`, PDF-as-PNG rejected (integration tests)
- [ ] `/health/ready` reports DB, storage, Hangfire

## Launch readiness score: **72 / 100**

Blockers before production: JWT secret in env, migrate off localStorage, Redis in production, SMTP secrets, package CVE backlog (AutoMapper, OpenTelemetry).
