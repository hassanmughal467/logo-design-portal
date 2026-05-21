# Environment Variable Reference

ASP.NET Core maps `Section__Key` environment variables to nested configuration. Values below apply to **LogoDesignPortal.API** (`Backend/src/`).

---

## Core

| Variable | Config key | Required | Environments |
|----------|------------|----------|--------------|
| `ASPNETCORE_ENVIRONMENT` | Hosting | Yes | All |
| `ASPNETCORE_URLS` | URLs | Optional | Dev |

---

## Database

| Variable | Config key | Required |
|----------|------------|----------|
| `ConnectionStrings__DefaultConnection` | MySQL connection | Staging, Production |

**Example (do not commit real passwords):**

```
Server=staging-db.internal;Port=3306;Database=LogoDesignPortalDb_Staging;User=ldp_staging;Password=<from-vault>;
```

---

## Redis

| Variable | Config key | Required |
|----------|------------|----------|
| `ConnectionStrings__Redis` | Redis | Staging, Production |
| `Redis__Configuration` | Alternate Redis string | Optional |

**Example:**

```
staging-redis:6379,abortConnect=false,ssl=true,password=<from-vault>
```

---

## JWT

| Variable | Config key | Required |
|----------|------------|----------|
| `Jwt__Key` | Signing key (≥32 chars) | Staging, Production |
| `Jwt__Issuer` | Issuer | Optional override |
| `Jwt__Audience` | Audience | Optional override |

---

## CORS

| Variable | Config key | Notes |
|----------|------------|-------|
| `Cors__AllowedOrigins__0` | First origin | Index 0..N for array |
| `Cors__AllowedOrigins__1` | Second origin | Staging/Production HTTPS only |

---

## Email (SMTP)

| Variable | Config key |
|----------|------------|
| `Email__SmtpServer` | SMTP host |
| `Email__SmtpPort` | Port |
| `Email__SmtpUsername` | Username |
| `Email__SmtpPassword` | Password |
| `Email__FromEmail` | From address |
| `Email__FrontendUrl` | Password reset links |

---

## Scalability & ops

| Variable | Config key | Notes |
|----------|------------|-------|
| `Scalability__AllowInMemoryFallback` | `true`/`false` | **Never** `true` in Production |
| `DISABLE_RATE_LIMIT` | N/A (middleware) | `true` disables limits (emergency only) |

---

## Observability

| Variable | Config key |
|----------|------------|
| `Observability__Enabled` | Enable OTLP |
| `Observability__OtlpEndpoint` | OTLP URL |
| `Observability__ApplicationInsightsConnectionString` | App Insights |

---

## File storage

| Variable | Config key | Default per env |
|----------|------------|-----------------|
| `FileStorage__Path` | Upload root | Dev: `Files_Dev`, Staging: `Files_Staging`, Prod: `Files` |

Override only when using dedicated volumes (document in runbook).

---

## Frontend (Playwright / CI)

| Variable | Purpose |
|----------|---------|
| `E2E_BASE_URL` | Angular dev server (default `http://localhost:4200`) |
| `E2E_API_URL` | API for API tests (default `http://localhost:5000`) |
| `STAGING_API_URL` | Staging smoke tests |

---

## IIS `web.config` example

```xml
<environmentVariable name="ConnectionStrings__DefaultConnection" value="..." />
<environmentVariable name="ConnectionStrings__Redis" value="..." />
<environmentVariable name="Jwt__Key" value="..." />
```

See `Backend/src/LogoDesignPortal.API/web.config.staging.example.xml`.

---

## Development User Secrets

```powershell
cd Backend\src\LogoDesignPortal.API
dotnet user-secrets set "Jwt:Key" "<random-32+-chars>"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=127.0.0.1;..."
dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379"
```

Project ID: `logo-design-portal-api-dev` (in `.csproj`).
