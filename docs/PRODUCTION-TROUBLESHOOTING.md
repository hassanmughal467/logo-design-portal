# Production 500 Error Troubleshooting Guide

When you see **500 Internal Server Error** on endpoints like `/api/Invoices/`, `/api/ClientPricing/`, or `/api/analytics/overview` after deployment, follow these steps.

> **Migration safety:** For keeping production data safe when applying schema changes, see [PRODUCTION-MIGRATION-SAFETY.md](./PRODUCTION-MIGRATION-SAFETY.md).

---

## 1. Check Health Endpoint First

**URL:** `https://api.hawkmerchandising.com/api/system/health`

This endpoint is **anonymous** (no auth required) and returns:

```json
{
  "database": "ok",
  "databaseError": null,
  "signalR": "ok",
  "storage": "ok"
}
```

- **If `database` is `"error"`** → Check `databaseError` for the message. Usually:
  - **Connection refused** → MySQL not running or wrong host/port
  - **Access denied** → Wrong username/password in connection string
  - **Table doesn't exist** → Migrations not applied (see Step 2)

- **If `database` is `"ok"`** → DB and tables are fine. The 500 is likely a bug in a specific service (check backend logs).

---

## 2. Apply Migrations on Production

### Option A: Use SQL Script (recommended for IIS)

Run the complete migration script on your production MySQL database:

```bash
mysql -h YOUR_DB_HOST -u YOUR_USER -p LogoDesignPortalDb < Backend/scripts/ApplyAllMigrations.sql
```

**Script location:** `Backend/scripts/ApplyAllMigrations.sql` (idempotent — safe to run multiple times. Skips already-applied migrations.)

See `Backend/scripts/README-MIGRATION.md` for full instructions.

### Option B: Use dotnet ef (if .NET SDK is available)

```bash
cd Backend/src/LogoDesignPortal.API
dotnet ef database update --project ../LogoDesignPortal.Infrastructure
```

**Important:** Use the **production connection string** (set via environment variable or `appsettings.Production.json` override).

Example with env var:
```bash
$env:ConnectionStrings__DefaultConnection="Server=YOUR_DB_HOST;Database=LogoDesignPortalDb;User=YOUR_USER;Password=YOUR_PASSWORD;"
dotnet ef database update --project ../LogoDesignPortal.Infrastructure
```

---

## 3. Verify Production Configuration

| Setting | Where | Notes |
|--------|-------|-------|
| `ConnectionStrings:DefaultConnection` | appsettings.Production.json or env | Must point to production MySQL |
| `Jwt:Key` | appsettings.Production.json or env | Min 32 chars; must match frontend token validation |
| `Jwt:Issuer` / `Jwt:Audience` | Same | Must match token generation |
| `Cors:AllowedOrigins` | appsettings.Production.json | Must include `https://admin.hawkmerchandising.com` |

---

## 4. Check Backend Logs

The API logs full exception details when a 500 occurs. Look for:

```
Unhandled exception: {Message}. Path: {Path}, Method: {Method}
```

- **Path** tells you which endpoint failed
- **Message** often indicates the cause (e.g. `Object reference not set`, `Table 'X' doesn't exist`)

**Where logs go:** Depends on hosting (IIS, Kestrel, Docker). Check:
- IIS: Event Viewer, or log file path in `web.config` / hosting config
- Kestrel: Console output or configured sink (Serilog, etc.)

---

## 5. Common Causes of 500 on These Endpoints

| Endpoint | Typical Cause |
|----------|----------------|
| `/api/invoices` | Missing `Invoices` table; null ref in `InvoiceService` |
| `/api/client-logo-pricing/*` | Missing `ClientLogoPricings` table; wrong `ClientId` format |
| `/api/admin/analytics/overview` | Missing analytics tables; DB query error |

---

## 6. Frontend API URL

Ensure production frontend uses the correct API URL:

**File:** `Frontend/src/environments/environment.prod.ts`

```ts
export const environment = {
  production: true,
  apiUrl: 'https://api.hawkmerchandising.com',  // Use HTTPS in production
  apiVersion: ''
};
```

Rebuild the frontend after changing this.

---

## 7. Quick Checklist Before Deploy

- [ ] Migrations applied on production DB
- [ ] `appsettings.Production.json` or env vars have correct connection string
- [ ] JWT Key, Issuer, Audience configured
- [ ] CORS includes `https://admin.hawkmerchandising.com`
- [ ] Health check returns `database: "ok"`
