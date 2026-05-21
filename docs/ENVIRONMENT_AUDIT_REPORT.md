# Environment Architecture Audit Report

**Date:** May 2026  
**Scope:** `Backend/src/LogoDesignPortal.API`, `Frontend/`, IIS, CI workflows  
**Legacy:** `Backend/LogoDesignPortal.API/` (outside solution) — **ignored**

---

## Executive summary

The portal had partial multi-environment support (Development, Testing, Staging, Production JSON overlays) but suffered from **secret leakage in Development**, **permissive base CORS/LAN entries**, **missing staging frontend build**, **no structural config validator**, and **shared upload directory names**. This audit drove a hardened layout with isolated `FileStorage` paths, fail-fast validators, and documented deployment flows.

---

## appsettings structure (before → after)

| File | Role |
|------|------|
| `appsettings.json` | Production-safe defaults; minimal localhost CORS |
| `appsettings.Development.json` | LAN CORS, `Files_Dev`, User Secrets placeholders |
| `appsettings.Testing.json` | SQLite, `Files_Test`, integration JWT (not published) |
| `appsettings.Staging.json` | HTTPS hosts, `Files_Staging`, kill switches, test payments |
| `appsettings.Production.json` | Restricted hosts, no auto-migrate, env-only secrets |

**Inheritance:** ASP.NET Core merges base + `appsettings.{ASPNETCORE_ENVIRONMENT}.json` + environment variables (highest precedence).

---

## Findings

### Unsafe defaults (remediated)

| Issue | Risk | Remediation |
|-------|------|-------------|
| `Password=ADMIN` in Development JSON | Credential leak | Placeholder + User Secrets (`UserSecretsId` added) |
| JWT placeholder in Staging JSON | False sense of security | Removed; `Jwt__Key` required via env |
| Base CORS LAN/public IPs | Over-broad Staging CORS | Moved to Development overlay only |
| `AllowedHosts: *` in base | Host header attacks on Staging | Staging/Production restrict hosts |
| Shared `Files/` path | Cross-env overwrite | `Files_Dev`, `Files_Test`, `Files_Staging`, `Files` |

### Production risks (mitigated)

- **Startup fail-fast:** `ProductionSecretsValidator` + `EnvironmentConfigurationValidator`
- **Redis mandatory** in Staging/Production (`ScalabilityServiceRegistration`)
- **Swagger** Development-only
- **Auto-migration** disabled in Staging/Production (`Database:RunAfterStartup: false`)
- **Exception details** blocked when `IncludeExceptionDetailsInProduction: true`

### Hardcoded values

| Location | Notes |
|----------|-------|
| `CorsAllowedOrigins.cs` | Built-in hawkmerchandising origins (intentional) |
| `environment.production.ts` | Build-time API URL (expected for SPA) |
| `web.config` | `ASPNETCORE_ENVIRONMENT=Production` — use `web.config.staging.example.xml` for staging |
| E2E CI credentials | Acceptable in GitHub Actions secrets/env only |

### Missing separation (addressed)

- Angular **staging** and **testing** build configurations
- Staging IIS template (`web.config.staging.example.xml`)
- Central **ENVIRONMENT_VARIABLE_REFERENCE.md**
- Staging smoke test hook (`e2e/tests/smoke/staging-health.spec.ts`)

### Duplicated configs

- Overlap between base and Development CORS — Development now owns extended list
- `environment.prod.ts` vs `environment.production.ts` — prod re-exports production (backward compatible)
- Legacy `Backend/LogoDesignPortal.API/appsettings*` — duplicate; do not use

---

## Component audit

| Component | Development | Testing | Staging | Production |
|-----------|-------------|---------|---------|------------|
| **MySQL** | Local + secrets | SQLite (tests) | Env var | Env var |
| **Redis** | Optional (in-memory OK) | In-memory | Required | Required |
| **SignalR** | Local hub | In-memory backplane | Redis backplane | Redis backplane |
| **Hangfire** | In-memory OK | Disabled | Redis | Redis |
| **Uploads** | `Files_Dev` | `Files_Test` | `Files_Staging` | `Files` |
| **SMTP** | Optional | N/A | Staging relay | Production relay |
| **Payments** | N/A | N/A | `UseTestMode: true` | Live |

---

## CI/CD assumptions

- PR/push: `dotnet test` all matrices; Playwright uses `environment.e2e` → `environment.testing`
- Release tags: `deploy-artifacts.yml` builds **production** frontend + API zip
- Staging deploy: manual or extended workflow with `ng build --configuration=staging`

---

## Remaining gaps (operational)

1. **DNS/TLS** for `staging-*.hawkmerchandising.com` must exist before staging go-live
2. **Secret manager** not wired in code (env vars documented; Key Vault provider optional)
3. **Cloud file storage** — local paths only; see `CLOUD_STORAGE_MIGRATION_GUIDE.md`
4. **`Payments:UseTestMode`** — document in payment integration when provider SDK is added
5. **gitleaks** in PR pipeline — recommended, not yet enforced

---

## Verification

```powershell
cd Backend
dotnet test src/LogoDesignPortal.API.IntegrationTests -c Release `
  --filter "FullyQualifiedName~EnvironmentAppSettings|FullyQualifiedName~ProductionConfiguration"
```

```powershell
cd Frontend
npm run build:staging
npm run build:production
```
