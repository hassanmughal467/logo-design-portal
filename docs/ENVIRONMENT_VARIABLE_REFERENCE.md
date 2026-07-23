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

| Variable | Config key | Required | Notes |
|----------|------------|----------|-------|
| `Jwt__Key` | Signing key (≥32 chars) | Staging, Production | **Empty in committed `appsettings.json`** — set via env or User Secrets |
| `Jwt__PreviousKey` | Prior signing key (rotation) | Optional | Used during key rotation window |
| `Jwt__KeyVersion` | Key version label | Optional | Default: `v1` |
| `Jwt__Issuer` | Issuer | Optional | Default in JSON |
| `Jwt__Audience` | Audience | Optional | Default in JSON |

---

## CORS

| Variable | Config key | Notes |
|----------|------------|-------|
| `Cors__AllowedOrigins__0` | First origin | Index 0..N for array |
| `Cors__AllowedOrigins__1` | Second origin | Staging/Production HTTPS only |

---

## Email (SMTP)

| Variable | Config key | Notes |
|----------|------------|-------|
| `Email__SmtpServer` | SMTP host | |
| `Email__SmtpPort` | Port | |
| `Email__SmtpUsername` | Username | |
| `Email__SmtpPassword` | Password | **Secret — empty in committed JSON** |
| `Email__FromEmail` | From address | |
| `Email__FrontendUrl` | Password reset links | |

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

## File storage (legacy path)

| Variable | Config key | Default per env |
|----------|------------|-----------------|
| `FileStorage__Path` | Upload root (legacy health checks / orphan cleanup) | Dev: `Files_Dev`, Staging: `Files_Staging`, Prod: `Files` |

Override only when using dedicated volumes (document in runbook).

---

## Object storage (`Storage` section)

Used by `IFileStorageService` for uploads, previews, and finals. **Staging and Production use Cloudflare R2** (`Storage:Provider` = `R2`); Development defaults to local disk.

| Variable | Config key | Required | Notes |
|----------|------------|----------|-------|
| `Storage__Provider` | `Local` or `R2` | Staging, Production | Production/Staging JSON sets `R2` |
| `Storage__R2__AccountId` | Cloudflare account ID | When Provider=R2 | From Cloudflare dashboard |
| `Storage__R2__AccessKeyId` | R2 API token access key | When Provider=R2 | **Secret — never commit** |
| `Storage__R2__SecretAccessKey` | R2 API token secret | When Provider=R2 | **Secret — never commit** |
| `Storage__R2__BucketName` | Bucket name | When Provider=R2 | Default: `<r2-bucket-name>` |
| `Storage__R2__PublicDomain` | Optional CDN domain | No | Leave empty to serve only via authorized API |
| `Storage__Local__BasePath` | Local disk root | When Provider=Local | Dev default: `Files_Dev` |

**Example (IIS / secret manager — do not commit real values):**

```
Storage__Provider=R2
Storage__R2__AccountId=<cloudflare-account-id>
Storage__R2__AccessKeyId=<from-vault>
Storage__R2__SecretAccessKey=<from-vault>
Storage__R2__BucketName=<r2-bucket-name>
```

---

## Exchange rate (USD → PKR)

Used by `ICurrencyService` for invoice PKR conversion and analytics. When the API is unavailable, the service falls back to Redis cache, then `ExchangeRate:FallbackRate`.

| Variable | Config key | Required | Notes |
|----------|------------|----------|-------|
| `ExchangeRate__ApiKey` | exchangerate-api.com key | Recommended | **Secret — empty in all committed JSON**; uses hardcoded fallback if unset |
| `ExchangeRate__BaseCurrency` | Source currency | No | Default: `USD` |
| `ExchangeRate__TargetCurrency` | Target currency | No | Default: `PKR` |
| `ExchangeRate__CacheTtlMinutes` | Redis cache TTL | No | Default: `60` |
| `ExchangeRate__FallbackRate` | Last-resort rate | No | Default: `280.0` |

**Example:**

```
ExchangeRate__ApiKey=<from-vault>
ExchangeRate__FallbackRate=280
```

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
<environmentVariable name="Storage__Provider" value="R2" />
<environmentVariable name="Storage__R2__AccountId" value="..." />
<environmentVariable name="Storage__R2__AccessKeyId" value="..." />
<environmentVariable name="Storage__R2__SecretAccessKey" value="..." />
<environmentVariable name="Storage__R2__BucketName" value="<r2-bucket-name>" />
<environmentVariable name="ExchangeRate__ApiKey" value="..." />
```

See `Backend/src/LogoDesignPortal.API/web.config.staging.example.xml`.

---

## Development User Secrets

```powershell
cd Backend\src\LogoDesignPortal.API
dotnet user-secrets set "Jwt:Key" "<random-32+-chars>"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=127.0.0.1;..."
dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379"
# Optional — only when testing R2 or live exchange rates locally:
dotnet user-secrets set "Storage:Provider" "R2"
dotnet user-secrets set "Storage:R2:AccountId" "<account-id>"
dotnet user-secrets set "Storage:R2:AccessKeyId" "<key>"
dotnet user-secrets set "Storage:R2:SecretAccessKey" "<secret>"
dotnet user-secrets set "ExchangeRate:ApiKey" "<api-key>"
```

Project ID: `logo-design-portal-api-dev` (in `.csproj`).
