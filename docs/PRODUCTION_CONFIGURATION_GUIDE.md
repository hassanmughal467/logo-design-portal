# Production Configuration Guide

**Canonical API:** `Backend/src/LogoDesignPortal.API/`  
**Never deploy:** `Backend/LogoDesignPortal.API/` (legacy)

---

## Prerequisites

- .NET 8 Hosting Bundle on Windows Server
- IIS + URL Rewrite + WebSockets enabled
- MySQL 8 (UTF8MB4)
- **Redis 6+** (mandatory in Production — rate limits, SignalR backplane, Hangfire, cache epochs)
- TLS certificate for admin + API hostnames

---

## Environment matrix

| Setting | Development | Staging | Production |
|---------|-------------|---------|------------|
| `ASPNETCORE_ENVIRONMENT` | Development | Staging | Production |
| Secrets | User Secrets | IIS / env vars | IIS / Key Vault / env vars |
| `ConnectionStrings__Redis` | Optional | Required* | **Required** |
| `Database:RunAfterStartup` | true (default) | false recommended | **false** (set in `appsettings.Production.json`) |
| `IncludeExceptionDetailsInProduction` | n/a | **false** | **false** |
| `Scalability:AllowInMemoryFallback` | true | false recommended | **false** |
| Swagger | On | Off | Off |
| CORS | localhost + dev IPs | HTTPS prod domains only | HTTPS prod domains only |

\* Staging may use kill switches in `ProductionSafety` section.

---

## Required environment variables (Production)

| Variable | Purpose |
|----------|---------|
| `ConnectionStrings__DefaultConnection` | MySQL connection (not `REPLACE_IN_*`) |
| `ConnectionStrings__Redis` | Redis — **startup fails without it** |
| `Jwt__Key` | Signing key ≥32 chars (empty in committed JSON) |
| `Storage__R2__AccountId`, `Storage__R2__AccessKeyId`, `Storage__R2__SecretAccessKey` | Cloudflare R2 — required when `Storage:Provider` is `R2` |
| `ExchangeRate__ApiKey` | USD→PKR rate API (optional; uses fallback if unset) |
| `Email__SmtpPassword` | Optional if SMTP used |
| `Email__FrontendUrl` | Password-reset links (now `https://admin.hawkmerchandising.com` in Production JSON) |

See `docs/SECRET_MANAGEMENT_GUIDE.md` for full mapping.

---

## IIS configuration

1. Set `ASPNETCORE_ENVIRONMENT=Production` in IIS Application **Environment Variables** (not only `web.config`).
2. Add connection string, JWT key, Redis, R2 credentials, and optional exchange-rate API key — **never commit secrets to git**.
3. Enable **WebSockets** for SignalR.
4. If behind ARR/reverse proxy: ensure forwarded headers; restrict who can send `X-Forwarded-*`.
5. `web.config`: 500MB `maxAllowedContentLength`; stdout logs under `api\logs` — rotate.
6. **CORS / login blocked in browser:** API `web.config` must remove IIS `OPTIONSVerbHandler` and `WebDAV` so OPTIONS preflight reaches ASP.NET Core. Redeploy API after pulling latest `web.config`. Verify:
   ```powershell
   curl -i -X OPTIONS "https://api.hawkmerchandising.com/api/auth/login" `
     -H "Origin: https://admin.hawkmerchandising.com" `
     -H "Access-Control-Request-Method: POST"
   ```
   Response must include `Access-Control-Allow-Origin: https://admin.hawkmerchandising.com` and `Access-Control-Allow-Credentials: true`.
7. Disable **Windows Authentication** on the API site (anonymous + ASP.NET Core JWT/cookies only).

---

## appsettings.Production.json (committed tuning only)

- `Database:RunAfterStartup: false` — apply migrations via scripted deploy
- `IncludeExceptionDetailsInProduction: false`
- `Scalability:AllowInMemoryFallback: false`
- **Action required:** Trim `Cors:AllowedOrigins` to production HTTPS admin URL only (file currently includes dev/LAN entries for operational convenience — remove before public launch)
- Set `AllowedHosts` to `admin.hawkmerchandising.com;api.hawkmerchandising.com` (recommended)

---

## File storage

- Production/staging use **Cloudflare R2** via `Storage:Provider` = `R2` and `Storage__R2__*` env vars
- Development uses local disk (`Storage:Provider` = `Local`, `Files_Dev`)
- Grant IIS app pool identity read/write only when using local paths; R2 uses API tokens
- Backup strategy: `ops/BACKUP_RUNBOOK.md` (MySQL); R2 versioning/lifecycle in Cloudflare console

---

## Auth cookies (SPA)

- Frontend `environment.prod.ts`: `useCookieAuth: true`, `apiUrl` matches live API
- Admin site must be HTTPS for `SameSite=None; Secure` cookies

---

## Observability

- Set `Observability:Enabled: true` and configure exporter when App Insights / OTel collector available
- Health: restrict `/health*` at reverse proxy in production

---

## Multi-instance checklist

Before second API node:

1. Redis reachable from all nodes
2. Shared file storage (SMB or future object storage)
3. `Database:RunAfterStartup: false` on all nodes
4. Sticky sessions **not** required (SignalR Redis backplane)

---

## Related documents

- `ENVIRONMENT_SETUP_GUIDE.md`
- `DEPLOYMENT_CHECKLIST.md`
- `docs/PRODUCTION-MIGRATION-SAFETY.md`
- `docs/REDIS_PRODUCTION_GUIDE.md`
