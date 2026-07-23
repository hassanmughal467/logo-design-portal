# Staging Environment Setup Guide

Production-like pre-production environment for UAT, integration testing, and release validation.

---

## Architecture goals

- **Parity:** Same stack as production (IIS, HTTPS, MySQL, Redis, SignalR, Hangfire)
- **Isolation:** Separate DB, Redis key prefixes, upload folder (`Files_Staging`), staging hostnames
- **Safety:** Billing/payout kill switches, test payment mode, no committed secrets

---

## Backend (`ASPNETCORE_ENVIRONMENT=Staging`)

### 1. Host prerequisites

| Service | Requirement |
|---------|-------------|
| IIS 10+ | ASP.NET Core Hosting Bundle 8.x |
| MySQL 8 | Database `LogoDesignPortalDb_Staging` (dedicated user) |
| Redis 6+ | Dedicated instance or logical DB index |
| TLS | Certificates for `staging-api` and `staging-admin` hostnames |

### 2. Configuration files

Uses `appsettings.json` + `appsettings.Staging.json`. **All secrets via environment variables** (see `ENVIRONMENT_VARIABLE_REFERENCE.md`).

Required IIS / process environment variables:

| Variable | Purpose |
|----------|---------|
| `ASPNETCORE_ENVIRONMENT` | `Staging` |
| `ConnectionStrings__DefaultConnection` | MySQL staging connection |
| `ConnectionStrings__Redis` | Redis (e.g. `staging-redis:6379,abortConnect=false`) |
| `Jwt__Key` | ≥32 character random signing key (unique from production) |
| `Email__SmtpPassword` | Optional staging SMTP |

### 3. IIS deployment

1. Publish API: `dotnet publish Backend/src/LogoDesignPortal.API -c Release -o ./publish-staging-api`
2. Copy `web.config.staging.example.xml` → site `web.config` and replace `REPLACE_AT_DEPLOY_TIME` values
3. Grant App Pool identity **Modify** on `Files_Staging` and `logs`
4. Enable **WebSocket** protocol for SignalR
5. Set `maxAllowedContentLength` ≥ 500 MB (template included)

### 4. Database

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Staging"
# Set ConnectionStrings__DefaultConnection first
dotnet ef database update --project Backend/src/LogoDesignPortal.Infrastructure --startup-project Backend/src/LogoDesignPortal.API
```

`Database:RunAfterStartup` is **false** — migrations are deliberate.

### 5. Redis & SignalR

- Cache, rate limits, Hangfire (`hangfire:ldp:`), SignalR backplane (`ldp:signalr`) share Redis
- Use a **different Redis database index** or instance than production

### 6. Production safety

`ProductionSafety` in staging disables billing generation and designer payout by default. Enable only for controlled UAT windows.

### 7. Payments

`Payments:UseTestMode: true` in staging appsettings — wire payment provider sandbox keys via env when integrated.

---

## Frontend

Build with staging API URL baked in:

```powershell
cd Frontend
npm ci
npm run build:staging
```

Deploy `dist/logo-design-portal-frontend` to IIS `staging-admin` site with `web.config` SPA rewrite.

**API URL:** `https://api.hawkmerchandising.com` (`environment.staging.ts`)

---

## Smoke validation

After deploy, run:

```powershell
curl -s https://api.hawkmerchandising.com/health/live
curl -s https://api.hawkmerchandising.com/health/ready
```

Playwright (optional, requires env):

```powershell
$env:STAGING_API_URL = "https://api.hawkmerchandising.com"
cd Frontend
npx playwright test e2e/tests/smoke/staging-health.spec.ts
```

See `STAGING_DEPLOYMENT_CHECKLIST.md` and `POST_DEPLOY_VALIDATION.md`.

---

## Local staging simulation

Not recommended for daily dev. To validate staging config locally:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Staging"
$env:ConnectionStrings__DefaultConnection = "Server=127.0.0.1;..."
$env:ConnectionStrings__Redis = "localhost:6379"
$env:Jwt__Key = "<32+ char random>"
dotnet run --project Backend/src/LogoDesignPortal.API
```

Startup will fail without Redis and real connection string (by design).
