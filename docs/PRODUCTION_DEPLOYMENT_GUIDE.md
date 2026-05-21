# Production Deployment Guide

Deploy **Logo Design Portal** API and Angular SPA to IIS with production-safe configuration.

---

## Prerequisites

- Windows Server with IIS 10+ and ASP.NET Core 8 Hosting Bundle
- MySQL 8 (production database, dedicated user, least privilege)
- Redis 6+ (cache, rate limit, SignalR backplane, Hangfire)
- TLS certificates for `admin.hawkmerchandising.com` and `api.hawkmerchandising.com`
- Secrets in IIS environment variables or secret manager

---

## Build artifacts

### Option A — GitHub release workflow

Push tag `v1.2.3` or run `deploy-artifacts.yml` manually. Download:

- `LogoDesignPortal-API.zip`
- `LogoDesignPortal-Frontend.zip`

### Option B — Local publish

```powershell
dotnet publish Backend/src/LogoDesignPortal.API -c Release -o ./publish-api
cd Frontend
npm ci
npm run build:production
```

---

## API deployment

1. Create IIS site `api.hawkmerchandising.com` → physical path `publish-api`
2. App Pool: No Managed Code, dedicated identity
3. Set `web.config` environment variables:

| Variable | Required |
|----------|----------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | Yes |
| `ConnectionStrings__Redis` | Yes |
| `Jwt__Key` | Yes (unique, ≥32 chars) |

4. Create `Files` and `logs` folders; grant App Pool **Modify**
5. Enable **WebSockets**
6. Run migrations **before** or during window:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
# Connection string from secure store
dotnet ef database update --project Backend/src/LogoDesignPortal.Infrastructure --startup-project Backend/src/LogoDesignPortal.API
```

7. Start App Pool; verify no 500.30 in `logs\stdout`

---

## Frontend deployment

1. Unzip frontend build to `admin.hawkmerchandising.com` site root
2. Ensure `web.config` SPA rewrite is present
3. Confirm `environment.production.ts` API URL matches live API (`https://api.hawkmerchandising.com`)

---

## Production configuration highlights

| Setting | Value |
|---------|-------|
| `Database:RunAfterStartup` | `false` |
| `IncludeExceptionDetailsInProduction` | `false` |
| `Scalability:AllowInMemoryFallback` | `false` |
| `AllowedHosts` | hawkmerchandising domains only |
| CORS | HTTPS admin origin only |
| JWT access token | 30 minutes (production appsettings) |
| `Payments:UseTestMode` | `false` |

---

## Post-deploy

Execute in order:

1. `docs/DEPLOYMENT_CHECKLIST.md`
2. `docs/POST_DEPLOY_VALIDATION.md`
3. Monitor Hangfire `/hangfire` (Admin only)
4. Confirm `ProductionSafety` flags match business intent

---

## Rollback

Keep previous zip artifacts. See `ROLLBACK_GUIDE.md`.

---

## Troubleshooting

- **500.30:** missing connection string or JWT — see `PRODUCTION-TROUBLESHOOTING.md`
- **Redis errors:** connectivity, TLS, password
- **Upload 413:** IIS `maxAllowedContentLength` vs app limits
- **CORS:** origin must match `Cors:AllowedOrigins` HTTPS entries
