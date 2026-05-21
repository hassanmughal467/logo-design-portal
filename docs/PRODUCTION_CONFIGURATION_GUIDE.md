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
| `Jwt__Key` | Signing key ≥32 chars |
| `Email__SmtpPassword` | Optional if SMTP used |
| `Email__FrontendUrl` | Password-reset links (now `https://admin.hawkmerchandising.com` in Production JSON) |

See `docs/SECRET_MANAGEMENT_GUIDE.md` for full mapping.

---

## IIS configuration

1. Set `ASPNETCORE_ENVIRONMENT=Production` in IIS Application **Environment Variables** (not only `web.config`).
2. Add connection string, JWT key, Redis — **never commit secrets to git**.
3. Enable **WebSockets** for SignalR.
4. If behind ARR/reverse proxy: ensure forwarded headers; restrict who can send `X-Forwarded-*`.
5. `web.config`: 500MB `maxAllowedContentLength`; stdout logs under `api\logs` — rotate.

---

## appsettings.Production.json (committed tuning only)

- `Database:RunAfterStartup: false` — apply migrations via scripted deploy
- `IncludeExceptionDetailsInProduction: false`
- `Scalability:AllowInMemoryFallback: false`
- **Action required:** Trim `Cors:AllowedOrigins` to production HTTPS admin URL only (file currently includes dev/LAN entries for operational convenience — remove before public launch)
- Set `AllowedHosts` to `admin.hawkmerchandising.com;api.hawkmerchandising.com` (recommended)

---

## File storage

- Use absolute path outside publish directory for `FileStorage:Path`
- Grant IIS app pool identity read/write
- Backup strategy: `docs/BACKUP_STRATEGY.md`

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
