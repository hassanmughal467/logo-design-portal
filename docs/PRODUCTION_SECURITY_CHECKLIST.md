# Production Security Checklist

## Pre-deploy (required)

- [ ] `ASPNETCORE_ENVIRONMENT=Production`
- [ ] `Jwt__Key` — strong random ≥32 chars, not a placeholder
- [ ] `ConnectionStrings__DefaultConnection` — production MySQL, least privilege user
- [ ] `ConnectionStrings__Redis` — production Redis for SignalR/Hangfire/cache
- [ ] `IncludeExceptionDetailsInProduction=false`
- [ ] Swagger returns **404** (Development-only in `Program.cs`)
- [ ] CORS `Cors:AllowedOrigins` — production domains only
- [ ] HTTPS termination + `UseForwardedHeaders` behind reverse proxy
- [ ] Hangfire dashboard restricted (`HangfireDashboardAuthorizationFilter`)
- [ ] File storage path writable by app pool, outside web root

## Health / readiness

Probe **`/health/ready`** (ready-tagged checks):

| Check | Tag | Failure impact |
|-------|-----|----------------|
| `database_schema` | ready, db | Not ready — migrations incomplete |
| `hangfire` | ready | Degraded/unhealthy — background jobs |
| `redis` | ready | Required when configured |
| `file_storage` | ready | Uploads will fail |
| `disk_space` | ready | Low disk — risk of upload failures |
| `smtp` | ready | Degraded — email optional |

Liveness: **`/health`** (all checks).

## Auth

- [ ] Access token TTL ≤ 60 min production (30 min recommended)
- [ ] Refresh token rotation planned
- [ ] Account lockout enabled (existing `FailedLoginAttempts`)
- [ ] CSRF enabled when using cookie auth

## Uploads

- [ ] `ProductionSafety:DisableFileUploads` false unless incident
- [ ] IIS `maxAllowedContentLength` ≥ 500 MB if large uploads required
- [ ] Antivirus hook configured when available

## Logging / monitoring

- [ ] Serilog JSON to centralized sink (not raw console in prod)
- [ ] No logging of passwords, tokens, or full card data
- [ ] Correlation ID (`X-Correlation-Id`) forwarded from load balancer
- [ ] **Week 2:** Sentry / OpenTelemetry OTLP exporter wired

## Observability readiness: **65%**

Foundations in place (Serilog, correlation, health). Missing: APM dashboards, alerting, log aggregation.

## Launch readiness score: **72 / 100**

See `WEEK1_SECURITY_HARDENING.md` for prioritized backlog.
